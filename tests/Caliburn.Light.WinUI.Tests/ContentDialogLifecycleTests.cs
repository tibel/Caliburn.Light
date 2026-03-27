using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using TUnit.Core.Executors;

namespace Caliburn.Light.WinUI.Tests;

[TestExecutor<WinUITestExecutor>]
public class ContentDialogLifecycleTests
{
    private static async Task<(ContentDialog dialog, Window window)> CreateDialogWithXamlRoot(object dataContext)
    {
        var window = new Window();
        var grid = new Grid();
        window.Content = grid;
        window.Activate();

        var loadedTcs = new TaskCompletionSource();
        grid.Loaded += (_, _) => loadedTcs.TrySetResult();
        await loadedTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        var dialog = new ContentDialog
        {
            DataContext = dataContext,
            XamlRoot = grid.XamlRoot,
            Title = "Test",
            CloseButtonText = "Close"
        };
        return (dialog, window);
    }

    private static async Task WaitForOpenedAsync(ContentDialog dialog)
    {
        var tcs = new TaskCompletionSource();
        TypedEventHandler<ContentDialog, ContentDialogOpenedEventArgs> handler = null!;
        handler = (_, _) =>
        {
            dialog.Opened -= handler;
            tcs.TrySetResult();
        };
        dialog.Opened += handler;
        _ = dialog.ShowAsync();
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    private static async Task WaitForClosedAsync(ContentDialog dialog)
    {
        var tcs = new TaskCompletionSource();
        TypedEventHandler<ContentDialog, ContentDialogClosedEventArgs> handler = null!;
        handler = (_, _) =>
        {
            dialog.Closed -= handler;
            tcs.TrySetResult();
        };
        dialog.Closed += handler;
        dialog.Hide();
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task Constructor_SetsView()
    {
        var dialog = new ContentDialog();
        var lifecycle = new ContentDialogLifecycle(dialog, "ctx");
        await Assert.That(ReferenceEquals(lifecycle.View, dialog)).IsTrue();
    }

    [Test]
    public async Task Constructor_SetsContext()
    {
        var dialog = new ContentDialog();
        var lifecycle = new ContentDialogLifecycle(dialog, "myContext");
        await Assert.That(lifecycle.Context).IsEqualTo("myContext");
    }

    [Test]
    public async Task Constructor_NullContext_IsNull()
    {
        var dialog = new ContentDialog();
        var lifecycle = new ContentDialogLifecycle(dialog, null);
        await Assert.That(lifecycle.Context).IsNull();
    }

    [Test]
    public async Task Opened_ActivatesViewModel()
    {
        var screen = new TestScreen();
        var (dialog, window) = await CreateDialogWithXamlRoot(screen);
        _ = new ContentDialogLifecycle(dialog, null);

        await WaitForOpenedAsync(dialog);
        await Assert.That(screen.IsActive).IsTrue();

        dialog.Hide();
        window.Close();
    }

    [Test]
    public async Task Opened_AttachesView()
    {
        var screen = new TestScreen();
        var (dialog, window) = await CreateDialogWithXamlRoot(screen);
        _ = new ContentDialogLifecycle(dialog, "ctx");

        await WaitForOpenedAsync(dialog);
        var attachedView = ((IViewAware)screen).GetView("ctx");
        await Assert.That(attachedView).IsSameReferenceAs(dialog);

        dialog.Hide();
        window.Close();
    }

    [Test]
    public async Task Closed_DeactivatesViewModel()
    {
        var screen = new TestScreen();
        var (dialog, window) = await CreateDialogWithXamlRoot(screen);
        _ = new ContentDialogLifecycle(dialog, null);

        await WaitForOpenedAsync(dialog);
        await Assert.That(screen.IsActive).IsTrue();

        await WaitForClosedAsync(dialog);
        await Assert.That(screen.IsActive).IsFalse();

        window.Close();
    }

    [Test]
    public async Task Closed_DetachesView()
    {
        var screen = new TestScreen();
        var (dialog, window) = await CreateDialogWithXamlRoot(screen);
        _ = new ContentDialogLifecycle(dialog, "ctx");

        await WaitForOpenedAsync(dialog);
        await Assert.That(((IViewAware)screen).GetView("ctx")).IsNotNull();

        await WaitForClosedAsync(dialog);
        await Assert.That(((IViewAware)screen).GetView("ctx")).IsNull();

        window.Close();
    }

    [Test]
    public async Task NoViewModel_OpenClose_DoesNotThrow()
    {
        var (dialog, window) = await CreateDialogWithXamlRoot("not a screen");
        var lifecycle = new ContentDialogLifecycle(dialog, null);
        await Assert.That(ReferenceEquals(lifecycle.View, dialog)).IsTrue();
        window.Close();
    }
}
