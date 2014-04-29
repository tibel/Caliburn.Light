using Caliburn.Light;
using System;
#if NETFX_CORE
using Windows.UI.Xaml;
using Microsoft.Xaml.Interactivity;
#else
using System.Windows;
#endif

namespace Caliburn.Xaml
{
    /// <summary>
    /// Binds a view to a view model.
    /// </summary>
    public static class ViewModelBinder
    {
        private static readonly ILogger Log = LogManager.GetLogger(typeof (ViewModelBinder));

        /// <summary>
        /// Binds the specified viewModel to the view.
        /// </summary>
        ///<remarks>Passes the the view model, view and creation context (or null for default) to use in applying binding.</remarks>
        public static Action<object, DependencyObject, object> Bind = (viewModel, view, context) =>
        {
            Log.Info("Binding {0} and {1}.", view, viewModel);

            var noDataContext = (bool)view.GetValue(Xaml.Bind.NoDataContextProperty);

            var frameworkElement = view as FrameworkElement;
            if (frameworkElement != null && !noDataContext)
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
        };
    }
}
