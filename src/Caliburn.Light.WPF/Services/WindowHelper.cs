using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Caliburn.Light.WPF;

internal static class WindowHelper
{
    public static Task ShowModal(this Window window, Window owner)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(owner);

        window.Owner = owner;

        var hWndOwner = new WindowInteropHelper(owner).Handle;
        var tcs = new TaskCompletionSource<bool?>();

        void closeHandler(object? sender, EventArgs _)
        {
            var w = (Window)sender!;
            w.Closed -= closeHandler;

            User32.EnableWindow(hWndOwner, true);
            w.Owner.Activate();

            tcs.TrySetResult(null);
        }

        try
        {
            User32.EnableWindow(hWndOwner, false);
            window.Closed += closeHandler;

            window.Show();
        }
        catch
        {
            window.Closed -= closeHandler;
            User32.EnableWindow(hWndOwner, true);

            throw;
        }

        return tcs.Task;
    }
}
