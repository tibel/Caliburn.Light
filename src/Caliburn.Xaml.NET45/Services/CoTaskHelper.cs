using System.Linq;
using System.Windows;

namespace Caliburn.Light
{
    internal static class CoTaskHelper
    {
        public static Window GetActiveWindow(CoroutineExecutionContext context)
        {
            return GetWindowFromContext(context) ?? GetFirstActiveWindow();
        }

        private static Window GetWindowFromContext(CoroutineExecutionContext context)
        {
            var view = context.Source as DependencyObject;
            return view != null ? Window.GetWindow(view) : null;
        }

        private static Window GetFirstActiveWindow()
        {
            var application = Application.Current;
            if (application == null) return null;
            var active = application.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            return active ?? application.MainWindow;
        }
    }
}
