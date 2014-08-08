using System.Linq;
using System.Windows;

namespace Caliburn.Light
{
    internal static class CoTaskHelper
    {
        public static Window GetActiveWindow(CoroutineExecutionContext context)
        {
            var view = context.Source as DependencyObject;
            if (view != null)
            {
                return Window.GetWindow(view);
            }

            var application = Application.Current;
            if (application != null)
            {
                var active = application.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
                return active ?? application.MainWindow;
            }

            return null;
        }
    }
}
