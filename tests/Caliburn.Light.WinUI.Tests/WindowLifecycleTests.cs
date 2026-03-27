using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TUnit.Core.Executors;

namespace Caliburn.Light.WinUI.Tests;

public class TestScreen : Screen { }

/// <summary>
/// A Screen that denies closing via ICloseGuard.
/// </summary>
public class CloseGuardScreen : Screen
{
    public bool AllowClose { get; set; }

    public override Task<bool> CanCloseAsync() => Task.FromResult(AllowClose);
}

[TestExecutor<WinUITestExecutor>]
public class WindowLifecycleTests
{
    [Test]
    public async Task Constructor_SetsView()
    {
        var window = new Window();
        var lifecycle = new WindowLifecycle(window, "ctx", false);
        await Assert.That(lifecycle.View).IsSameReferenceAs(window);
    }

    [Test]
    public async Task Constructor_SetsContext()
    {
        var window = new Window();
        var lifecycle = new WindowLifecycle(window, "myContext", false);
        await Assert.That(lifecycle.Context).IsEqualTo("myContext");
    }

    [Test]
    public async Task Constructor_NullContext_IsNull()
    {
        var window = new Window();
        var lifecycle = new WindowLifecycle(window, null, false);
        await Assert.That(lifecycle.Context).IsNull();
    }

    [Test]
    public async Task Constructor_WithScreen_ActivatesViewModel()
    {
        var screen = new TestScreen();
        var grid = new Grid { DataContext = screen };
        var window = new Window { Content = grid };
        _ = new WindowLifecycle(window, null, false);
        await Assert.That(screen.IsActive).IsTrue();
    }

    [Test]
    public async Task Constructor_WithScreen_AttachesView()
    {
        var screen = new TestScreen();
        var grid = new Grid { DataContext = screen };
        var window = new Window { Content = grid };
        _ = new WindowLifecycle(window, "ctx", false);
        var attachedView = ((IViewAware)screen).GetView("ctx");
        await Assert.That(attachedView).IsSameReferenceAs(grid);
    }

    [Test]
    public async Task Constructor_WithCloseGuard_ActivatesScreen()
    {
        var screen = new CloseGuardScreen { AllowClose = false };
        var grid = new Grid { DataContext = screen };
        var window = new Window { Content = grid };
        _ = new WindowLifecycle(window, null, false);
        await Assert.That(screen.IsActive).IsTrue();
    }

    [Test]
    public async Task Constructor_WithoutCloseGuard_DoesNotThrow()
    {
        var screen = new TestScreen();
        var grid = new Grid { DataContext = screen };
        var window = new Window { Content = grid };
        _ = new WindowLifecycle(window, null, false);
        await Assert.That(screen.IsActive).IsTrue();
    }

    [Test]
    public async Task Constructor_WithContext_AttachesViewWithContext()
    {
        var screen = new TestScreen();
        var grid = new Grid { DataContext = screen };
        var window = new Window { Content = grid };
        _ = new WindowLifecycle(window, "popup", false);
        var attachedView = ((IViewAware)screen).GetView("popup");
        await Assert.That(attachedView).IsSameReferenceAs(grid);
    }

    [Test]
    public async Task Closed_DeactivatesViewModel()
    {
        var screen = new TestScreen();
        var grid = new Grid { DataContext = screen };
        var window = new Window { Content = grid };
        _ = new WindowLifecycle(window, null, false);
        await Assert.That(screen.IsActive).IsTrue();

        window.Close();
        await Assert.That(screen.IsActive).IsFalse();
    }
}
