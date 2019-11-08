using System.Windows;

namespace Caliburn.Light.WPF
{
    /// <summary>
    /// Hosts attached properties related to View-First.
    /// </summary>
    public static class Bind
    {
        /// <summary>
        /// The DependencyProperty for the CommandParameter used in x:Bind scenarios.
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter",
                typeof (object), typeof (Bind), null);

		/// <summary>
        /// Gets the command parameter
        /// </summary>
        /// <param name="dependencyObject">The dependency object to bind to.</param>
        /// <returns>The command parameter.</returns>
        public static object GetCommandParameter(DependencyObject dependencyObject)
        {
            return dependencyObject.GetValue(CommandParameterProperty);
        }

        /// <summary>
        /// Sets the command parameter.
        /// </summary>
        /// <param name="dependencyObject">The dependency object to bind to.</param>
        /// <param name="value">The command parameter.</param>
        public static void SetCommandParameter(DependencyObject dependencyObject, object value)
        {
            dependencyObject.SetValue(CommandParameterProperty, value);
        }

        /// <summary>
        /// Whether changing DataContext is tracked for <see cref="IViewAware"/>.
        /// </summary>
        public static readonly DependencyProperty DataContextProperty =
            DependencyProperty.RegisterAttached("DataContext",
                typeof(bool), typeof(Bind), new PropertyMetadata(BooleanBoxes.FalseBox, OnBindDataContextChanged));

        /// <summary>
        /// Gets if changing DataContext is tracked.
        /// </summary>
        /// <param name="dependencyObject">The dependency object to bind to.</param>
        /// <returns>Whether changing DataContext is tracked.</returns>
        public static bool GetDataContext(DependencyObject dependencyObject)
        {
            return (bool) dependencyObject.GetValue(DataContextProperty);
        }

        /// <summary>
        /// Sets if changing DataContext is tracked.
        /// </summary>
        /// <param name="dependencyObject">The dependency object to bind to.</param>
        /// <param name="value">Whether changing DataContext should be tracked.</param>
        public static void SetDataContext(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(DataContextProperty, BooleanBoxes.Box(value));
        }

        private static void OnBindDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (DesignMode.DesignModeEnabled || e.NewValue == e.OldValue || !(d is FrameworkElement fe))
                return;

            if ((bool)e.NewValue)
            {
                fe.DataContextChanged += OnDataContextChanged;
                OnDataContextChanged(fe, null, fe.DataContext);
            }
            else
            {
                fe.DataContextChanged -= OnDataContextChanged;
            }
        }

        private static void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            OnDataContextChanged((FrameworkElement)sender, e.OldValue, e.NewValue);
        }

        private static void OnDataContextChanged(FrameworkElement view, object oldValue, object newValue)
        {
            var context = string.IsNullOrEmpty(view.Name)
                ? null
                : view.Name;

            if (oldValue is IViewAware oldViewAware)
                oldViewAware.DetachView(view, context);

            if (newValue is IViewAware newViewAware)
                newViewAware.AttachView(view, context);
        }
    }
}
