using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using TUnit.Core;
using TUnit.Core.Interfaces;

namespace Caliburn.Light.WinUI.Tests;

/// <summary>
/// TUnit test executor that dispatches test execution onto the WinUI UI thread.
/// Boots a minimal WinUI Application on a dedicated STA thread (once per process),
/// then dispatches each test body onto the DispatcherQueue.
/// Apply via [TestExecutor&lt;WinUITestExecutor&gt;] at assembly, class, or method level.
///
/// Key techniques:
/// - Win32 SendMessage(WM_CLOSE) triggers the full OS close path (AppWindow.Closing)
///   for ICloseGuard testing. WinUI's Window.Close() bypasses AppWindow.Closing.
/// - Wait for FrameworkElement.Loaded to obtain XamlRoot, which is needed for
///   ContentDialog.ShowAsync() and Popup. XamlRoot is null until the XAML tree loads.
/// - IXamlMetadataProvider on TestApp enables Frame.Navigate() for PageLifecycle tests.
/// </summary>
public class WinUITestExecutor : ITestExecutor
{
    internal static DispatcherQueue? _dispatcherQueue;
    internal static volatile bool _initialized;
    private static readonly object _lock = new();

    public async ValueTask ExecuteTest(TestContext context, Func<ValueTask> action)
    {
        EnsureInitialized();

        var dispatcher = _dispatcherQueue
            ?? throw new InvalidOperationException("WinUI DispatcherQueue is not available.");

        if (dispatcher.HasThreadAccess)
        {
            await action();
            return;
        }

        var tcs = new TaskCompletionSource();

#pragma warning disable TUnit0031 // async void is intentional — tcs bridges completion
        if (!dispatcher.TryEnqueue(DispatcherQueuePriority.Normal, async () =>
#pragma warning restore TUnit0031
        {
            try
            {
                await action();
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }))
        {
            throw new InvalidOperationException("Failed to enqueue test on WinUI DispatcherQueue.");
        }

        await tcs.Task;
    }

    private static void EnsureInitialized()
    {
        if (_initialized) return;

        lock (_lock)
        {
            if (_initialized) return;

            var ready = new ManualResetEventSlim(false);
            Exception? initError = null;

            var thread = new Thread(() =>
            {
                try
                {
                    WinRT.ComWrappersSupport.InitializeComWrappers();

                    Application.Start(p =>
                    {
                        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

                        var syncContext = new DispatcherQueueSynchronizationContext(_dispatcherQueue);
                        SynchronizationContext.SetSynchronizationContext(syncContext);

                        _ = new TestApp();

                        ready.Set();
                    });
                }
                catch (Exception ex)
                {
                    initError = ex;
                    ready.Set();
                }
            })
            {
                IsBackground = true,
                Name = "WinUI Test Thread"
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            if (!ready.Wait(TimeSpan.FromSeconds(15)))
                throw new TimeoutException("WinUI runtime failed to initialize within 15 seconds.");

            if (initError is not null)
                throw new InvalidOperationException("WinUI runtime initialization failed.", initError);

            _initialized = true;
        }
    }

    /// <summary>
    /// Minimal WinUI application for test hosting.
    /// Implements IXamlMetadataProvider to enable Frame.Navigate() for PageLifecycle tests.
    /// </summary>
    private class TestApp : Application, IXamlMetadataProvider
    {
        public IXamlType GetXamlType(Type type)
        {
            if (typeof(Page).IsAssignableFrom(type))
                return new SimpleXamlType(type);
            return null!;
        }

        public IXamlType GetXamlType(string fullName) => null!;
        public XmlnsDefinition[] GetXmlnsDefinitions() => [];

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Don't create any windows — tests will create UI elements as needed
        }
    }

    /// <summary>
    /// Minimal IXamlType that supports type activation for Frame.Navigate().
    /// </summary>
    private class SimpleXamlType : IXamlType
    {
        private readonly Type _type;
        public SimpleXamlType(Type type) => _type = type;

        public string FullName => _type.FullName!;
        public Type UnderlyingType => _type;
        public object ActivateInstance() => Activator.CreateInstance(_type)!;

        public IXamlType BaseType => null!;
        public IXamlType BoxedType => null!;
        public IXamlMember ContentProperty => null!;
        public bool IsArray => false;
        public bool IsBindable => false;
        public bool IsCollection => false;
        public bool IsConstructible => true;
        public bool IsDictionary => false;
        public bool IsMarkupExtension => false;
        public IXamlType ItemType => null!;
        public IXamlType KeyType => null!;
        public IXamlMember GetMember(string name) => null!;
        public void AddToMap(object instance, object key, object value) { }
        public void AddToVector(object instance, object value) { }
        public void RunInitializer() { }
        public object CreateFromString(string value) => null!;
    }
}
