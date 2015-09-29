using System;
using System.Collections.Generic;
using System.Reflection;
#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using System.Windows.Data;
#endif

namespace Caliburn.Light
{
    /// <summary>
    /// Some helper methods when dealing with data bindings.
    /// </summary>
    public static class BindingHelper
    {
        /// <summary>
        /// Returns a value that indicates whether the specified property is currently data-bound.
        /// </summary>
        /// <param name="target">The object where <paramref name="property"/> is.</param>
        /// <param name="property">The dependency property to check.</param>
        /// <returns>True if the specified property is data-bound, otherwise False.</returns>
        public static bool IsDataBound(DependencyObject target, DependencyProperty property)
        {
#if NETFX_CORE
            return target.ReadLocalValue(property) is BindingExpressionBase;
#else
            return BindingOperations.IsDataBound(target, property);
#endif
        }

        /// <summary>
        /// Creates and associates a new <see cref="T:System.Windows.Data.BindingExpressionBase"/> with the specified binding target property.
        /// </summary>
        /// <param name="target">The target to set the binding to.</param>
        /// <param name="property">The property on the target to bind.</param>
        /// <param name="binding">The binding to assign to the target property.</param>
        public static void SetBinding(DependencyObject target, DependencyProperty property, BindingBase binding)
        {
            BindingOperations.SetBinding(target, property, binding);
        }

        /// <summary>
        /// Remove data Binding (if any) from a property. 
        /// </summary>
        /// <param name="target">Object from which to remove Binding</param>
        /// <param name="property">Property from which to remove Binding</param> 
        public static void ClearBinding(DependencyObject target, DependencyProperty property)
        {
            if (IsDataBound(target, property))
                target.ClearValue(property);
        }

        /// <summary>
        /// Force a data transfer from Binding source to target.
        /// </summary>
        /// <param name="target">The object where <paramref name="property"/> is.</param>
        /// <param name="property">The dependency property to refresh.</param>
        public static void RefreshBinding(DependencyObject target, DependencyProperty property)
        {
#if NETFX_CORE
            var bindingExpression = target.ReadLocalValue(property) as BindingExpression;
            if (bindingExpression == null || bindingExpression.ParentBinding == null) return;
            BindingOperations.SetBinding(target, property, bindingExpression.ParentBinding);
#else
            var bindingExpressionBase = BindingOperations.GetBindingExpressionBase(target, property);
            if (bindingExpressionBase == null) return;
            bindingExpressionBase.UpdateTarget();
#endif
        }

        private static readonly Dictionary<Type, List<DependencyProperty>> DependencyPropertyCache =
            new Dictionary<Type, List<DependencyProperty>>();

        /// <summary>
        /// Gets the dependency properties from <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to inspect.</param>
        /// <returns>The dependency properties.</returns>
        public static IEnumerable<DependencyProperty> GetDependencyProperties(Type type)
        {
            List<DependencyProperty> properties;
            if (!DependencyPropertyCache.TryGetValue(type, out properties))
            {
                properties = new List<DependencyProperty>();
                for (; type != null && type != typeof (DependencyObject); type = type.GetTypeInfo().BaseType)
                {
                    foreach (var fieldInfo in type.GetRuntimeFields())
                    {
                        if (fieldInfo.IsPublic && fieldInfo.FieldType == typeof (DependencyProperty))
                        {
                            var dependencyProperty = fieldInfo.GetValue(null) as DependencyProperty;
                            if (dependencyProperty != null)
                                properties.Add(dependencyProperty);
                        }
                    }
                }
                DependencyPropertyCache[type] = properties;
            }
            return properties;
        }
    }
}
