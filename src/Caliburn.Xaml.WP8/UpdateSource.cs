using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Caliburn.Light
{
    /// <summary>
    /// Hosts attached properties related to <see cref="Binding"/> and <see cref="UpdateSourceTrigger"/>.
    /// </summary>
    public static class UpdateSource
    {
        /// <summary>
        /// Allows to update the binding source when the property changes.
        /// </summary>
        public static readonly DependencyProperty
            OnPropertyChangedProperty =
                DependencyProperty.RegisterAttached(
                    "OnPropertyChanged",
                    typeof (bool),
                    typeof (UpdateSource),
                    new PropertyMetadata(false, OnPropertyChanged));

        /// <summary>
        /// Gets wether the binding source should be updated when a property changes.
        /// </summary>
        /// <param name="obj">The view.</param>
        /// <returns>Wether or not the source sould be updated.</returns>
        public static bool GetOnPropertyChanged(DependencyObject obj)
        {
            return (bool)obj.GetValue(OnPropertyChangedProperty);
        }

        /// <summary>
        /// Sets wether the binding source should be updated when a property changes.
        /// </summary>
        /// <param name="obj">The view.</param>
        /// <param name="value">Wether or not the source sould be updated.</param>
        public static void SetOnPropertyChanged(DependencyObject obj, bool value)
        {
            obj.SetValue(OnPropertyChangedProperty, value);
        }

        private static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!ViewHelper.IsInDesignTool || e.NewValue == e.OldValue) return;

            var textBox = obj as TextBox;
            if (textBox != null)
            {
                if ((bool)e.NewValue)
                {
                    textBox.TextChanged += OnTextChanged;
                }
                else
                {
                    textBox.TextChanged -= OnTextChanged;
                }
            }

            var passwordBox = obj as PasswordBox;
            if (passwordBox != null)
            {
                if ((bool)e.NewValue)
                {
                    passwordBox.PasswordChanged += OnPasswordChanged;
                }
                else
                {
                    passwordBox.PasswordChanged -= OnPasswordChanged;
                }
            }
        }

        private static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox) sender;
            var bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
            if (bindingExpression != null)
            {
                bindingExpression.UpdateSource();
            }
        }

        private static void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox) sender;
            var bindingExpression = passwordBox.GetBindingExpression(PasswordBox.PasswordProperty);
            if (bindingExpression != null)
            {
                bindingExpression.UpdateSource();
            }
        }
    }
}
