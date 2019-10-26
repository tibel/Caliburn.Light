using System;
#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
#endif

namespace Caliburn.Light
{
    /// <summary>
    /// Locates view and view-model instances.
    /// </summary>
    public sealed class ViewModelLocator : IViewModelLocator
    {
        private static readonly ILogger Log = LogManager.GetLogger(typeof(ViewModelLocator));

        private readonly IViewModelTypeResolver _typeResolver;
        private readonly IServiceLocator _serviceLocator;

        /// <summary>
        /// Creates an instance of <see cref="ViewModelLocator"/>.
        /// </summary>
        /// <param name="typeResolver">The type resolver.</param>
        /// <param name="serviceLocator">The service locator.</param>
        public ViewModelLocator(IViewModelTypeResolver typeResolver, IServiceLocator serviceLocator)
        {
            if (typeResolver == null)
                throw new ArgumentNullException(nameof(typeResolver));

            if (serviceLocator == null)
                throw new ArgumentNullException(nameof(serviceLocator));

            _typeResolver = typeResolver;
            _serviceLocator = serviceLocator;
        }

        /// <summary>
        /// Locates the view for the specified model instance.
        /// </summary>
        /// <param name="model">the model instance.</param>
        /// <param name="context">The context (or null).</param>
        /// <returns>The view.</returns>
        public UIElement LocateForModel(object model, string context)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var view = TryGetViewFromViewAware(model, context);
            if (view != null)
            {
                Log.Info("Using cached view for {0}.", model);
                return view;
            }

            var modelType = model.GetType();
            var viewType = _typeResolver.GetViewType(modelType, context);
            if (viewType == null)
            {
                Log.Error("Cannot find view for {0}.", modelType);
                return new TextBlock { Text = string.Format("Cannot find view for {0}.", modelType) };
            }

            view = _serviceLocator.GetInstance(viewType) as UIElement;
            if (view == null)
            {
                view = (UIElement)Activator.CreateInstance(viewType);
            }

            return view;
        }

        private UIElement TryGetViewFromViewAware(object model, string context)
        {
            var viewAware = model as IViewAware;
            if (viewAware != null)
            {
                var view = viewAware.GetView(context) as UIElement;
                if (view != null)
                {
#if !NETFX_CORE
                    var windowCheck = view as Window;
                    if (windowCheck == null ||
                        (!windowCheck.IsLoaded && new WindowInteropHelper(windowCheck).Handle != IntPtr.Zero))
                    {
                        return view;
                    }
#else
                    return view;
#endif
                }
            }

            return null;
        }

        /// <summary>
        /// Locates the view model for the specified view instance.
        /// </summary>
        /// <param name="view">The view instance.</param>
        /// <returns>The view model.</returns>
        public object LocateForView(UIElement view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var frameworkElement = view as FrameworkElement;
            if (frameworkElement != null && frameworkElement.DataContext != null)
            {
                Log.Info("Using current data context for {0}.", view);
                return frameworkElement.DataContext;
            }

            var viewType = view.GetType();
            var modelType = _typeResolver.GetModelType(viewType);
            if (modelType == null)
            {
                Log.Error("Cannot find model for {0}.", viewType);
                return null;
            }

            var model = _serviceLocator.GetInstance(modelType);
            if (model == null)
            {
                Log.Error("Cannot locate {0}.", modelType);
                return null;
            }

            return model;
        }
    }
}
