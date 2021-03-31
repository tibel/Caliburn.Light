using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.UI.Popups;
using WinRT;

namespace Demo.HelloSpecialValues
{
    //WORKAROUND https://github.com/microsoft/microsoft-ui-xaml/issues/4167
    internal static class DialogHelper
    {
        public static IAsyncOperation<IUICommand> ShowAsyncEx(this MessageDialog dialog, Window parent = null)
        {
            var handle = parent is null
                ? GetActiveWindow()
                : parent.As<IWindowNative>().WindowHandle;

            if (handle == IntPtr.Zero)
                throw new InvalidOperationException();

            dialog.As<IInitializeWithWindow>().Initialize(handle);
            return dialog.ShowAsync();
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("EECDBF0E-BAE9-4CB6-A68E-9598E1CB57BB")]
        private interface IWindowNative
        {
            IntPtr WindowHandle { get; }
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1")]
        private interface IInitializeWithWindow
        {
            void Initialize(IntPtr hwnd);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();
    }
}
