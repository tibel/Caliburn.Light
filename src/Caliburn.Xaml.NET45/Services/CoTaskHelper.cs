using System.Linq;
using System.Windows;

namespace Caliburn.Light
{
    /// <summary>
    /// Some helper methods when dealing with <see cref="ICoTask"/>.
    /// </summary>
    public static class CoTaskHelper
    {
        /// <summary>
        /// Gets the active window from the execution context.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <returns>The active window.</returns>
        /// <remarks>If no window could be retrieved from the context, the first active window will be returned.</remarks>
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
