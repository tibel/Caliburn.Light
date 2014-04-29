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
    /// Hosts attached properties related to View-First.
    /// </summary>
    public static class Bind
    {
        /// <summary>
        /// Allows binding on an existing view. Use this on root UserControls, Pages and Windows; not in a DataTemplate.
        /// </summary>
        public static DependencyProperty ModelProperty = DependencyProperty.RegisterAttached("Model", typeof (object),
            typeof (Bind), new PropertyMetadata(null, OnModelChanged));

        /// <summary>
        /// Allows binding on an existing view without setting the data context. Use this from within a DataTemplate.
        /// </summary>
        public static DependencyProperty ModelWithoutContextProperty =
            DependencyProperty.RegisterAttached("ModelWithoutContext", typeof (object), typeof (Bind),
                new PropertyMetadata(null, OnModelWithoutContextChanged));

        internal static DependencyProperty NoDataContextProperty = DependencyProperty.RegisterAttached("NoDataContext",
            typeof (bool), typeof (Bind), null);

        /// <summary>
        /// Gets the model to bind to.
        /// </summary>
        /// <param name = "dependencyObject">The dependency object to bind to.</param>
        /// <returns>The model.</returns>
        public static object GetModelWithoutContext(DependencyObject dependencyObject)
        {
            return dependencyObject.GetValue(ModelWithoutContextProperty);
        }

        /// <summary>
        /// Sets the model to bind to.
        /// </summary>
        /// <param name = "dependencyObject">The dependency object to bind to.</param>
        /// <param name = "value">The model.</param>
        public static void SetModelWithoutContext(DependencyObject dependencyObject, object value)
        {
            dependencyObject.SetValue(ModelWithoutContextProperty, value);
        }

        /// <summary>
        /// Gets the model to bind to.
        /// </summary>
        /// <param name = "dependencyObject">The dependency object to bind to.</param>
        /// <returns>The model.</returns>
        public static object GetModel(DependencyObject dependencyObject)
        {
            return dependencyObject.GetValue(ModelProperty);
        }

        /// <summary>
        /// Sets the model to bind to.
        /// </summary>
        /// <param name = "dependencyObject">The dependency object to bind to.</param>
        /// <param name = "value">The model.</param>
        public static void SetModel(DependencyObject dependencyObject, object value)
        {
            dependencyObject.SetValue(ModelProperty, value);
        }

        private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (UIContext.IsInDesignTool || e.NewValue == null || e.NewValue == e.OldValue)
                return;

            var fe = d as FrameworkElement;
            if (fe == null) return;

            ViewHelper.ExecuteOnLoad(fe, (sender, args) => SetModelCore(e.NewValue, fe, false));
        }

        private static void OnModelWithoutContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (UIContext.IsInDesignTool || e.NewValue == null || e.NewValue == e.OldValue)
                return;

            var fe = d as FrameworkElement;
            if (fe == null) return;

            ViewHelper.ExecuteOnLoad(fe, (sender, args) => SetModelCore(e.NewValue, fe, true));
        }

        private static void SetModelCore(object viewModel, FrameworkElement view, bool noDataContext)
        {
            var context = string.IsNullOrEmpty(view.Name)
                ? view.GetHashCode().ToString(CultureInfo.InvariantCulture)
                : view.Name;

            if (noDataContext)
                view.SetValue(NoDataContextProperty, true);

            ViewModelBinder.Bind(viewModel, view, context);
        }

        /// <summary>
        /// Allows application of conventions at design-time.
        /// </summary>
        public static DependencyProperty AtDesignTimeProperty = DependencyProperty.RegisterAttached("AtDesignTime",
            typeof (bool), typeof (Bind), new PropertyMetadata(false, OnAtDesignTimeChanged));

        private static readonly DependencyProperty DesignDataContextProperty =
            DependencyProperty.RegisterAttached("DesignDataContext", typeof (object), typeof (Bind),
                new PropertyMetadata(null, OnDesignDataContextChanged));

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

        private static void OnAtDesignTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!UIContext.IsInDesignTool) return;

            var enabled = (bool) e.NewValue;
            if (!enabled) return;

            BindingOperations.SetBinding(d, DesignDataContextProperty, new Binding());
        }

        private static void OnDesignDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!UIContext.IsInDesignTool || e.NewValue == null) return;

            var enabled = (bool) d.GetValue(AtDesignTimeProperty);
            if (!enabled) return;

            var view = d as FrameworkElement;
            if (view == null) return;

            var context = string.IsNullOrEmpty(view.Name)
                ? view.GetHashCode().ToString(CultureInfo.InvariantCulture)
                : view.Name;

            ViewModelBinder.Bind(e.NewValue, view, context);
        }
    }
}
