using Microsoft.UI.Xaml;
using Windows.Foundation;
using Windows.UI.Popups;

namespace Demo.HelloSpecialValues
{
    //WORKAROUND https://github.com/microsoft/microsoft-ui-xaml/issues/4167
    internal static class DialogHelper
    {
        public static IAsyncOperation<IUICommand> ShowAsyncEx(this MessageDialog dialog, Window parent)
        {
            var handle = WinRT.Interop.WindowNative.GetWindowHandle(parent);
            WinRT.Interop.InitializeWithWindow.Initialize(dialog, handle);

            return dialog.ShowAsync();
        }

        public static IAsyncOperation<IUICommand> ShowAsyncEx(this MessageDialog dialog)
        {
            var application = (App)Application.Current;
            return ShowAsyncEx(dialog, application.MainWindow);
        }
    }
}
