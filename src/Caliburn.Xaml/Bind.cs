using System.Globalization;
using Caliburn.Light;
#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using System.Windows.Data;
#endif

namespace Caliburn.Xaml
{
    /// <summary>
    ///   Hosts dependency properties for binding.
    /// </summary>
    public static class Bind
    {
        /// <summary>
        ///   Allows binding on an existing view. Use this on root UserControls, Pages and Windows; not in a DataTemplate.
        /// </summary>
        public static DependencyProperty ModelProperty =
            DependencyProperty.RegisterAttached(
                "Model",
                typeof (object),
                typeof (Bind),
                new PropertyMetadata(null, ModelChanged)
                );

        /// <summary>
        ///   Allows binding on an existing view without setting the data context. Use this from within a DataTemplate.
        /// </summary>
        public static DependencyProperty ModelWithoutContextProperty =
            DependencyProperty.RegisterAttached(
                "ModelWithoutContext",
                typeof (object),
                typeof (Bind),
                new PropertyMetadata(null, ModelWithoutContextChanged)
                );

        internal static DependencyProperty NoContextProperty =
            DependencyProperty.RegisterAttached(
                "NoContext",
                typeof (bool),
                typeof (Bind),
                new PropertyMetadata(false)
                );

        /// <summary>
        ///   Gets the model to bind to.
        /// </summary>
        /// <param name = "dependencyObject">The dependency object to bind to.</param>
        /// <returns>The model.</returns>
        public static object GetModelWithoutContext(DependencyObject dependencyObject)
        {
            return dependencyObject.GetValue(ModelWithoutContextProperty);
        }

        /// <summary>
        ///   Sets the model to bind to.
        /// </summary>
        /// <param name = "dependencyObject">The dependency object to bind to.</param>
        /// <param name = "value">The model.</param>
        public static void SetModelWithoutContext(DependencyObject dependencyObject, object value)
        {
            dependencyObject.SetValue(ModelWithoutContextProperty, value);
        }

        /// <summary>
        ///   Gets the model to bind to.
        /// </summary>
        /// <param name = "dependencyObject">The dependency object to bind to.</param>
        /// <returns>The model.</returns>
        public static object GetModel(DependencyObject dependencyObject)
        {
            return dependencyObject.GetValue(ModelProperty);
        }

        /// <summary>
        ///   Sets the model to bind to.
        /// </summary>
        /// <param name = "dependencyObject">The dependency object to bind to.</param>
        /// <param name = "value">The model.</param>
        public static void SetModel(DependencyObject dependencyObject, object value)
        {
            dependencyObject.SetValue(ModelProperty, value);
        }

        private static void ModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (UIContext.IsInDesignTool || e.NewValue == null || e.NewValue == e.OldValue)
            {
                return;
            }

            var fe = d as FrameworkElement;
            if (fe == null)
            {
                return;
            }

            ViewHelper.ExecuteOnLoad(fe, delegate
            {
                var target = e.NewValue;
                var containerKey = e.NewValue as string;
                if (containerKey != null)
                {
                    target = IoC.GetInstance(null, containerKey);
                }

                var context = string.IsNullOrEmpty(fe.Name)
                    ? fe.GetHashCode().ToString(CultureInfo.InvariantCulture)
                    : fe.Name;

                ViewModelBinder.Bind(target, d, context);
            });
        }

        private static void ModelWithoutContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (UIContext.IsInDesignTool || e.NewValue == null || e.NewValue == e.OldValue)
            {
                return;
            }

            var fe = d as FrameworkElement;
            if (fe == null)
            {
                return;
            }

            ViewHelper.ExecuteOnLoad(fe, delegate
            {
                var target = e.NewValue;
                var containerKey = e.NewValue as string;
                if (containerKey != null)
                {
                    target = IoC.GetInstance(null, containerKey);
                }

                var context = string.IsNullOrEmpty(fe.Name)
                    ? fe.GetHashCode().ToString(CultureInfo.InvariantCulture)
                    : fe.Name;

                d.SetValue(NoContextProperty, true);
                ViewModelBinder.Bind(target, d, context);
            });
        }

        /// <summary>
        /// Allows application of conventions at design-time.
        /// </summary>
        public static DependencyProperty AtDesignTimeProperty =
            DependencyProperty.RegisterAttached(
                "AtDesignTime",
                typeof (bool),
                typeof (Bind),
                new PropertyMetadata(false, AtDesignTimeChanged)
                );

        /// <summary>
        /// Gets whether or not conventions are being applied at design-time.
        /// </summary>
        /// <param name="dependencyObject">The ui to apply conventions to.</param>
        /// <returns>Whether or not conventions are applied.</returns>
#if !SILVERLIGHT && !NETFX_CORE
        [AttachedPropertyBrowsableForTypeAttribute(typeof (DependencyObject))]
#endif
        public static bool GetAtDesignTime(DependencyObject dependencyObject)
        {
            return (bool) dependencyObject.GetValue(AtDesignTimeProperty);
        }

        /// <summary>
        /// Sets whether or not do bind conventions at design-time.
        /// </summary>
        /// <param name="dependencyObject">The ui to apply conventions to.</param>
        /// <param name="value">Whether or not to apply conventions.</param>
        public static void SetAtDesignTime(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(AtDesignTimeProperty, value);
        }

        private static void AtDesignTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!UIContext.IsInDesignTool)
                return;

            var atDesignTime = (bool) e.NewValue;
            if (!atDesignTime)
                return;

            BindingOperations.SetBinding(d, DataContextProperty, new Binding());
        }

        private static readonly DependencyProperty DataContextProperty =
            DependencyProperty.RegisterAttached(
                "DataContext",
                typeof (object),
                typeof (Bind),
                new PropertyMetadata(null, DataContextChanged)
                );

        private static void DataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!UIContext.IsInDesignTool)
                return;

            var enable = d.GetValue(AtDesignTimeProperty);
            if (enable == null || ((bool) enable) == false || e.NewValue == null)
                return;

            var fe = d as FrameworkElement;
            if (fe == null)
                return;

            ViewModelBinder.Bind(e.NewValue, d,
                string.IsNullOrEmpty(fe.Name) ? fe.GetHashCode().ToString(CultureInfo.InvariantCulture) : fe.Name);
        }
    }
}
