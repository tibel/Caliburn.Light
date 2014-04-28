using System.Reflection;
#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Data;
#endif

namespace Caliburn.Xaml
{
    /// <summary>
    /// Some helper methods when dealing with UI elements.
    /// </summary>
    public static class ViewHelper
    {
        /// <summary>
        /// Determines whether the specified <paramref name="element"/> is loaded.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>true if the element is loaded; otherwise, false.
        /// </returns>
        public static bool IsElementLoaded(FrameworkElement element)
        {
            if (element == null)
                return false;

#if NETFX_CORE
            var content = Window.Current.Content;
            var parent = element.Parent ?? VisualTreeHelper.GetParent(element);
            return parent != null || (content != null && element == content);
#elif SILVERLIGHT
            //var root = Application.Current.RootVisual;
            var parent = element.Parent ?? VisualTreeHelper.GetParent(element);
            var children = VisualTreeHelper.GetChildrenCount(element);
            return parent != null && children > 0;
#else
            return element.IsLoaded;
#endif
        }

        private const string DefaultContentPropertyName = "Content";

        /// <summary>
        /// Finds the Content property of specified <paramref name="element"/>.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>An object that represents the specified property, or null if the property is not found.</returns>
        public static PropertyInfo FindContentProperty(object element)
        {
            var type = element.GetType();
            var contentPropertyAttribute = type.GetTypeInfo().GetCustomAttribute<ContentPropertyAttribute>(true);
            var contentPropertyName = (contentPropertyAttribute == null) ? DefaultContentPropertyName : contentPropertyAttribute.Name;
            return type.GetRuntimeProperty(contentPropertyName);
        }

        /// <summary>
        /// Determines whether a particular dependency property already has a binding on the provided element.
        /// </summary>
        public static bool HasBinding(FrameworkElement element, DependencyProperty property)
        {
#if SILVERLIGHT || NETFX_CORE
            return element.GetBindingExpression(property) != null;
#else
            return BindingOperations.GetBindingBase(element, property) != null;
#endif
        }
    }
}
