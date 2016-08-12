using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Weakly;

namespace Caliburn.Light
{
    /// <summary>
    /// Builds an <see cref="ICommand"/> in a strongly typed fashion.
    /// </summary>
    public sealed class DelegateCommandBuilder
    {
        private Func<Task> _execute;
        private Func<bool> _canExecute;
        private INotifyPropertyChanged _target;
        private string[] _propertyNames;

        internal DelegateCommandBuilder()
        {
        }

        /// <summary>
        /// Sets the command execute function.
        /// </summary>
        /// <param name="execute">The execute function.</param>
        /// <returns>Itself</returns>
        public DelegateCommandBuilder OnExecute(Action execute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            if (_execute != null)
                throw new InvalidOperationException("Execute already set.");

            _execute = () =>
            {
                execute();
                return TaskHelper.Completed();
            };

            return this;
        }

        /// <summary>
        /// Sets the command execute function.
        /// </summary>
        /// <param name="execute">The execute function.</param>
        /// <returns>Itself</returns>
        public DelegateCommandBuilder OnExecute(Func<Task> execute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            if (_execute != null)
                throw new InvalidOperationException("Execute already set.");

            _execute = execute;
            return this;
        }

        /// <summary>
        /// Sets the command canExecute function.
        /// </summary>
        /// <param name="canExecute">The canExecute function.</param>
        /// <returns>Itself</returns>
        public DelegateCommandBuilder OnCanExecute(Func<bool> canExecute)
        {
            if (canExecute == null)
                throw new ArgumentNullException(nameof(canExecute));
            if (_canExecute != null)
                throw new InvalidOperationException("CanExecute already set.");

            _canExecute = canExecute;
            return this;
        }

        /// <summary>
        /// Sets the properties to listen for change notifications.
        /// </summary>
        /// <param name="target">The object to observe.</param>
        /// <param name="propertyNames">The property names.</param>
        /// <returns>Itself</returns>
        public DelegateCommandBuilder Observe(INotifyPropertyChanged target, params string[] propertyNames)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (_target != null)
                throw new InvalidOperationException("Observe already set.");
            if (propertyNames == null)
                throw new ArgumentNullException(nameof(propertyNames));
            if (propertyNames.Length == 0 || Array.IndexOf(propertyNames, string.Empty) >= 0)
                throw new ArgumentException("List of properties is empty.", nameof(propertyNames));

            _target = target;
            _propertyNames = propertyNames;
            return this;
        }

        /// <summary>
        /// Builds the command.
        /// </summary>
        /// <returns>The newly build command.</returns>
        public AsyncCommand Build()
        {
            if (_execute == null)
                throw new InvalidOperationException("Execute not set.");
            if (_target != null && _canExecute == null)
                throw new InvalidOperationException("CanExecute not set but Observe used.");

            Func<object, bool> canExecute = null;
            if (_canExecute != null)
                canExecute = p => _canExecute();

            return new AsyncDelegateCommandImpl<object>(p => p, p => _execute(), canExecute, _target, _propertyNames);
        }
    }

    /// <summary>
    /// Builds an <see cref="ICommand"/> in a strongly typed fashion.
    /// </summary>
    /// <typeparam name="TParameter">The type of the command parameter.</typeparam>
    public sealed class DelegateCommandBuilder<TParameter>
    {
        private readonly Func<object, TParameter> _coerceParameter;
        private Func<TParameter, Task> _execute;
        private Func<TParameter, bool> _canExecute;
        private INotifyPropertyChanged _target;
        private string[] _propertyNames;

        internal DelegateCommandBuilder(Func<object, TParameter> coerceParameter)
        {
            _coerceParameter = coerceParameter ?? CoerceParameter;
        }

        private static TParameter CoerceParameter(object parameter)
        {
            if (parameter == null)
                return default(TParameter);

            var specialValue = parameter as ISpecialValue;
            if (specialValue != null)
                parameter = specialValue.Resolve(new CoroutineExecutionContext());

            if (parameter is TParameter)
                return (TParameter)parameter;

            return (TParameter)Convert.ChangeType(parameter, typeof(TParameter));
        }

        /// <summary>
        /// Sets the command execute function.
        /// </summary>
        /// <param name="execute">The execute function.</param>
        /// <returns>Itself</returns>
        public DelegateCommandBuilder<TParameter> OnExecute(Action<TParameter> execute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            if (_execute != null)
                throw new InvalidOperationException("Execute already set.");

            _execute = p =>
            {
                execute(p);
                return TaskHelper.Completed();
            };

            return this;
        }

        /// <summary>
        /// Sets the command execute function.
        /// </summary>
        /// <param name="execute">The execute function.</param>
        /// <returns>Itself</returns>
        public DelegateCommandBuilder<TParameter> OnExecute(Func<TParameter, Task> execute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            if (_execute != null)
                throw new InvalidOperationException("Execute already set.");

            _execute = execute;
            return this;
        }

        /// <summary>
        /// Sets the command canExecute function.
        /// </summary>
        /// <param name="canExecute">The canExecute function.</param>
        /// <returns>Itself</returns>
        public DelegateCommandBuilder<TParameter> OnCanExecute(Func<bool> canExecute)
        {
            if (canExecute == null)
                throw new ArgumentNullException(nameof(canExecute));
            if (_canExecute != null)
                throw new InvalidOperationException("CanExecute already set.");

            _canExecute = p => canExecute();
            return this;
        }

        /// <summary>
        /// Sets the command canExecute function.
        /// </summary>
        /// <param name="canExecute">The canExecute function.</param>
        /// <returns>Itself</returns>
        public DelegateCommandBuilder<TParameter> OnCanExecute(Func<TParameter, bool> canExecute)
        {
            if (canExecute == null)
                throw new ArgumentNullException(nameof(canExecute));
            if (_canExecute != null)
                throw new InvalidOperationException("CanExecute already set.");

            _canExecute = canExecute;
            return this;
        }

        /// <summary>
        /// Sets the properties to listen for change notifications.
        /// </summary>
        /// <param name="target">The object to observe.</param>
        /// <param name="propertyNames">The property names.</param>
        /// <returns>Itself</returns>
        public DelegateCommandBuilder<TParameter> Observe(INotifyPropertyChanged target, params string[] propertyNames)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (_target != null)
                throw new InvalidOperationException("Observe already set.");
            if (propertyNames == null)
                throw new ArgumentNullException(nameof(propertyNames));
            if (propertyNames.Length == 0 || Array.IndexOf(propertyNames, string.Empty) >= 0)
                throw new ArgumentException("List of properties is empty.", nameof(propertyNames));

            _target = target;
            _propertyNames = propertyNames;
            return this;
        }

        /// <summary>
        /// Builds the command.
        /// </summary>
        /// <returns>The newly build command.</returns>
        public AsyncCommand Build()
        {
            if (_execute == null)
                throw new InvalidOperationException("Execute not set.");
            if (_target != null && _canExecute == null)
                throw new InvalidOperationException("CanExecute not set but Observe used.");

            return new AsyncDelegateCommandImpl<TParameter>(_coerceParameter, _execute, _canExecute, _target, _propertyNames);
        }
    }
}
