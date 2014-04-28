using Caliburn.Light;
using System;
#if NETFX_CORE
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Controls;
#endif

namespace Caliburn.Xaml
{
    /// <summary>
    /// Hosts attached properties related to view models.
    /// </summary>
    public static class View
    {
        /// <summary>
        /// A dependency property for assigning a context to a particular portion of the UI.
        /// </summary>
        public static readonly DependencyProperty ContextProperty =
            DependencyProperty.RegisterAttached(
                "Context",
                typeof (object),
                typeof (View),
                new PropertyMetadata(null, OnContextChanged)
                );

        /// <summary>
        /// A dependency property for attaching a model to the UI.
        /// </summary>
        public static DependencyProperty ModelProperty =
            DependencyProperty.RegisterAttached(
                "Model",
                typeof (object),
                typeof (View),
                new PropertyMetadata(null, OnModelChanged)
                );

        

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
        public static object GetContext(DependencyObject d)
        {
            return d.GetValue(ContextProperty);
        }

        /// <summary>
        /// Sets the context.
        /// </summary>
        /// <param name="d">The element to attach the context to.</param>
        /// <param name="value">The context.</param>
        public static void SetContext(DependencyObject d, object value)
        {
            d.SetValue(ContextProperty, value);
        }

        private static void OnModelChanged(DependencyObject targetLocation, DependencyPropertyChangedEventArgs args)
        {
            if (args.OldValue == args.NewValue)
            {
                return;
            }

            if (args.NewValue != null)
            {
                var context = GetContext(targetLocation);
                var view = ViewLocator.LocateForModel(args.NewValue, targetLocation, context);

                SetContentProperty(targetLocation, view);
                ViewModelBinder.Bind(args.NewValue, view, context);
            }
            else
            {
                SetContentProperty(targetLocation, null);
            }
        }

        private static void OnContextChanged(DependencyObject targetLocation, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue)
            {
                return;
            }

            var model = GetModel(targetLocation);
            if (model == null)
            {
                return;
            }

            var view = ViewLocator.LocateForModel(model, targetLocation, e.NewValue);

            SetContentProperty(targetLocation, view);
            ViewModelBinder.Bind(model, view, e.NewValue);
        }

        private static void SetContentProperty(object targetLocation, object view)
        {
            var fe = view as FrameworkElement;
            if (fe != null && fe.Parent != null)
            {
                SetContentPropertyCore(fe.Parent, null);
            }

            SetContentPropertyCore(targetLocation, view);
        }

        private static void SetContentPropertyCore(object targetLocation, object view)
        {
            try
            {
                var contentProperty = ViewHelper.FindContentProperty(targetLocation);
                contentProperty.SetValue(targetLocation, view);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(View)).Error("Failed to set Content property. {0}", e);
            }
        }
    }
}
