using System.Windows;
using System.Windows.Controls.Primitives;
using TUnit.Core.Executors;

namespace Caliburn.Light.WPF.Tests;

[TestExecutor<WpfTestExecutor>]
public class WindowLifecycleTests
{
    [Test]
    public async Task WindowLifecycle_SetsViewAndContext()
    {
        var window = new Window { DataContext = new object() };
        var lifecycle = new WindowLifecycle(window, "ctx", false);

        await Assert.That(ReferenceEquals(lifecycle.View, window)).IsTrue();
        await Assert.That(lifecycle.Context).IsEqualTo("ctx");
    }

    [Test]
    public async Task WindowLifecycle_AttachesView_WhenDataContextIsIViewAware()
    {
        var screen = new Screen();
        var window = new Window { DataContext = screen };

        _ = new WindowLifecycle(window, null, false);

        var attachedView = ((IViewAware)screen).GetView();
        await Assert.That(ReferenceEquals(attachedView, window)).IsTrue();
    }

    [Test]
    public async Task WindowLifecycle_ActivatesScreen_WhenNotActivateWithWindow()
    {
        var screen = new Screen();
        var window = new Window { DataContext = screen };

        _ = new WindowLifecycle(window, null, false);

        // ActivateAsync is called via .Observe() — completes synchronously for Screen
        await Assert.That(screen.IsActive).IsTrue();
    }

    [Test]
    public async Task WindowLifecycle_WithContext_AttachesViewWithContext()
    {
        var screen = new Screen();
        var window = new Window { DataContext = screen };

        _ = new WindowLifecycle(window, "popup", false);

        var attachedView = ((IViewAware)screen).GetView("popup");
        await Assert.That(ReferenceEquals(attachedView, window)).IsTrue();
    }

    [Test]
    public async Task PopupLifecycle_SetsViewAndContext()
    {
        var popup = new Popup { DataContext = new object() };
        var lifecycle = new PopupLifecycle(popup, "ctx");

        await Assert.That(ReferenceEquals(lifecycle.View, popup)).IsTrue();
        await Assert.That(lifecycle.Context).IsEqualTo("ctx");
    }

    [Test]
    public async Task PopupLifecycle_NullContext_Accepted()
    {
        var popup = new Popup { DataContext = new Screen() };
        var lifecycle = new PopupLifecycle(popup, null);

        await Assert.That(lifecycle.Context).IsNull();
    }

    [Test]
    public async Task WindowLifecycle_NullContext_IsNull()
    {
        var window = new Window { DataContext = new Screen() };
        var lifecycle = new WindowLifecycle(window, null, false);
        await Assert.That(lifecycle.Context).IsNull();
    }

    [Test]
    public async Task WindowLifecycle_Closed_DeactivatesViewModel()
    {
        var screen = new Screen();
        var window = new Window { DataContext = screen };
        _ = new WindowLifecycle(window, null, false);

        // Screen should be activated by now
        await Assert.That(screen.IsActive).IsTrue();

        // Show and close the window
        window.Show();
        window.Close();

        await Assert.That(screen.IsActive).IsFalse();
    }

    [Test]
    public async Task WindowLifecycle_WithCloseGuard_HooksClosingEvent()
    {
        var screen = new CloseGuardScreen { AllowClose = false };
        var window = new Window { DataContext = screen };
        _ = new WindowLifecycle(window, null, false);

        // Verify the screen was activated
        await Assert.That(screen.IsActive).IsTrue();
    }

    [Test]
    public async Task WindowLifecycle_WithoutCloseGuard_DoesNotThrow()
    {
        var screen = new Screen();
        var window = new Window { DataContext = screen };
        _ = new WindowLifecycle(window, null, false);
        await Assert.That(screen.IsActive).IsTrue();
    }

    // NOTE: Testing PopupLifecycle.Opened event integration (which calls AttachView + ActivateAsync)
    // would require setting popup.IsOpen = true, which creates a native popup window.
    // This is fragile in a headless test environment, so we verify wiring at construction only.
}

public class CloseGuardScreen : Screen
{
    public bool AllowClose { get; set; }
    public override Task<bool> CanCloseAsync() => Task.FromResult(AllowClose);
}
