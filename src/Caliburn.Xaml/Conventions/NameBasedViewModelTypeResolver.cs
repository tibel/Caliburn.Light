using System;
using System.Linq;

namespace Caliburn.Light
{
    /// <summary>
    /// Resolves view and view-model types based on the type names.
    /// </summary>
    public class NameBasedViewModelTypeResolver : IViewModelTypeResolver
    {
        private static readonly ILogger Log = LogManager.GetLogger(typeof(NameBasedViewModelTypeResolver));

        /// <summary>
        /// Determines the view model type based on the specified view type.
        /// </summary>
        /// <param name="viewType">The view type.</param>
        /// <returns>The view model type or null, if not found.</returns>
        public Type GetModelType(Type viewType)
        {
            var viewTypeName = viewType.FullName;

            var classNames = ViewTypeNameTransformer.TransformName(viewTypeName, false);
            var interfaceNames = ViewTypeNameTransformer.TransformName(viewTypeName, true);

            var candidates = classNames.Concat(interfaceNames).ToArray();
            var modelType = TypeResolver.FindByName(candidates);

            if (modelType == null)
            {
                Log.Warn("View Model not found. Searched: {0}.", string.Join(", ", candidates));
            }

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
            var modelTypeName = modelType.FullName;
            var viewTypeList = ViewModelTypeNameTransformer.TransformName(modelTypeName, context);
            var viewType = TypeResolver.FindByName(viewTypeList);

            if (viewType == null)
            {
                Log.Warn("View not found. Searched: {0}.", string.Join(", ", viewTypeList));
            }

            return viewType;
        }
    }
}
