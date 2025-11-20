using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;

namespace Caliburn.Light.WinUI;

internal static class WindowHelper
{
    public static Task ShowModal(this Window window, Window owner)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(owner);

        var hWndOwner = Win32Interop.GetWindowFromWindowId(owner.AppWindow.Id);
        var hWnd = Win32Interop.GetWindowFromWindowId(window.AppWindow.Id);
        var tcs = new TaskCompletionSource<bool?>();

        void destroyingHandler(AppWindow sender, object _)
        {
            sender.Destroying -= destroyingHandler;

            owner.Activate();

            tcs.TrySetResult(null);
        }

        try
        {
            // https://github.com/microsoft/microsoft-ui-xaml/issues/10396
            User32.SetWindowLongPtr(hWnd, User32.GWLP_HWNDPARENT, hWndOwner);
            if (window.AppWindow.Presenter is OverlappedPresenter presenter)
                presenter.IsModal = true;

            window.AppWindow.Destroying += destroyingHandler;

            window.AppWindow.Show();
        }
        catch
        {
            window.AppWindow.Destroying -= destroyingHandler;

            throw;
        }

        return tcs.Task;
    }
}
