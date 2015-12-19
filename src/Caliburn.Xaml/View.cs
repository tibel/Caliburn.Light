using System;
#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using System.Windows;
using System.Windows.Controls;
#endif

namespace Caliburn.Light
{
    /// <summary>
    /// Hosts attached properties related to ViewModel-First.
    /// </summary>
    public static class View
    {
        /// <summary>
        /// A dependency property for assigning a context to a particular portion of the UI.
        /// </summary>
        public static readonly DependencyProperty ContextProperty = DependencyProperty.RegisterAttached("Context",
            typeof (string), typeof (View), new PropertyMetadata(null, OnContextChanged));

        /// <summary>
        /// A dependency property for attaching a model to the UI.
        /// </summary>
        public static readonly DependencyProperty ModelProperty = DependencyProperty.RegisterAttached("Model",
            typeof (object), typeof (View), new PropertyMetadata(null, OnModelChanged));

        /// <summary>
        /// Sets the model.
        /// </summary>
        /// <param name="d">The element to attach the model to.</param>
        /// <param name="value">The model.</param>
        public static void SetModel(DependencyObject d, object value)
        {
            d.SetValue(ModelProperty, value);
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <param name="d">The element the model is attached to.</param>
        /// <returns>The model.</returns>
        public static object GetModel(DependencyObject d)
        {
            return d.GetValue(ModelProperty);
        }

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <param name="d">The element the context is attached to.</param>
        /// <returns>The context.</returns>
        public static string GetContext(DependencyObject d)
        {
            return (string) d.GetValue(ContextProperty);
        }

        /// <summary>
        /// Sets the context.
        /// </summary>
        /// <param name="d">The element to attach the context to.</param>
        /// <param name="value">The context.</param>
        public static void SetContext(DependencyObject d, string value)
        {
            d.SetValue(ContextProperty, value);
        }

        private static void OnModelChanged(DependencyObject targetLocation, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue) return;

            if (e.NewValue == null)
            {
                SetContentProperty(targetLocation, null);
            }
            else
            {
                var context = GetContext(targetLocation);
                SetContentCore(targetLocation, e.NewValue, context);
            }
        }

        private static void OnContextChanged(DependencyObject targetLocation, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue) return;

            var model = GetModel(targetLocation);
            if (model == null) return;

            var context = (string) e.NewValue;
            SetContentCore(targetLocation, model, context);
        }

        private static void SetContentCore(DependencyObject targetLocation, object model, string context)
        {
            if (ViewHelper.IsInDesignTool)
            {
                var placeholder = new TextBlock { Text = string.Format("View for {0}", model.GetType()) };
                SetContentProperty(targetLocation, placeholder);
                return;
            }

            var viewModelLocator = IoC.GetInstance<IViewModelLocator>();
            var viewModelBinder = IoC.GetInstance<IViewModelBinder>();

            var view = viewModelLocator.LocateForModel(model, context);
            RemoveFromParent(view);

            viewModelBinder.Bind(model, view, context);
            SetContentProperty(targetLocation, view);
        }

        private static void RemoveFromParent(DependencyObject view)
        {
            var fe = view as FrameworkElement;
            if (fe != null && fe.Parent != null)
            {
                SetContentProperty(fe.Parent, null);
            }
        }

        private static void SetContentProperty(DependencyObject targetLocation, DependencyObject view)
        {
            var contentControl = targetLocation as ContentControl;
            if (contentControl == null)
                throw new NotSupportedException("Only ContentControl is supported.");

            contentControl.Content = view;
        }
    }
}
