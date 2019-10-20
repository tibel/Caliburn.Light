using System;
using System.Collections.Generic;
using System.ComponentModel;
#if NETFX_CORE
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace Caliburn.Light
{
    /// <summary>
    /// Resolves view and view-model types.
    /// </summary>
    public class ViewModelTypeResolver : IViewModelTypeResolver
    {
        private readonly Dictionary<Type, Type> _modelTypeLookup = new Dictionary<Type, Type>();
        private readonly Dictionary<ViewTypeLookupKey, Type> _viewTypeLookup = new Dictionary<ViewTypeLookupKey, Type>(new ViewTypeLookupKeyComparer());

        /// <summary>
        /// Determines the view model type based on the specified view type.
        /// </summary>
        /// <param name="viewType">The view type.</param>
        /// <returns>The view model type or null, if not found.</returns>
        public Type GetModelType(Type viewType)
        {
            if (viewType == null)
                throw new ArgumentNullException(nameof(viewType));

            Type modelType;
            _modelTypeLookup.TryGetValue(viewType, out modelType);
            return modelType;
        }

        /// <summary>
        /// Locates the view type based on the specified model type.
        /// </summary>
        /// <param name="modelType">The model type.</param>
        /// <param name="context">The context instance (or null).</param>
        /// <returns>The view type or null, if not found.</returns>
        public Type GetViewType(Type modelType, string context)
        {
            if (modelType == null)
                throw new ArgumentNullException(nameof(modelType));

            Type viewType;
            _viewTypeLookup.TryGetValue(new ViewTypeLookupKey(modelType, context ?? string.Empty), out viewType);
            return viewType;
        }

        /// <summary>
        /// Adds a view view-model mapping.
        /// </summary>
        /// <typeparam name="TView">The view type.</typeparam>
        /// <typeparam name="TViewModel">The view-model type.</typeparam>
        /// <param name="context">The context instance (or null).</param>
        public void AddMapping<TView, TViewModel>(string context = null)
            where TView : UIElement
            where TViewModel : INotifyPropertyChanged
        {
            var viewType = typeof(TView);
            var modelType = typeof(TViewModel);

            if (context == null)
                _modelTypeLookup.Add(viewType, modelType);

            _viewTypeLookup.Add(new ViewTypeLookupKey(modelType, context ?? string.Empty), viewType);
        }
    }
}
