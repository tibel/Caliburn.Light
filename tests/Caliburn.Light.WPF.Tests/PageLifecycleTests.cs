using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using TUnit.Core.Executors;

namespace Caliburn.Light.WPF.Tests;

[TestExecutor<WpfTestExecutor>]
public class PageLifecycleTests
{
    [Test]
    public async Task Constructor_SetsNavigationService()
    {
        var frame = new Frame();
        var locator = new StubLocator();
        var lifecycle = new PageLifecycle(frame.NavigationService, "ctx", locator);

        await Assert.That(ReferenceEquals(lifecycle.NavigationService, frame.NavigationService)).IsTrue();
    }

    [Test]
    public async Task Constructor_SetsContext()
    {
        var frame = new Frame();
        var locator = new StubLocator();
        var lifecycle = new PageLifecycle(frame.NavigationService, "myCtx", locator);

        await Assert.That(lifecycle.Context).IsEqualTo("myCtx");
    }

    [Test]
    public async Task Constructor_NullContext_IsNull()
    {
        var frame = new Frame();
        var locator = new StubLocator();
        var lifecycle = new PageLifecycle(frame.NavigationService, null, locator);

        await Assert.That(lifecycle.Context).IsNull();
    }

    [Test]
    public async Task Dispose_DoesNotThrow()
    {
        var frame = new Frame();
        var locator = new StubLocator();
        var lifecycle = new PageLifecycle(frame.NavigationService, null, locator);
        lifecycle.Dispose();

        await Assert.That(lifecycle.NavigationService).IsNotNull();
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

    [Test]
    public async Task Navigate_ToPage_SetsViewModelLocator()
    {
        var frame = new Frame();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(frame.NavigationService, null, locator);

        var page = new Page();
        frame.Navigate(page);
        await Dispatcher.Yield(DispatcherPriority.Background);

        var setLocator = View.GetViewModelLocator(page);
        await Assert.That(ReferenceEquals(setLocator, locator)).IsTrue();
    }

    [Test]
    public async Task Navigate_ToPage_WithScreen_ActivatesViewModel()
    {
        var frame = new Frame();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(frame.NavigationService, null, locator);

        var screen = new Screen();
        var page = new Page { DataContext = screen };
        frame.Navigate(page);
        await Dispatcher.Yield(DispatcherPriority.Background);

        await Assert.That(screen.IsActive).IsTrue();
    }

    [Test]
    public async Task Navigate_ToPage_WithScreen_AttachesView()
    {
        var frame = new Frame();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(frame.NavigationService, "ctx", locator);

        var screen = new Screen();
        var page = new Page { DataContext = screen };
        frame.Navigate(page);
        await Dispatcher.Yield(DispatcherPriority.Background);

        var attachedView = ((IViewAware)screen).GetView("ctx");
        await Assert.That(ReferenceEquals(attachedView, page)).IsTrue();
    }

    [Test]
    public async Task Navigate_ToPage_WithNullDataContext_ResolvesViaLocator()
    {
        var frame = new Frame();
        var expectedVm = new Screen();
        var locator = new StubLocator { ViewModelForView = expectedVm };
        using var lifecycle = new PageLifecycle(frame.NavigationService, null, locator);

        var page = new Page();
        frame.Navigate(page);
        await Dispatcher.Yield(DispatcherPriority.Background);

        await Assert.That(ReferenceEquals(page.DataContext, expectedVm)).IsTrue();
    }

    [Test]
    public async Task Navigate_FromPage_DeactivatesScreen()
    {
        var frame = new Frame();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(frame.NavigationService, null, locator);

        var screen = new Screen();
        var page1 = new Page { DataContext = screen };
        frame.Navigate(page1);
        await Dispatcher.Yield(DispatcherPriority.Background);
        await Assert.That(screen.IsActive).IsTrue();

        var page2 = new Page();
        frame.Navigate(page2);
        await Dispatcher.Yield(DispatcherPriority.Background);
        await Assert.That(screen.IsActive).IsFalse();
    }

    [Test]
    public async Task Navigate_FromPage_DetachesView()
    {
        var frame = new Frame();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(frame.NavigationService, "ctx", locator);

        var screen = new Screen();
        var page1 = new Page { DataContext = screen };
        frame.Navigate(page1);
        await Dispatcher.Yield(DispatcherPriority.Background);
        await Assert.That(((IViewAware)screen).GetView("ctx")).IsNotNull();

        var page2 = new Page();
        frame.Navigate(page2);
        await Dispatcher.Yield(DispatcherPriority.Background);
        await Assert.That(((IViewAware)screen).GetView("ctx")).IsNull();
    }

    [Test]
    public async Task Navigate_WithCloseGuard_DenyingClose_CancelsNavigation()
    {
        var frame = new Frame();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(frame.NavigationService, null, locator);

        var guard = new CloseGuardScreen { AllowClose = false };
        var page1 = new Page { DataContext = guard };
        frame.Navigate(page1);
        await Dispatcher.Yield(DispatcherPriority.Background);
        await Assert.That(guard.IsActive).IsTrue();

        // Navigation should be cancelled by close guard
        var page2 = new Page();
        frame.Navigate(page2);
        await Dispatcher.Yield(DispatcherPriority.Background);

        await Assert.That(guard.IsActive).IsTrue();
        await Assert.That(ReferenceEquals(frame.Content, page1)).IsTrue();
    }

    [Test]
    public async Task Navigate_WithCloseGuard_AllowingClose_Navigates()
    {
        var frame = new Frame();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(frame.NavigationService, null, locator);

        var guard = new CloseGuardScreen { AllowClose = true };
        var page1 = new Page { DataContext = guard };
        frame.Navigate(page1);
        await Dispatcher.Yield(DispatcherPriority.Background);
        await Assert.That(guard.IsActive).IsTrue();

        var page2 = new Page();
        frame.Navigate(page2);
        await Dispatcher.Yield(DispatcherPriority.Background);

        await Assert.That(guard.IsActive).IsFalse();
    }

    private sealed class StubLocator : IViewModelLocator
    {
        public object? ViewModelForView { get; set; }
        public UIElement LocateForModel(object model, string? context) => new TextBlock();
        public object? LocateForView(UIElement view) => ViewModelForView;
    }
}
