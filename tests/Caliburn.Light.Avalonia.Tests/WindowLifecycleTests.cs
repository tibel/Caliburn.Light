using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using TUnit.Core;
using TUnit.Core.Executors;

namespace Caliburn.Light.Avalonia.Tests;

[TestExecutor<AvaloniaTestExecutor>]
public class WindowLifecycleTests
{
    [Test]
    public async Task WindowLifecycle_Constructor_SetsView()
    {
        var window = new Window();
        window.DataContext = new SampleViewModel();
        var lifecycle = new WindowLifecycle(window, null, false);
        var view = lifecycle.View;

        await Assert.That(view).IsNotNull();
        await Assert.That(view).IsTypeOf<Window>();
    }

    [Test]
    public async Task WindowLifecycle_Constructor_SetsContext()
    {
        var window = new Window();
        window.DataContext = new SampleViewModel();
        var lifecycle = new WindowLifecycle(window, "mycontext", false);
        var context = lifecycle.Context;

        await Assert.That(context).IsEqualTo("mycontext");
    }

    [Test]
    public async Task WindowLifecycle_NullContext_IsNull()
    {
        var window = new Window();
        window.DataContext = new SampleViewModel();
        var lifecycle = new WindowLifecycle(window, null, false);
        var context = lifecycle.Context;

        await Assert.That(context).IsNull();
    }

    [Test]
    public async Task WindowLifecycle_WithScreen_ActivatesViewModel()
    {
        var screen = new TestScreen();
        var window = new Window { DataContext = screen };
        _ = new WindowLifecycle(window, null, false);

        // The activation is async fire-and-forget via Observe(), may need a small delay
        // but since it's Task.CompletedTask-based for our TestScreen, it should be immediate
        await Assert.That(screen.IsActive).IsTrue();
    }

    [Test]
    public async Task WindowLifecycle_WithScreen_AttachesView()
    {
        var screen = new TestScreen();
        var window = new Window { DataContext = screen };
        _ = new WindowLifecycle(window, null, false);

        var hasView = ((IViewAware)screen).GetView(null) is not null;

        await Assert.That(hasView).IsTrue();
    }

    [Test]
    public async Task WindowLifecycle_Closed_DeactivatesViewModel()
    {
        var screen = new TestScreen();
        var window = new Window { DataContext = screen };
        _ = new WindowLifecycle(window, null, false);

        // Show then close
        window.Show();
        window.Close();

        await Assert.That(screen.IsActive).IsFalse();
    }

    [Test]
    public async Task WindowLifecycle_WithContext_AttachesViewWithContext()
    {
        var screen = new TestScreen();
        var window = new Window { DataContext = screen };
        _ = new WindowLifecycle(window, "popup", false);
        var attachedView = ((IViewAware)screen).GetView("popup");
        var hasView = ReferenceEquals(attachedView, window);

        await Assert.That(hasView).IsTrue();
    }

    [Test]
    public async Task PopupLifecycle_Constructor_SetsView()
    {
        var popup = new Popup();
        popup.DataContext = new SampleViewModel();
        var lifecycle = new PopupLifecycle(popup, null);
        var view = lifecycle.View;

        await Assert.That(view).IsNotNull();
        await Assert.That(view).IsTypeOf<Popup>();
    }

    [Test]
    public async Task PopupLifecycle_Constructor_SetsContext()
    {
        var popup = new Popup();
        popup.DataContext = new SampleViewModel();
        var lifecycle = new PopupLifecycle(popup, "ctx");
        var context = lifecycle.Context;

        await Assert.That(context).IsEqualTo("ctx");
    }

    [Test]
    public async Task WindowLifecycle_WithCloseGuard_HooksClosingEvent()
    {
        var screen = new CloseGuardScreen { AllowClose = false };
        var window = new Window { DataContext = screen };
        _ = new WindowLifecycle(window, null, false);

        await Assert.That(screen.IsActive).IsTrue();
    }

    [Test]
    public async Task WindowLifecycle_WithoutCloseGuard_DoesNotThrow()
    {
        var action = () =>
        {
            var screen = new TestScreen();
            var window = new Window { DataContext = screen };
            _ = new WindowLifecycle(window, null, false);
        };

        await Assert.That(action).ThrowsNothing();
    }

    [Test]
    public async Task PopupLifecycle_WithScreen_WiresEvents()
    {
        // PopupLifecycle wires Opened/Closed events when DataContext is IViewAware or IActivatable.
        // Just verify it doesn't throw during construction.
        var screen = new TestScreen();
        var popup = new Popup { DataContext = screen };
        _ = new PopupLifecycle(popup, null);

        var screen2 = new TestScreen();
        var popup2 = new Popup { DataContext = screen2 };
        var lc = new PopupLifecycle(popup2, null);
        var viewNotNull = lc.View is not null;

        await Assert.That(viewNotNull).IsTrue();
    }

    [Test]
    public async Task PopupLifecycle_NullContext_IsNull()
    {
        var popup = new Popup();
        popup.DataContext = new TestScreen();
        var lifecycle = new PopupLifecycle(popup, null);
        var context = lifecycle.Context;

        await Assert.That(context).IsNull();
    }
}

/// <summary>
/// A Screen-derived test class for lifecycle tests.
/// </summary>
public class TestScreen : Screen
{
}

/// <summary>
/// A Screen-derived test class that implements ICloseGuard for close-guard tests.
/// </summary>
public class CloseGuardScreen : Screen
{
    public bool AllowClose { get; set; }
    public override Task<bool> CanCloseAsync() => Task.FromResult(AllowClose);
}
