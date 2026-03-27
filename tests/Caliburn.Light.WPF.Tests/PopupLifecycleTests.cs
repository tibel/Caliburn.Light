using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using TUnit.Core.Executors;

namespace Caliburn.Light.WPF.Tests;

[TestExecutor<WpfTestExecutor>]
public class PopupLifecycleTests
{
    private static Popup CreatePopupInWindow(object dataContext)
    {
        var popup = new Popup { DataContext = dataContext, Child = new Grid() };
        var window = new Window { Content = popup };
        window.Show();
        return popup;
    }

    private static async Task YieldDispatcher()
    {
        await Dispatcher.CurrentDispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);
    }

    [Test]
    public async Task Constructor_SetsView()
    {
        var popup = new Popup();
        var lifecycle = new PopupLifecycle(popup, "ctx");
        await Assert.That(ReferenceEquals(lifecycle.View, popup)).IsTrue();
    }

    [Test]
    public async Task Constructor_SetsContext()
    {
        var popup = new Popup();
        var lifecycle = new PopupLifecycle(popup, "myContext");
        await Assert.That(lifecycle.Context).IsEqualTo("myContext");
    }

    [Test]
    public async Task Constructor_NullContext_IsNull()
    {
        var popup = new Popup();
        var lifecycle = new PopupLifecycle(popup, null);
        await Assert.That(lifecycle.Context).IsNull();
    }

    [Test]
    public async Task Opened_ActivatesViewModel()
    {
        var screen = new Screen();
        var popup = CreatePopupInWindow(screen);
        _ = new PopupLifecycle(popup, null);

        popup.IsOpen = true;
        await Assert.That(screen.IsActive).IsTrue();

        popup.IsOpen = false;
    }

    [Test]
    public async Task Opened_AttachesView()
    {
        var screen = new Screen();
        var popup = CreatePopupInWindow(screen);
        _ = new PopupLifecycle(popup, "ctx");

        popup.IsOpen = true;
        var attachedView = ((IViewAware)screen).GetView("ctx");
        await Assert.That(ReferenceEquals(attachedView, popup)).IsTrue();

        popup.IsOpen = false;
    }

    [Test]
    public async Task Closed_DeactivatesViewModel()
    {
        var screen = new Screen();
        var popup = CreatePopupInWindow(screen);
        _ = new PopupLifecycle(popup, null);

        popup.IsOpen = true;
        await Assert.That(screen.IsActive).IsTrue();

        popup.IsOpen = false;
        await YieldDispatcher();
        await Assert.That(screen.IsActive).IsFalse();
    }

    [Test]
    public async Task Closed_DetachesView()
    {
        var screen = new Screen();
        var popup = CreatePopupInWindow(screen);
        _ = new PopupLifecycle(popup, "ctx");

        popup.IsOpen = true;
        await Assert.That(((IViewAware)screen).GetView("ctx")).IsNotNull();

        popup.IsOpen = false;
        await YieldDispatcher();
        await Assert.That(((IViewAware)screen).GetView("ctx")).IsNull();
    }

    [Test]
    public async Task Reopen_ReactivatesViewModel()
    {
        var screen = new Screen();
        var popup = CreatePopupInWindow(screen);
        _ = new PopupLifecycle(popup, null);

        popup.IsOpen = true;
        await Assert.That(screen.IsActive).IsTrue();

        popup.IsOpen = false;
        await YieldDispatcher();
        await Assert.That(screen.IsActive).IsFalse();

        popup.IsOpen = true;
        await Assert.That(screen.IsActive).IsTrue();

        popup.IsOpen = false;
    }

    [Test]
    public async Task NoViewModel_OpenClose_DoesNotThrow()
    {
        var popup = CreatePopupInWindow("not a screen");
        var lifecycle = new PopupLifecycle(popup, null);

        popup.IsOpen = true;
        await Assert.That(popup.IsOpen).IsTrue();

        popup.IsOpen = false;
        await YieldDispatcher();

        await Assert.That(ReferenceEquals(lifecycle.View, popup)).IsTrue();
    }
}
