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
        private readonly IViewModelTypeResolver _typeResolver;
        private readonly IServiceLocator _serviceLocator;
        private readonly ILogger _logger;

        /// <summary>
        /// Creates an instance of <see cref="ViewModelLocator"/>.
        /// </summary>
        /// <param name="typeResolver">The type resolver.</param>
        /// <param name="serviceLocator">The service locator.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public ViewModelLocator(IViewModelTypeResolver typeResolver, IServiceLocator serviceLocator, ILoggerFactory loggerFactory)
        {
            if (typeResolver is null)
                throw new ArgumentNullException(nameof(typeResolver));
            if (serviceLocator is null)
                throw new ArgumentNullException(nameof(serviceLocator));
            if (loggerFactory is null)
                throw new ArgumentNullException(nameof(loggerFactory));

            _typeResolver = typeResolver;
            _serviceLocator = serviceLocator;
            _logger = loggerFactory.GetLogger(typeof(ViewModelLocator));
        }

        /// <summary>
        /// Locates the view for the specified model instance.
        /// </summary>
        /// <param name="model">the model instance.</param>
        /// <param name="context">The context (or null).</param>
        /// <returns>The view.</returns>
        public UIElement LocateForModel(object model, string context)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));

            var view = TryGetViewFromViewAware(model, context);
            if (view is object)
            {
                _logger.Info("Using cached view for {0}.", model);
                return view;
            }

            var modelType = model.GetType();
            var viewType = _typeResolver.GetViewType(modelType, context);
            if (viewType is null)
            {
                _logger.Error("Cannot find view for {0}.", modelType);
                return new TextBlock { Text = string.Format("Cannot find view for {0}.", modelType) };
            }

            return _serviceLocator.GetInstance(viewType) as UIElement
                ?? (UIElement)Activator.CreateInstance(viewType);
        }

        private UIElement TryGetViewFromViewAware(object model, string context)
        {
            if (model is IViewAware viewAware)
            {
                if (viewAware.GetView(context) is UIElement view)
                {
#if !NETFX_CORE
                    if (view is Window window && (window.IsLoaded || new WindowInteropHelper(window).Handle == IntPtr.Zero))
                        return null;
#endif

                    // remove from parent
                    if (view is FrameworkElement fe && fe.Parent is ContentControl parent)
                            parent.Content = null;

                    return view;
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
            if (view is null)
                throw new ArgumentNullException(nameof(view));

            var frameworkElement = view as FrameworkElement;
            if (frameworkElement is object && frameworkElement.DataContext is object)
            {
                _logger.Info("Using current data context for {0}.", view);
                return frameworkElement.DataContext;
            }

            var viewType = view.GetType();
            var modelType = _typeResolver.GetModelType(viewType);
            if (modelType is null)
            {
                _logger.Error("Cannot find model for {0}.", viewType);
                return null;
            }

            var model = _serviceLocator.GetInstance(modelType);
            if (model is null)
            {
                _logger.Error("Cannot locate {0}.", modelType);
                return null;
            }

            return model;
        }
    }
}
