using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
#if NETFX_CORE
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace Caliburn.Light
{
    /// <summary>
    /// Resolves view and view-model types based on the type names.
    /// </summary>
    public class NameBasedViewModelTypeResolver : IViewModelTypeResolver
    {
        private static readonly ILogger Log = LogManager.GetLogger(typeof(NameBasedViewModelTypeResolver));

        private readonly List<Assembly> _assemblies = new List<Assembly>();
        private readonly Dictionary<string, Type> _typeNameCache = new Dictionary<string, Type>();

        /// <summary>
        /// The assemblies inspected by the <seealso cref="NameBasedViewModelTypeResolver"/>.
        /// </summary>
        public IReadOnlyCollection<Assembly> Assemblies
        {
            get { return _assemblies.AsReadOnly(); }
        }

        /// <summary>
        /// Add an assembly to the <seealso cref="NameBasedViewModelTypeResolver"/>.
        /// </summary>
        /// <param name="assembly">The assembly to inspect.</param>
        public void AddAssembly(Assembly assembly)
        {
            if (_assemblies.Contains(assembly)) return;

            var types = ExtractTypes(assembly);
            foreach (var type in types)
            {
                _typeNameCache.Add(type.FullName, type);
            }
        }

        /// <summary>
        /// Removes all registered types.
        /// </summary>
        public void Reset()
        {
            _assemblies.Clear();
            _typeNameCache.Clear();
        }

        private static IEnumerable<Type> ExtractTypes(Assembly assembly)
        {
            return assembly.ExportedTypes
                .Where(t =>
                    typeof(UIElement).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()) ||
                    typeof(INotifyPropertyChanged).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()));
        }

        private Type FindTypeByNames(IEnumerable<string> names)
        {
            if (names == null) return null;

            foreach (var name in names)
            {
                Type type;
                if (_typeNameCache.TryGetValue(name, out type))
                    return type;
            }

            return null;
        }

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
            var modelType = FindTypeByNames(candidates);

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
            var viewType = FindTypeByNames(viewTypeList);

            if (viewType == null)
            {
                Log.Warn("View not found. Searched: {0}.", string.Join(", ", viewTypeList));
            }

            return viewType;
        }
    }
}
