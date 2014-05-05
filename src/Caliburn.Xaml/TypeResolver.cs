using System.Collections.ObjectModel;
using Weakly;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
#if !NETFX_CORE
using System.Windows;
#else
using Windows.UI.Xaml;
#endif

namespace Caliburn.Light
{
    /// <summary>
    /// A strategy for resolving type names by the framework.
    /// </summary>
    public static class TypeResolver
    {
        private static readonly List<Assembly> KnownAssemblies = new List<Assembly>();
        private static readonly Dictionary<String, Type> TypeNameCache = new Dictionary<string, Type>();

        /// <summary>
        /// The assemblies inspected by the <seealso cref="TypeResolver"/>.
        /// </summary>
        public static IReadOnlyCollection<Assembly> Assemblies
        {
            get { return new ReadOnlyCollection<Assembly>(KnownAssemblies); }
        }

        /// <summary>
        /// Add an assembly to the <seealso cref="TypeResolver"/>.
        /// </summary>
        /// <param name="assembly">The assembly to inspect.</param>
        public static void AddAssembly(Assembly assembly)
        {
            if (KnownAssemblies.Contains(assembly)) return;
            ExtractTypes(assembly).ForEach(type => TypeNameCache.Add(type.FullName, type));
        }

        /// <summary>
        /// Removes all registered types.
        /// </summary>
        public static void Reset()
        {
            KnownAssemblies.Clear();
            TypeNameCache.Clear();
        }

        /// <summary>
        /// Finds a type which matches one of the <paramref name="names"/>.
        /// </summary>
        /// <param name="names">The names to search for.</param>
        /// <returns>The fist type that matchen one of the names or null if not found.</returns>
        public static Type FindByName(IEnumerable<string> names)
        {
            if (names == null) return null;
            return names.Select(n => TypeNameCache.GetValueOrDefault(n)).FirstOrDefault(t => t != null);
        }

        /// <summary>
        /// Extracts the types from the spezified assembly that can be resolved.
        /// </summary>
        public static Func<Assembly, IEnumerable<Type>> ExtractTypes = assembly =>
            assembly.ExportedTypes
                .Where(t =>
                    typeof (UIElement).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()) ||
                    typeof (INotifyPropertyChanged).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()));
    }
}
