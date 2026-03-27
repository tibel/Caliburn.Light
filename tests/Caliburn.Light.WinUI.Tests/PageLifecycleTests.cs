using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using TUnit.Core;
using TUnit.Core.Executors;

namespace Caliburn.Light.WinUI.Tests;

public class ScreenPage : Page
{
    public ScreenPage() { DataContext = new TestScreen(); }
}

public class AltScreenPage : Page
{
    public AltScreenPage() { DataContext = new TestScreen(); }
}

public class CloseGuardPage : Page
{
    public CloseGuardPage() { DataContext = new CloseGuardScreen { AllowClose = false }; }
}

public class EmptyPage : Page { }

[TestExecutor<WinUITestExecutor>]
public class PageLifecycleTests
{
    [Test]
    public async Task Constructor_SetsNavigationService()
    {
        var frame = new Frame();
        var locator = new StubLocator();
        var lifecycle = new PageLifecycle(frame, "ctx", locator);
        await Assert.That(ReferenceEquals(lifecycle.NavigationService, frame)).IsTrue();
    }

    [Test]
    public async Task Constructor_SetsContext()
    {
        var frame = new Frame();
        var locator = new StubLocator();
        var lifecycle = new PageLifecycle(frame, "myContext", locator);
        await Assert.That(lifecycle.Context).IsEqualTo("myContext");
    }

    [Test]
    public async Task Constructor_NullContext_IsNull()
    {
        var frame = new Frame();
        var locator = new StubLocator();
        var lifecycle = new PageLifecycle(frame, null, locator);
        await Assert.That(lifecycle.Context).IsNull();
    }

    [Test]
    public async Task Constructor_SetsFrameDataContextToNull()
    {
        var frame = new Frame();
        frame.DataContext = "initial";
        var locator = new StubLocator();
        _ = new PageLifecycle(frame, null, locator);
        await Assert.That(frame.DataContext).IsNull();
    }

    [Test]
    public async Task Dispose_DoesNotThrow()
    {
        var action = () =>
        {
            var frame = new Frame();
            var locator = new StubLocator();
            var lifecycle = new PageLifecycle(frame, null, locator);
            lifecycle.Dispose();
        };

        await Assert.That(action).ThrowsNothing();
    }

    [Test]
    public async Task TypeContract_IsSealed()
    {
        await Assert.That(typeof(PageLifecycle).IsSealed).IsTrue();
    }

    [Test]
    public async Task TypeContract_ImplementsIDisposable()
    {
        await Assert.That(typeof(IDisposable).IsAssignableFrom(typeof(PageLifecycle))).IsTrue();
    }

    private static async Task<(Frame frame, Window window)> CreateNavigationFrame()
    {
        var frame = new Frame();
        var window = new Window { Content = frame };
        var loaded = new TaskCompletionSource();
        frame.Loaded += (s, e) => loaded.TrySetResult();
        window.Activate();
        await loaded.Task.WaitAsync(TimeSpan.FromSeconds(5));
        return (frame, window);
    }

    [Test]
    public async Task Navigate_ActivatesViewModel()
    {
        var (frame, window) = await CreateNavigationFrame();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(frame, null, locator);

        frame.Navigate(typeof(ScreenPage));

        var screen = (TestScreen)((Page)frame.Content).DataContext;
        await Assert.That(screen.IsActive).IsTrue();
        window.Close();
    }

    [Test]
    public async Task Navigate_AttachesView()
    {
        var (frame, window) = await CreateNavigationFrame();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(frame, "ctx", locator);

        frame.Navigate(typeof(ScreenPage));

        var page = (Page)frame.Content;
        var screen = (TestScreen)page.DataContext;
        var attachedView = ((IViewAware)screen).GetView("ctx");
        await Assert.That(attachedView).IsSameReferenceAs(page);
        window.Close();
    }

    [Test]
    public async Task Navigate_Away_DeactivatesViewModel()
    {
        var (frame, window) = await CreateNavigationFrame();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(frame, null, locator);

        frame.Navigate(typeof(ScreenPage));
        var firstScreen = (TestScreen)((Page)frame.Content).DataContext;
        await Assert.That(firstScreen.IsActive).IsTrue();

        frame.Navigate(typeof(AltScreenPage));
        await Assert.That(firstScreen.IsActive).IsFalse();
        window.Close();
    }

    [Test]
    public async Task Navigate_Away_DetachesView()
    {
        var (frame, window) = await CreateNavigationFrame();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(frame, "ctx", locator);

        frame.Navigate(typeof(ScreenPage));
        var firstPage = (Page)frame.Content;
        var firstScreen = (TestScreen)firstPage.DataContext;

        frame.Navigate(typeof(AltScreenPage));

        var detachedView = ((IViewAware)firstScreen).GetView("ctx");
        await Assert.That(detachedView).IsNull();
        window.Close();
    }

    [Test]
    public async Task Navigate_WithNullDataContext_UsesLocator()
    {
        var (frame, window) = await CreateNavigationFrame();
        var vm = new TestScreen();
        var locator = new StubLocator(vm);
        using var lifecycle = new PageLifecycle(frame, null, locator);

        frame.Navigate(typeof(EmptyPage));

        var page = (Page)frame.Content;
        await Assert.That(page.DataContext).IsSameReferenceAs(vm);
        window.Close();
    }

    [Test]
    public async Task Navigate_SetsViewModelLocatorOnPage()
    {
        var (frame, window) = await CreateNavigationFrame();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(frame, null, locator);

        frame.Navigate(typeof(EmptyPage));

        var page = (Page)frame.Content;
        await Assert.That(View.GetViewModelLocator(page)).IsSameReferenceAs(locator);
        window.Close();
    }

    [Test]
    public async Task Navigate_SecondPage_ActivatesSecond()
    {
        var (frame, window) = await CreateNavigationFrame();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(frame, null, locator);

        frame.Navigate(typeof(ScreenPage));
        frame.Navigate(typeof(AltScreenPage));

        var secondScreen = (TestScreen)((Page)frame.Content).DataContext;
        await Assert.That(secondScreen.IsActive).IsTrue();
        window.Close();
    }

    private sealed class StubLocator : IViewModelLocator
    {
        private readonly object? _viewModel;
        public StubLocator(object? viewModel = null) => _viewModel = viewModel;
        public UIElement LocateForModel(object model, string? context) => new TextBlock();
        public object? LocateForView(UIElement view) => _viewModel;
    }
}
