using System;
using System.Windows.Input;
#if !NETFX_CORE
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#endif

namespace Caliburn.Xaml
{
    /// <summary>
    /// Executes a specified ICommand when invoked.
    /// It also maintains the Enabled state of the target element based on the CanExecute method of the command.
    /// </summary>
    public class InvokeCommandAction : TriggerAction<UIElement>
    {
        /// <summary>
        /// Identifies the <seealso cref="InvokeCommandAction.Command"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command",
            typeof (ICommand), typeof (InvokeCommandAction),
            new PropertyMetadata(null, OnCommandChanged));

        /// <summary>
        /// Identifies the <seealso cref="InvokeCommandAction.CommandParameter"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof (object), typeof (InvokeCommandAction),
                new PropertyMetadata(null, OnCommandParameterChanged));

        /// <summary>
        /// Gets or sets the command to execute when invoked.
        /// </summary>
        public ICommand Command
        {
            get { return GetValue(CommandProperty) as ICommand; }
            set { SetValue(CommandProperty, value); }
        }

        /// <summary>
        /// Gets or sets the command parameter to supply on command execution.
        /// </summary>
        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        /// <summary>
        /// This method is called after the behavior is attached.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            UpdateEnabledState();
        }

        /// <summary>
        /// Sets the Command and CommandParameter properties to null.
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            Command = null;
            CommandParameter = null;
        }

        /// <summary>
        /// Updates the Enabled state of the target element based on the CanExecute method of the command.
        /// </summary>
        protected virtual void UpdateEnabledState()
        {
            if (AssociatedObject != null && Command != null)
            {
                var canExecute = Command.CanExecute(CommandParameter);

#if SILVERLIGHT || NETFX_CORE
                var control = AssociatedObject as Control;
                if (control != null)
                    control.IsEnabled = canExecute;
#else
                AssociatedObject.IsEnabled = canExecute;
#endif
            }
        }

        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="parameter">This parameter is passed to the command; the CommandParameter specified in the CommandParameterProperty is used for command invocation if not null.</param>
        protected override void Invoke(object parameter)
        {
            if (Command != null)
            {
                if (CommandParameter != null)
                {
                    Command.Execute(CommandParameter);
                }
                else
                {
                    Command.Execute(parameter);
                }
            }
        }

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue) return;

            var action = (InvokeCommandAction)d;
            var oldValue = (ICommand) e.OldValue;
            var newValue = (ICommand) e.NewValue;

            if (oldValue != null)
            {
                oldValue.CanExecuteChanged -= action.OnCommandCanExecuteChanged;
            }

            if (newValue != null)
            {
                newValue.CanExecuteChanged += action.OnCommandCanExecuteChanged;
                action.UpdateEnabledState();
            }
        }

        private static void OnCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue) return;

            var action = (InvokeCommandAction)d;
            action.UpdateEnabledState();
        }

        private void OnCommandCanExecuteChanged(object sender, EventArgs e)
        {
            UpdateEnabledState();
        }
    }
}
