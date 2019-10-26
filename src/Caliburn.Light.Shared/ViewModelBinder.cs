#if NETFX_CORE
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace Caliburn.Light
{
    /// <summary>
    /// Binds a view to a view model.
    /// </summary>
    public sealed class ViewModelBinder : IViewModelBinder
    {
        private static readonly ILogger Log = LogManager.GetLogger(typeof (ViewModelBinder));

        /// <summary>
        /// Binds the specified viewModel to the view.
        /// </summary>
        /// <param name="viewModel">The view model</param>
        /// <param name="view">The view.</param>
        /// <param name="context">The creation context (or null for default).</param>
        /// <param name="setDataContext">Whether to set the DataContext or leave unchanged.</param>
        public void Bind(object viewModel, UIElement view, string context, bool setDataContext)
        {
            Log.Info("Binding {0} and {1}.", view, viewModel);

            var frameworkElement = view as FrameworkElement;
            if (frameworkElement != null && setDataContext)
            {
                Log.Info("Setting DC of {0} to {1}.", frameworkElement, viewModel);
                frameworkElement.DataContext = viewModel;
            }

            var viewAware = viewModel as IViewAware;
            if (viewAware != null)
            {
                Log.Info("Attaching {0} to {1}.", view, viewAware);
                viewAware.AttachView(view, context);
            }
        }
    }
}
