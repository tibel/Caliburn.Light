using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Caliburn.Light.WPF
{
    internal static class WindowHelper
    {
        public static Task ShowModal(this Window window)
        {
            if (window is null)
                throw new ArgumentNullException(nameof(window));
            if (window.Owner is null)
                throw new ArgumentException("Window has no Owner set.", nameof(window));

            var tcs = new TaskCompletionSource<object>();

            void closeHandler(object sender, EventArgs args)
            {
                var w = (Window)sender;
                w.Closed -= closeHandler;

                w.Owner.SetNativeEnabled(true);
                tcs.SetResult(null);
            }

            try
            {
                window.Owner.SetNativeEnabled(false);
                window.Closed += closeHandler;

                window.Show();
            }
            catch
            {
                window.Closed -= closeHandler;
                window.Owner.SetNativeEnabled(true);

                throw;
            }

            return tcs.Task;
        }

        private const int GWL_STYLE = -16;
        private const int WS_DISABLED = 0x08000000;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private static void SetNativeEnabled(this Window window, bool enabled)
        {
            var handle = new WindowInteropHelper(window).Handle;
            _ = SetWindowLong(handle, GWL_STYLE,
                (GetWindowLong(handle, GWL_STYLE) & ~WS_DISABLED) | (enabled ? 0 : WS_DISABLED));
        }
    }
}
