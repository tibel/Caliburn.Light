using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TUnit.Core;
using TUnit.Core.Executors;

namespace Caliburn.Light.WinUI.Tests;

/// <summary>
/// Tests that run directly on the WinUI UI thread via WinUITestExecutor.
/// This enables testing of full window lifecycle including AppWindow events.
/// Uses Win32 WM_CLOSE to trigger the real OS close path (AppWindow.Closing),
/// since WinUI's Window.Close() bypasses AppWindow.Closing in the test host.
/// </summary>
[TestExecutor<WinUITestExecutor>]
public class WindowIntegrationTests
{
    const uint WM_CLOSE = 0x0010;

    [DllImport("user32.dll")]
    static extern nint SendMessage(nint hWnd, uint Msg, nuint wParam, nint lParam);

    static nint GetHwnd(Window window) =>
        WinRT.Interop.WindowNative.GetWindowHandle(window);

    [Test]
    public async Task Window_HasHwnd_BeforeActivate()
    {
        var window = new Window { Content = new Grid() };
        await Assert.That(GetHwnd(window)).IsNotEqualTo(nint.Zero);
    }

    [Test]
    public async Task Window_CanActivateAndClose()
    {
        var window = new Window { Content = new Grid() };
        window.Activate();
        window.Close();
    }

    [Test]
    public async Task Window_ClosedEvent_Fires()
    {
        bool closedFired = false;
        var window = new Window { Content = new Grid() };
        window.Activate();
        window.Closed += (s, e) => { closedFired = true; };
        window.Close();

        await Assert.That(closedFired).IsTrue();
    }

    [Test]
    public async Task WM_CLOSE_Fires_AppWindowClosing()
    {
        bool closingFired = false;
        var window = new Window { Content = new Grid() };
        window.Activate();
        window.AppWindow.Closing += (s, e) => { closingFired = true; };

        SendMessage(GetHwnd(window), WM_CLOSE, 0, 0);

        await Assert.That(closingFired).IsTrue();
    }

    [Test]
    public async Task WM_CLOSE_CanBeCancelled_ViaAppWindowClosing()
    {
        bool closedFired = false;
        var window = new Window { Content = new Grid() };
        window.Activate();

        window.AppWindow.Closing += (s, e) => { e.Cancel = true; };
        window.Closed += (s, e) => { closedFired = true; };

        SendMessage(GetHwnd(window), WM_CLOSE, 0, 0);

        // Close was cancelled — window should still be alive
        await Assert.That(closedFired).IsFalse();
        await Assert.That(window.AppWindow.IsVisible).IsTrue();

        // Clean up
        window.Close();
    }

    [Test]
    public async Task CloseGuard_PreventsClose_ViaWM_CLOSE()
    {
        var screen = new CloseGuardScreen { AllowClose = false };
        var grid = new Grid { DataContext = screen };
        var window = new Window { Content = grid };
        _ = new WindowLifecycle(window, null, false);
        window.Activate();

        await Assert.That(screen.IsActive).IsTrue();

        // WM_CLOSE triggers AppWindow.Closing → ICloseGuard blocks close
        SendMessage(GetHwnd(window), WM_CLOSE, 0, 0);

        await Assert.That(screen.IsActive).IsTrue();
        await Assert.That(window.AppWindow.IsVisible).IsTrue();

        // Clean up: allow close
        screen.AllowClose = true;
        window.Close();
    }

    [Test]
    public async Task CloseGuard_AllowsClose_ViaWM_CLOSE_WhenPermitted()
    {
        var screen = new CloseGuardScreen { AllowClose = true };
        var grid = new Grid { DataContext = screen };
        var window = new Window { Content = grid };
        _ = new WindowLifecycle(window, null, false);
        window.Activate();

        await Assert.That(screen.IsActive).IsTrue();

        SendMessage(GetHwnd(window), WM_CLOSE, 0, 0);

        await Assert.That(screen.IsActive).IsFalse();
    }

    [Test]
    public async Task CloseGuard_PreventsThenAllows_ViaWM_CLOSE()
    {
        var screen = new CloseGuardScreen { AllowClose = false };
        var grid = new Grid { DataContext = screen };
        var window = new Window { Content = grid };
        _ = new WindowLifecycle(window, null, false);
        window.Activate();

        // First attempt — blocked
        SendMessage(GetHwnd(window), WM_CLOSE, 0, 0);
        await Assert.That(screen.IsActive).IsTrue();

        // Now allow close
        screen.AllowClose = true;
        SendMessage(GetHwnd(window), WM_CLOSE, 0, 0);
        await Assert.That(screen.IsActive).IsFalse();
    }

    [Test]
    public async Task WindowLifecycle_Close_DeactivatesAndDetaches()
    {
        var screen = new TestScreen();
        var grid = new Grid { DataContext = screen };
        var window = new Window { Content = grid };
        _ = new WindowLifecycle(window, "ctx", false);
        window.Activate();

        window.Close();

        await Assert.That(screen.IsActive).IsFalse();
        await Assert.That(((IViewAware)screen).GetView("ctx")).IsNull();
    }

    [Test]
    public async Task WindowLifecycle_ActivateWithWindow_ActivatesOnWindowActivate()
    {
        var screen = new TestScreen();
        var grid = new Grid { DataContext = screen };
        var window = new Window { Content = grid };
        _ = new WindowLifecycle(window, null, true);

        window.Activate();

        await Assert.That(screen.IsActive).IsTrue();

        window.Close();
    }

    [Test]
    public async Task WindowManager_ShowWindow_FullPipeline()
    {
        var screen = new TestScreen();
        var config = new ViewModelLocatorConfiguration()
            .AddMapping<Grid, TestScreen>();
        var sp = new ServiceProviderWithInstance(typeof(Grid), new Grid());
        var locator = new ViewModelLocator(config, sp);
        var wm = new WindowManager(locator);

        wm.ShowWindow(screen, null);

        await Assert.That(screen.IsActive).IsTrue();
    }

    [Test]
    public async Task EnsureWindow_WithDisplayName_SetsWindowTitle()
    {
        var screen = new DisplayNameScreen { DisplayName = "My Window Title" };
        var config = new ViewModelLocatorConfiguration()
            .AddMapping<Grid, DisplayNameScreen>();
        var sp = new ServiceProviderWithInstance(typeof(Grid), new Grid());
        var locator = new ViewModelLocator(config, sp);
        var wm = new TestableWindowManager(locator);

        var textBlock = new TextBlock();
        var window = wm.CallEnsureWindow(screen, textBlock);

        await Assert.That(window.Title).IsEqualTo("My Window Title");
    }

    [Test]
    public async Task WindowManager_Activate_WithNoView_ReturnsFalse()
    {
        var config = new ViewModelLocatorConfiguration();
        var sp = new SimpleServiceProvider();
        var wm = new WindowManager(new ViewModelLocator(config, sp));

        var result = wm.Activate(new TestScreen());
        await Assert.That(result).IsFalse();
    }
}

/// <summary>
/// A Screen that also implements IHaveDisplayName for window title binding.
/// </summary>
public class DisplayNameScreen : Screen, IHaveDisplayName
{
    private string? _displayName;
    public string? DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }
}

/// <summary>
/// IServiceProvider that returns a stored instance for a specific type.
/// </summary>
internal sealed class ServiceProviderWithInstance : IServiceProvider
{
    private readonly Type _type;
    private readonly object _instance;

    public ServiceProviderWithInstance(Type type, object instance)
    {
        _type = type;
        _instance = instance;
    }

    public object? GetService(Type serviceType) =>
        serviceType == _type ? _instance : null;
}
