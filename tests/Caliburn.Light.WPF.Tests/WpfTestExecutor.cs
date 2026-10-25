using System.Threading;
using System.Windows;
using System.Windows.Threading;
using TUnit.Core;
using TUnit.Core.Interfaces;

namespace Caliburn.Light.WPF.Tests;

/// <summary>
/// TUnit test executor that dispatches test execution onto the WPF UI thread.
/// Boots a minimal WPF Application on a dedicated STA thread once per process,
/// then dispatches each test onto that Dispatcher.
/// Apply via [TestExecutor&lt;WpfTestExecutor&gt;] at assembly, class, or method level.
/// </summary>
public sealed class WpfTestExecutor : ITestExecutor
{
    private static Dispatcher? _dispatcher;
    private static volatile bool _initialized;
    private static readonly Lock _lock = new();

    public async ValueTask ExecuteTest(TestContext context, Func<ValueTask> action)
    {
        EnsureInitialized();

        var dispatcher = _dispatcher
            ?? throw new InvalidOperationException("WPF Dispatcher is not available.");

        if (dispatcher.CheckAccess())
        {
            await action();
            return;
        }

        var tcs = new TaskCompletionSource();

#pragma warning disable TUnit0031 // async void is intentional - tcs bridges completion
        _ = dispatcher.InvokeAsync(async () =>
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
        });

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
                    var app = new TestApp { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                    _dispatcher = app.Dispatcher;
                    _dispatcher.InvokeAsync(() => ready.Set());
                    app.Run();
                }
                catch (Exception ex)
                {
                    initError = ex;
                    ready.Set();
                }
            })
            {
                IsBackground = true,
                Name = "WPF Test Thread"
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            if (!ready.Wait(TimeSpan.FromSeconds(15)))
                throw new TimeoutException("WPF Application failed to initialize within 15 seconds.");

            if (initError is not null)
                throw new InvalidOperationException("WPF Application initialization failed.", initError);

            _initialized = true;
        }
    }

    private sealed class TestApp : Application { }
}
