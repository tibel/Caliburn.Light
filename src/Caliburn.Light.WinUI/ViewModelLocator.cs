using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Caliburn.Light.WinUI
{
    /// <summary>
    /// Locates view and view-model instances.
    /// </summary>
    public sealed class ViewModelLocator : IViewModelLocator
    {
        private readonly IViewModelTypeResolver _typeResolver;
        private readonly IServiceProvider _serviceProvider;
        private ILogger _logger;

        /// <summary>
        /// Creates an instance of <see cref="ViewModelLocator"/>.
        /// </summary>
        /// <param name="typeResolver">The type resolver.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public ViewModelLocator(IViewModelTypeResolver typeResolver, IServiceProvider serviceProvider)
        {
            if (typeResolver is null)
                throw new ArgumentNullException(nameof(typeResolver));
            if (serviceProvider is null)
                throw new ArgumentNullException(nameof(serviceProvider));

            _typeResolver = typeResolver;
            _serviceProvider = serviceProvider;
        }

        private ILogger Log => _logger ??= LogManager.GetLogger(GetType());

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
            if (view is not null)
            {
                Log.Info("Using cached view for {0}.", model);
                return view;
            }

            var modelType = model.GetType();
            var viewType = _typeResolver.GetViewType(modelType, context);
            if (viewType is null)
            {
                Log.Error("Cannot find view for {0}.", modelType);
                return new TextBlock { Text = string.Format("Cannot find view for {0}.", modelType) };
            }

            return _serviceProvider.GetService(viewType) as UIElement
                ?? (UIElement)Activator.CreateInstance(viewType);
        }

        private static UIElement TryGetViewFromViewAware(object model, string context)
        {
            if (model is IViewAware viewAware)
            {
                if (viewAware.GetView(context) is UIElement view)
                {
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

            if (view is FrameworkElement frameworkElement && frameworkElement.DataContext is not null)
            {
                Log.Info("Using current data context for {0}.", view);
                return frameworkElement.DataContext;
            }

            var viewType = view.GetType();
            var modelType = _typeResolver.GetModelType(viewType);
            if (modelType is null)
            {
                Log.Error("Cannot find model for {0}.", viewType);
                return null;
            }

            var model = _serviceProvider.GetService(modelType);
            if (model is null)
            {
                Log.Error("Cannot locate {0}.", modelType);
                return null;
            }

            return model;
        }
    }
}
