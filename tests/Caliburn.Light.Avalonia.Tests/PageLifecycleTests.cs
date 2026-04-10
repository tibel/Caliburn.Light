using Avalonia.Controls;
using TUnit.Core.Executors;

namespace Caliburn.Light.Avalonia.Tests;

[TestExecutor<AvaloniaTestExecutor>]
public class PageLifecycleTests
{
    [Test]
    public async Task Constructor_SetsNavigationPage()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        var lifecycle = new PageLifecycle(navigationPage, null, locator);

        await Assert.That(ReferenceEquals(lifecycle.NavigationPage, navigationPage)).IsTrue();
    }

    [Test]
    public async Task Constructor_SetsContext()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        var lifecycle = new PageLifecycle(navigationPage, "myCtx", locator);

        await Assert.That(lifecycle.Context).IsEqualTo("myCtx");
    }

    [Test]
    public async Task Constructor_NullContext_IsNull()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        var lifecycle = new PageLifecycle(navigationPage, null, locator);

        await Assert.That(lifecycle.Context).IsNull();
    }

    [Test]
    public async Task Constructor_SetsNavigationPageDataContextToNull()
    {
        var navigationPage = new NavigationPage { DataContext = "initial" };
        var locator = new StubLocator();
        _ = new PageLifecycle(navigationPage, null, locator);

        await Assert.That(navigationPage.DataContext).IsNull();
    }

    [Test]
    public async Task Dispose_DoesNotThrow()
    {
        var action = () =>
        {
            var navigationPage = new NavigationPage();
            var locator = new StubLocator();
            var lifecycle = new PageLifecycle(navigationPage, null, locator);
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

    [Test]
    public async Task Push_SetsViewModelLocator()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, null, locator);

        var page = new ContentPage();
        await navigationPage.PushAsync(page);

        var setLocator = View.GetViewModelLocator(page);
        await Assert.That(ReferenceEquals(setLocator, locator)).IsTrue();
    }

    [Test]
    public async Task Push_ActivatesViewModel()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, null, locator);

        var screen = new TestScreen();
        var page = new ContentPage { DataContext = screen };
        await navigationPage.PushAsync(page);

        await Assert.That(screen.IsActive).IsTrue();
    }

    [Test]
    public async Task Push_AttachesView()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, "ctx", locator);

        var screen = new TestScreen();
        var page = new ContentPage { DataContext = screen };
        await navigationPage.PushAsync(page);

        var attachedView = ((IViewAware)screen).GetView("ctx");
        await Assert.That(ReferenceEquals(attachedView, page)).IsTrue();
    }

    [Test]
    public async Task Push_WithNullDataContext_ResolvesViaLocator()
    {
        var navigationPage = new NavigationPage();
        var expectedVm = new TestScreen();
        var locator = new StubLocator(expectedVm);
        using var lifecycle = new PageLifecycle(navigationPage, null, locator);

        var page = new ContentPage();
        await navigationPage.PushAsync(page);

        await Assert.That(ReferenceEquals(page.DataContext, expectedVm)).IsTrue();
    }

    [Test]
    public async Task Push_SecondPage_DeactivatesPrevious()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, null, locator);

        var screen1 = new TestScreen();
        var page1 = new ContentPage { DataContext = screen1 };
        await navigationPage.PushAsync(page1);
        await Assert.That(screen1.IsActive).IsTrue();

        var page2 = new ContentPage();
        await navigationPage.PushAsync(page2);

        await Assert.That(screen1.IsActive).IsFalse();
    }

    [Test]
    public async Task Push_SecondPage_DetachesPreviousView()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, "ctx", locator);

        var screen1 = new TestScreen();
        var page1 = new ContentPage { DataContext = screen1 };
        await navigationPage.PushAsync(page1);
        await Assert.That(((IViewAware)screen1).GetView("ctx")).IsNotNull();

        var page2 = new ContentPage();
        await navigationPage.PushAsync(page2);

        await Assert.That(((IViewAware)screen1).GetView("ctx")).IsNull();
    }

    [Test]
    public async Task Push_SecondPage_ActivatesSecond()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, null, locator);

        var page1 = new ContentPage();
        await navigationPage.PushAsync(page1);

        var screen2 = new TestScreen();
        var page2 = new ContentPage { DataContext = screen2 };
        await navigationPage.PushAsync(page2);

        await Assert.That(screen2.IsActive).IsTrue();
    }

    [Test]
    public async Task Pop_DeactivatesPopped()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, null, locator);

        var page1 = new ContentPage();
        await navigationPage.PushAsync(page1);

        var screen2 = new TestScreen();
        var page2 = new ContentPage { DataContext = screen2 };
        await navigationPage.PushAsync(page2);
        await Assert.That(screen2.IsActive).IsTrue();

        await navigationPage.PopAsync();

        await Assert.That(screen2.IsActive).IsFalse();
    }

    [Test]
    public async Task Pop_ActivatesRevealed()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, null, locator);

        var screen1 = new TestScreen();
        var page1 = new ContentPage { DataContext = screen1 };
        await navigationPage.PushAsync(page1);

        var page2 = new ContentPage();
        await navigationPage.PushAsync(page2);
        await Assert.That(screen1.IsActive).IsFalse();

        await navigationPage.PopAsync();

        await Assert.That(screen1.IsActive).IsTrue();
    }

    [Test]
    public async Task Pop_AttachesViewToRevealed()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, "ctx", locator);

        var screen1 = new TestScreen();
        var page1 = new ContentPage { DataContext = screen1 };
        await navigationPage.PushAsync(page1);

        var page2 = new ContentPage();
        await navigationPage.PushAsync(page2);
        await Assert.That(((IViewAware)screen1).GetView("ctx")).IsNull();

        await navigationPage.PopAsync();

        var attachedView = ((IViewAware)screen1).GetView("ctx");
        await Assert.That(ReferenceEquals(attachedView, page1)).IsTrue();
    }

    [Test]
    public async Task Pop_DetachesPoppedView()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, "ctx", locator);

        var page1 = new ContentPage();
        await navigationPage.PushAsync(page1);

        var screen2 = new TestScreen();
        var page2 = new ContentPage { DataContext = screen2 };
        await navigationPage.PushAsync(page2);
        await Assert.That(((IViewAware)screen2).GetView("ctx")).IsNotNull();

        await navigationPage.PopAsync();

        await Assert.That(((IViewAware)screen2).GetView("ctx")).IsNull();
    }

    [Test]
    public async Task CloseGuard_DenyingClose_CancelsPop()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, null, locator);

        var page1 = new ContentPage();
        await navigationPage.PushAsync(page1);

        var guard = new CloseGuardScreen { AllowClose = false };
        var page2 = new ContentPage { DataContext = guard };
        await navigationPage.PushAsync(page2);
        await Assert.That(guard.IsActive).IsTrue();

        await navigationPage.PopAsync();

        // Pop should be cancelled — page2 stays active on top
        await Assert.That(guard.IsActive).IsTrue();
        await Assert.That(navigationPage.NavigationStack.Count).IsEqualTo(2);
    }

    [Test]
    public async Task CloseGuard_AllowingClose_AllowsPop()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, null, locator);

        var page1 = new ContentPage();
        await navigationPage.PushAsync(page1);

        var guard = new CloseGuardScreen { AllowClose = true };
        var page2 = new ContentPage { DataContext = guard };
        await navigationPage.PushAsync(page2);
        await Assert.That(guard.IsActive).IsTrue();

        await navigationPage.PopAsync();

        await Assert.That(guard.IsActive).IsFalse();
        await Assert.That(navigationPage.NavigationStack.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Constructor_NullNavigationPage_Throws()
    {
        await Assert.That(() => new PageLifecycle(null!, null, new StubLocator()))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_NullLocator_Throws()
    {
        await Assert.That(() => new PageLifecycle(new NavigationPage(), null, null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task InsertPage_SubscribesCloseGuard()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, null, locator);

        var page1 = new ContentPage();
        await navigationPage.PushAsync(page1);

        // Insert a page with a close guard behind the current page
        var guard = new CloseGuardScreen { AllowClose = false };
        var guardPage = new ContentPage { DataContext = guard };
        navigationPage.InsertPage(guardPage, page1);

        // Pop page1 to reveal guardPage
        await navigationPage.PopAsync();
        await Assert.That(guard.IsActive).IsTrue();

        // Try to push on top of guardPage — close guard should prevent it
        var page3 = new ContentPage();
        await navigationPage.PushAsync(page3);

        // Push should be cancelled — guardPage's close guard denies navigation away
        await Assert.That(navigationPage.NavigationStack.Count).IsEqualTo(1);
        await Assert.That(guard.IsActive).IsTrue();
    }

    [Test]
    public async Task RemovePage_NonCurrent_CleansUpHandler()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, null, locator);

        var screen1 = new TestScreen();
        var page1 = new ContentPage { DataContext = screen1 };
        await navigationPage.PushAsync(page1);

        var screen2 = new TestScreen();
        var page2 = new ContentPage { DataContext = screen2 };
        await navigationPage.PushAsync(page2);

        // Remove page1 (non-current, in background)
        navigationPage.RemovePage(page1);

        await Assert.That(navigationPage.NavigationStack.Count).IsEqualTo(1);
        // page2 should still be active
        await Assert.That(screen2.IsActive).IsTrue();
    }

    [Test]
    public async Task RemovePage_Current_ActivatesRevealed()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, null, locator);

        var screen1 = new TestScreen();
        var page1 = new ContentPage { DataContext = screen1 };
        await navigationPage.PushAsync(page1);

        var screen2 = new TestScreen();
        var page2 = new ContentPage { DataContext = screen2 };
        await navigationPage.PushAsync(page2);
        await Assert.That(screen1.IsActive).IsFalse();

        // Remove current page
        navigationPage.RemovePage(page2);

        await Assert.That(screen2.IsActive).IsFalse();
        await Assert.That(screen1.IsActive).IsTrue();
    }

    [Test]
    public async Task Replace_DeactivatesOld_ActivatesNew()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, null, locator);

        var screen1 = new TestScreen();
        var page1 = new ContentPage { DataContext = screen1 };
        await navigationPage.PushAsync(page1);
        await Assert.That(screen1.IsActive).IsTrue();

        var screen2 = new TestScreen();
        var page2 = new ContentPage { DataContext = screen2 };
        await navigationPage.ReplaceAsync(page2);

        await Assert.That(screen1.IsActive).IsFalse();
        await Assert.That(screen2.IsActive).IsTrue();
    }

    [Test]
    public async Task Replace_DetachesOld_AttachesNew()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, "ctx", locator);

        var screen1 = new TestScreen();
        var page1 = new ContentPage { DataContext = screen1 };
        await navigationPage.PushAsync(page1);

        var screen2 = new TestScreen();
        var page2 = new ContentPage { DataContext = screen2 };
        await navigationPage.ReplaceAsync(page2);

        await Assert.That(((IViewAware)screen1).GetView("ctx")).IsNull();
        var attachedView = ((IViewAware)screen2).GetView("ctx");
        await Assert.That(ReferenceEquals(attachedView, page2)).IsTrue();
    }

    [Test]
    public async Task Replace_CloseGuard_DenyingClose_CancelsReplace()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, null, locator);

        var guard = new CloseGuardScreen { AllowClose = false };
        var page1 = new ContentPage { DataContext = guard };
        await navigationPage.PushAsync(page1);
        await Assert.That(guard.IsActive).IsTrue();

        var page2 = new ContentPage();
        await navigationPage.ReplaceAsync(page2);

        // Replace should be cancelled
        await Assert.That(guard.IsActive).IsTrue();
        await Assert.That(navigationPage.NavigationStack.Count).IsEqualTo(1);
        await Assert.That(ReferenceEquals(navigationPage.NavigationStack[0], page1)).IsTrue();
    }

    [Test]
    public async Task PopToRoot_DeactivatesAllIntermediatePages()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, null, locator);

        var screenA = new TestScreen();
        var pageA = new ContentPage { DataContext = screenA };
        await navigationPage.PushAsync(pageA);

        var screenB = new TestScreen();
        var pageB = new ContentPage { DataContext = screenB };
        await navigationPage.PushAsync(pageB);

        var screenC = new TestScreen();
        var pageC = new ContentPage { DataContext = screenC };
        await navigationPage.PushAsync(pageC);
        await Assert.That(screenC.IsActive).IsTrue();

        await navigationPage.PopToRootAsync();

        await Assert.That(screenC.IsActive).IsFalse();
        await Assert.That(screenB.IsActive).IsFalse();
        await Assert.That(screenA.IsActive).IsTrue();
    }

    [Test]
    public async Task PopToRoot_ReattachesViewToRoot()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, "ctx", locator);

        var screenA = new TestScreen();
        var pageA = new ContentPage { DataContext = screenA };
        await navigationPage.PushAsync(pageA);

        var pageB = new ContentPage();
        await navigationPage.PushAsync(pageB);

        var pageC = new ContentPage();
        await navigationPage.PushAsync(pageC);
        await Assert.That(((IViewAware)screenA).GetView("ctx")).IsNull();

        await navigationPage.PopToRootAsync();

        var attachedView = ((IViewAware)screenA).GetView("ctx");
        await Assert.That(ReferenceEquals(attachedView, pageA)).IsTrue();
    }

    [Test]
    public async Task Push_DeactivatesPreviousWithCloseFalse()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, null, locator);

        var screen1 = new TestScreen();
        var page1 = new ContentPage { DataContext = screen1 };
        await navigationPage.PushAsync(page1);

        bool? wasClosed = null;
        screen1.Deactivating += (_, e) => wasClosed = e.WasClosed;

        var page2 = new ContentPage();
        await navigationPage.PushAsync(page2);

        await Assert.That(wasClosed).IsFalse();
    }

    [Test]
    public async Task Pop_DeactivatesPoppedWithCloseTrue()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, null, locator);

        var page1 = new ContentPage();
        await navigationPage.PushAsync(page1);

        var screen2 = new TestScreen();
        var page2 = new ContentPage { DataContext = screen2 };
        await navigationPage.PushAsync(page2);

        bool? wasClosed = null;
        screen2.Deactivating += (_, e) => wasClosed = e.WasClosed;

        await navigationPage.PopAsync();

        await Assert.That(wasClosed).IsTrue();
    }

    [Test]
    public async Task Replace_CloseGuard_Denied_OldPageStaysTracked()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, null, locator);

        var guard = new CloseGuardScreen { AllowClose = false };
        var page1 = new ContentPage { DataContext = guard };
        await navigationPage.PushAsync(page1);

        var page2 = new ContentPage();
        await navigationPage.ReplaceAsync(page2);

        // Close guard still denies after the failed replace
        await Assert.That(guard.IsActive).IsTrue();

        // Verify the page is still tracked by attempting another guarded navigation
        guard.AllowClose = false;
        var page3 = new ContentPage();
        await navigationPage.PushAsync(page3);

        // Push should be cancelled — guard still active
        await Assert.That(navigationPage.NavigationStack.Count).IsEqualTo(1);
        await Assert.That(guard.IsActive).IsTrue();
    }

    [Test]
    public async Task Replace_ActivatesNewPageExactlyOnce()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, null, locator);

        var page1 = new ContentPage();
        await navigationPage.PushAsync(page1);

        var screen2 = new ActivationCountingScreen();
        var page2 = new ContentPage { DataContext = screen2 };
        await navigationPage.ReplaceAsync(page2);

        await Assert.That(screen2.ActivationCount).IsEqualTo(1);
    }

    [Test]
    public async Task Dispose_StopsLifecycleHandling()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        var lifecycle = new PageLifecycle(navigationPage, null, locator);

        var screen1 = new TestScreen();
        var page1 = new ContentPage { DataContext = screen1 };
        await navigationPage.PushAsync(page1);
        await Assert.That(screen1.IsActive).IsTrue();

        lifecycle.Dispose();

        // Push a second page — lifecycle should NOT deactivate screen1
        var page2 = new ContentPage();
        await navigationPage.PushAsync(page2);

        await Assert.That(screen1.IsActive).IsTrue();
    }

    [Test]
    public async Task RemovePage_MiddleOfStack_DoesNotDisruptActivePage()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, null, locator);

        var screenA = new TestScreen();
        var pageA = new ContentPage { DataContext = screenA };
        await navigationPage.PushAsync(pageA);

        var screenB = new TestScreen();
        var pageB = new ContentPage { DataContext = screenB };
        await navigationPage.PushAsync(pageB);

        var screenC = new TestScreen();
        var pageC = new ContentPage { DataContext = screenC };
        await navigationPage.PushAsync(pageC);
        await Assert.That(screenC.IsActive).IsTrue();

        // Remove middle page B
        navigationPage.RemovePage(pageB);

        await Assert.That(screenC.IsActive).IsTrue();
        await Assert.That(navigationPage.NavigationStack.Count).IsEqualTo(2);
    }

    [Test]
    public async Task PushAfterPop_ActivatesNewPage()
    {
        var navigationPage = new NavigationPage();
        var locator = new StubLocator();
        using var lifecycle = new PageLifecycle(navigationPage, null, locator);

        var screenA = new TestScreen();
        var pageA = new ContentPage { DataContext = screenA };
        await navigationPage.PushAsync(pageA);

        var pageB = new ContentPage();
        await navigationPage.PushAsync(pageB);

        await navigationPage.PopAsync();
        await Assert.That(screenA.IsActive).IsTrue();

        var screenC = new TestScreen();
        var pageC = new ContentPage { DataContext = screenC };
        await navigationPage.PushAsync(pageC);

        await Assert.That(screenA.IsActive).IsFalse();
        await Assert.That(screenC.IsActive).IsTrue();
    }

    private sealed class ActivationCountingScreen : Screen
    {
        public int ActivationCount { get; private set; }

        protected override Task OnActivateAsync()
        {
            ActivationCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class StubLocator : IViewModelLocator
    {
        private readonly object? _viewModel;
        public StubLocator(object? viewModel = null) => _viewModel = viewModel;
        public Control LocateForModel(object model, string? context) => new TextBlock();
        public object? LocateForView(Control view) => _viewModel;
    }
}
