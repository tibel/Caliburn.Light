using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using TUnit.Core.Executors;

namespace Caliburn.Light.WinUI.Tests;

[TestExecutor<WinUITestExecutor>]
public class PopupLifecycleTests
{
    private static async Task<(Window window, Popup popup)> CreatePopupInWindow(object dataContext)
    {
        var window = new Window();
        var grid = new Grid();
        window.Content = grid;
        window.Activate();

        var loadedTcs = new TaskCompletionSource();
        grid.Loaded += (_, _) => loadedTcs.SetResult();
        await loadedTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        var popup = new Popup
        {
            DataContext = dataContext,
            XamlRoot = grid.XamlRoot,
            Child = new Grid { Width = 10, Height = 10 }
        };
        return (window, popup);
    }

    private static async Task OpenPopupAsync(Popup popup)
    {
        var tcs = new TaskCompletionSource();
        EventHandler<object> handler = null!;
        handler = (_, _) =>
        {
            popup.Opened -= handler;
            tcs.TrySetResult();
        };
        popup.Opened += handler;
        popup.IsOpen = true;
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    private static async Task ClosePopupAsync(Popup popup)
    {
        var tcs = new TaskCompletionSource();
        EventHandler<object> handler = null!;
        handler = (_, _) =>
        {
            popup.Closed -= handler;
            tcs.TrySetResult();
        };
        popup.Closed += handler;
        popup.IsOpen = false;
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task Constructor_SetsView()
    {
        var popup = new Popup();
        var lifecycle = new PopupLifecycle(popup, "ctx");
        await Assert.That(lifecycle.View).IsSameReferenceAs(popup);
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
        var screen = new TestScreen();
        var (window, popup) = await CreatePopupInWindow(screen);
        _ = new PopupLifecycle(popup, null);

        await OpenPopupAsync(popup);
        await Assert.That(screen.IsActive).IsTrue();

        window.Close();
    }

    [Test]
    public async Task Opened_AttachesView()
    {
        var screen = new TestScreen();
        var (window, popup) = await CreatePopupInWindow(screen);
        _ = new PopupLifecycle(popup, "ctx");

        await OpenPopupAsync(popup);
        var attachedView = ((IViewAware)screen).GetView("ctx");
        await Assert.That(attachedView).IsSameReferenceAs(popup);

        window.Close();
    }

    [Test]
    public async Task Closed_DeactivatesViewModel()
    {
        var screen = new TestScreen();
        var (window, popup) = await CreatePopupInWindow(screen);
        _ = new PopupLifecycle(popup, null);

        await OpenPopupAsync(popup);
        await Assert.That(screen.IsActive).IsTrue();

        await ClosePopupAsync(popup);
        await Assert.That(screen.IsActive).IsFalse();

        window.Close();
    }

    [Test]
    public async Task Closed_DetachesView()
    {
        var screen = new TestScreen();
        var (window, popup) = await CreatePopupInWindow(screen);
        _ = new PopupLifecycle(popup, "ctx");

        await OpenPopupAsync(popup);
        await Assert.That(((IViewAware)screen).GetView("ctx")).IsSameReferenceAs(popup);

        await ClosePopupAsync(popup);
        await Assert.That(((IViewAware)screen).GetView("ctx")).IsNull();

        window.Close();
    }

    [Test]
    public async Task Reopen_ReactivatesViewModel()
    {
        var screen = new TestScreen();
        var (window, popup) = await CreatePopupInWindow(screen);
        _ = new PopupLifecycle(popup, null);

        await OpenPopupAsync(popup);
        await Assert.That(screen.IsActive).IsTrue();

        await ClosePopupAsync(popup);
        await Assert.That(screen.IsActive).IsFalse();

        // Reopen — events are still wired (Popup.Closed is not terminal)
        await OpenPopupAsync(popup);
        await Assert.That(screen.IsActive).IsTrue();

        window.Close();
    }

    [Test]
    public async Task NoViewModel_DoesNotThrow()
    {
        var popup = new Popup { DataContext = "not a screen" };
        var lifecycle = new PopupLifecycle(popup, null);
        await Assert.That(lifecycle.View).IsSameReferenceAs(popup);
    }
}
