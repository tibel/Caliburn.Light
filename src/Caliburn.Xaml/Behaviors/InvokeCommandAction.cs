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
using Weakly;

namespace Caliburn.Light
{
    /// <summary>
    /// Executes a specified ICommand when invoked.
    /// It also maintains the Enabled state of the target element based on the CanExecute method of the command.
    /// </summary>
    public class InvokeCommandAction : TriggerAction<DependencyObject>
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
            UpdateCommand();
            UpdateEnabledState();
        }

        /// <summary>
        /// Sets the Command and CommandParameter properties to null.
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            ResetCommand();
        }

        /// <summary>
        /// Updates the Enabled state of the target element based on the CanExecute method of the command.
        /// </summary>
        protected void UpdateEnabledState()
        {
            if (AssociatedObject == null) return;

            var canExecute = true;
            if (Command != null)
            {
                var resolvedParameter = CommandParameter;

                var specialValue = resolvedParameter as ISpecialValue;
                if (specialValue != null)
                {
                    var context = new CoroutineExecutionContext
                    {
                        Source = AssociatedObject,
                    };
                    resolvedParameter = specialValue.Resolve(context);
                }

                canExecute = Command.CanExecute(resolvedParameter);
            }

#if SILVERLIGHT || NETFX_CORE
            var control = AssociatedObject as Control;
            if (control != null && !BindingHelper.IsDataBound(control, Control.IsEnabledProperty))
                control.IsEnabled = canExecute;
#else
            var control = AssociatedObject as FrameworkElement;
            if (control != null && !BindingHelper.IsDataBound(control, UIElement.IsEnabledProperty))
                control.IsEnabled = canExecute;
#endif
        }

        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="parameter">This parameter is passed to the command; the CommandParameter specified in the CommandParameterProperty is used for command invocation if not null.</param>
        protected override void Invoke(object parameter)
        {
            if (Command != null)
            {
                var resolvedParameter = CommandParameter;

                var specialValue = resolvedParameter as ISpecialValue;
                if (specialValue != null)
                {
                    var context = new CoroutineExecutionContext
                    {
                        Source = AssociatedObject,
                        EventArgs = parameter,
                    };
                    resolvedParameter = specialValue.Resolve(context);
                }

                Command.Execute(resolvedParameter ?? parameter);
            }
        }

        private IDisposable _canExecuteChangedRegistration;

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var action = (InvokeCommandAction)d;
            action.UpdateCommand();
            action.UpdateEnabledState();
        }

        private static void OnCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var action = (InvokeCommandAction)d;
            action.UpdateEnabledState();
        }

        private void ResetCommand()
        {
            if (_canExecuteChangedRegistration != null)
            {
                _canExecuteChangedRegistration.Dispose();
                _canExecuteChangedRegistration = null;
            }
        }

        private void UpdateCommand()
        {
            ResetCommand();
            if (Command == null) return;
            _canExecuteChangedRegistration = Command.RegisterCanExecuteChangedWeak(this, (t, s, e) => t.UpdateEnabledState());
        }
    }
}
