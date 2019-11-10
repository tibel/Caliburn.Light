using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Caliburn.Light.WPF
{
    /// <summary>
    /// Hosts attached properties related to ViewModel-First.
    /// </summary>
    public static class View
    {
        /// <summary>
        /// A dependency property for assigning a <see cref="IViewModelLocator"/> to a particular portion of the UI.
        /// </summary>
        public static readonly DependencyProperty ViewModelLocatorProperty =
            DependencyProperty.RegisterAttached("ViewModelLocator",
                typeof(IViewModelLocator), typeof(View), null);

        /// <summary>
        /// Gets the attached <see cref="IViewModelLocator"/>.
        /// </summary>
        /// <param name="d">The element the <see cref="IViewModelLocator"/> is attached to.</param>
        /// <returns>The <see cref="IViewModelLocator"/>.</returns>
        public static IViewModelLocator GetViewModelLocator(DependencyObject d)
        {
            return (IViewModelLocator)d.GetValue(ViewModelLocatorProperty);
        }

        /// <summary>
        /// Sets the <see cref="IViewModelLocator"/>.
        /// </summary>
        /// <param name="d">The element to attach the <see cref="IViewModelLocator"/> to.</param>
        /// <param name="value">The <see cref="IViewModelLocator"/>.</param>
        public static void SetViewModelLocator(DependencyObject d, IViewModelLocator value)
        {
            d.SetValue(ViewModelLocatorProperty, value);
        }

        private static IViewModelLocator GetCurrentViewModelLocator(DependencyObject d)
        {
            var viewModelLocator = GetViewModelLocator(d);

            while (viewModelLocator is null)
            {
                d = VisualTreeHelper.GetParent(d);
                if (d is null)
                    break;

                viewModelLocator = GetViewModelLocator(d);
            }

            return viewModelLocator;
        }

        /// <summary>
        /// A dependency property for attaching a model to the UI.
        /// </summary>
        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.RegisterAttached("Model",
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

        private static void OnModelChanged(DependencyObject targetLocation, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue) return;
            SetContentCore(targetLocation, e.NewValue, GetContext(targetLocation));
        }

        /// <summary>
        /// A dependency property for assigning a context to a particular portion of the UI.
        /// </summary>
        public static readonly DependencyProperty ContextProperty =
            DependencyProperty.RegisterAttached("Context",
                typeof(string), typeof(View), new PropertyMetadata(null, OnContextChanged));

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

        private static void OnContextChanged(DependencyObject targetLocation, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue) return;
            SetContentCore(targetLocation, GetModel(targetLocation), (string)e.NewValue);
        }

        private static void SetContentCore(DependencyObject targetLocation, object model, string context)
        {
            if (model is null)
            {
                SetContentProperty(targetLocation, null);
                return;
            }

            if (DesignMode.DesignModeEnabled)
            {
                var placeholder = new TextBlock { Text = string.Format("View for {0}", model.GetType()) };
                SetContentProperty(targetLocation, placeholder);
                return;
            }

            var viewModelLocator = GetCurrentViewModelLocator(targetLocation);
            if (viewModelLocator is null)
                throw new InvalidOperationException("Could not find 'IViewModelLocator' in control hierarchy.");

            var view = viewModelLocator.LocateForModel(model, context);

            if (view is FrameworkElement fe)
            {
                if (IsCurrentView(targetLocation, view) && fe.DataContext is IViewAware currentViewAware)
                    currentViewAware.DetachView(view, context);

                fe.DataContext = model;
            }

            if (model is IViewAware viewAware)
                viewAware.AttachView(view, context);

            SetContentProperty(targetLocation, view);
        }

        private static bool IsCurrentView(DependencyObject targetLocation, UIElement view)
        {
            var currentView = targetLocation is ContentControl contentControl ? contentControl.Content : null;
            return currentView is object && ReferenceEquals(currentView, view);
        }

        private static void SetContentProperty(DependencyObject targetLocation, UIElement view)
        {
            if (targetLocation is ContentControl contentControl)
                contentControl.Content = view;
            else
                throw new NotSupportedException("Only ContentControl is supported.");
        }

        /// <summary>
        /// A dependency property for marking a view as generated.
        /// </summary>
        public static readonly DependencyProperty IsGeneratedProperty =
            DependencyProperty.RegisterAttached("IsGenerated",
                typeof(bool), typeof(View), new PropertyMetadata(BooleanBoxes.FalseBox));

        /// <summary>
        /// Gets the IsGenerated property for <paramref name="view"/>.
        /// </summary>
        /// <param name="view">The view to search.</param>
        /// <returns>Whether the supplied view is generated.</returns>
        public static bool GetIsGenerated(DependencyObject view)
        {
            return (bool)view.GetValue(IsGeneratedProperty);
        }

        /// <summary>
        /// Sets the IsGenerated property for <paramref name="view"/>.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="value">true, if the view is generated by the framework.</param>
        public static void SetIsGenerated(DependencyObject view, bool value)
        {
            view.SetValue(IsGeneratedProperty, BooleanBoxes.Box(value));
        }

        private static readonly DependencyProperty PreviouslyAttachedProperty =
            DependencyProperty.RegisterAttached("PreviouslyAttached",
                typeof(bool), typeof(View), new PropertyMetadata(BooleanBoxes.FalseBox));

        /// <summary>
        /// Executes the handler the fist time the element is loaded.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="handler">The handler.</param>
        public static void ExecuteOnFirstLoad(FrameworkElement element, Action<FrameworkElement> handler)
        {
            if ((bool)element.GetValue(PreviouslyAttachedProperty)) return;
            element.SetValue(PreviouslyAttachedProperty, BooleanBoxes.TrueBox);
            ExecuteOnLoad(element, handler);
        }

        /// <summary>
        /// Executes the handler immediately if the element is loaded, otherwise wires it to the Loaded event.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="handler">The handler.</param>
        /// <returns>true if the handler was executed immediately; false otherwise</returns>
        public static bool ExecuteOnLoad(FrameworkElement element, Action<FrameworkElement> handler)
        {
            if (IsElementLoaded(element))
            {
                handler(element);
                return true;
            }

            RoutedEventHandler loaded = null;
            loaded = delegate
            {
                element.Loaded -= loaded;
                handler(element);
            };
            element.Loaded += loaded;
            return false;
        }

        /// <summary>
        /// Executes the handler when the element is unloaded.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="handler">The handler.</param>
        public static void ExecuteOnUnload(FrameworkElement element, Action<FrameworkElement> handler)
        {
            RoutedEventHandler unloaded = null;
            unloaded = delegate
            {
                element.Unloaded -= unloaded;
                handler(element);
            };
            element.Unloaded += unloaded;
        }

        /// <summary>
        /// Executes the handler the next time the elements's LayoutUpdated event fires.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="handler">The handler.</param>
        public static void ExecuteOnLayoutUpdated(FrameworkElement element, Action<FrameworkElement> handler)
        {
            EventHandler onLayoutUpdate = null;
            onLayoutUpdate = delegate
            {
                element.LayoutUpdated -= onLayoutUpdate;
                handler(element);
            };
            element.LayoutUpdated += onLayoutUpdate;
        }

        /// <summary>
        /// Determines whether the specified <paramref name="element"/> is loaded.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>true if the element is loaded; otherwise, false.
        /// </returns>
        public static bool IsElementLoaded(FrameworkElement element)
        {
            return element.IsLoaded;
        }
    }
}
