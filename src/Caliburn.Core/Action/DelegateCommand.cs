using System;
using System.ComponentModel;

namespace Caliburn.Light
{
    /// <summary>
    /// A delegate command that can be data-bound.
    /// </summary>
    public sealed class DelegateCommand : BindableCommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;
        private readonly string[] _propertyNames;
        private readonly IDisposable _propertyChangedRegistration;

        /// <summary>
        /// Initializes a new instance of <see cref="DelegateCommand"/>.
        /// </summary>
        /// <param name="execute">The execute function.</param>
        /// <param name="canExecute">The canExecute function.</param>
        public DelegateCommand(Action execute, Func<bool> canExecute = null)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DelegateCommand"/>.
        /// </summary>
        /// <param name="execute">The execute function.</param>
        /// <param name="canExecute">The canExecute function.</param>
        /// <param name="target">The object to observe for change notifications.</param>
        /// <param name="propertyNames">The property names.</param>
        public DelegateCommand(Action execute, Func<bool> canExecute, INotifyPropertyChanged target, params string[] propertyNames)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            if (canExecute == null)
                throw new ArgumentNullException(nameof(canExecute));
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (propertyNames == null || propertyNames.Length == 0)
                throw new ArgumentNullException(nameof(propertyNames));

            _execute = execute;
            _canExecute = canExecute;
            _propertyNames = propertyNames;
            _propertyChangedRegistration = target.RegisterPropertyChangedWeak(this, (t, s, e) => t.OnPropertyChanged(e));
        }

        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || Array.IndexOf(_propertyNames, e.PropertyName) >= 0)
                OnCanExecuteChanged();
        }

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        protected override bool CanExecuteCore(object parameter)
        {
            if (_canExecute == null) return true;
            return _canExecute();
        }

        /// <summary>
        /// Called when the command is invoked.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        public override void Execute(object parameter)
        {
            _execute();
        }
    }

    /// <summary>
    /// A delegate command that can be data-bound.
    /// </summary>
    /// <typeparam name="TParameter">The type of the command parameter.</typeparam>
    public sealed class DelegateCommand<TParameter> : BindableCommand
    {
        private readonly Func<object, TParameter> _coerceParameter;
        private readonly Action<TParameter> _execute;
        private readonly Func<TParameter, bool> _canExecute;
        private readonly string[] _propertyNames;
        private readonly IDisposable _propertyChangedRegistration;

        /// <summary>
        /// Initializes a new instance of <see cref="DelegateCommand&lt;TParameter&gt;"/>.
        /// </summary>
        /// <param name="coerceParameter">The function to coerce the provided value to <typeparamref name="TParameter"/>.</param>
        /// <param name="execute">The execute function.</param>
        /// <param name="canExecute">The canExecute function.</param>
        public DelegateCommand(Func<object, TParameter> coerceParameter, Action<TParameter> execute, Func<TParameter, bool> canExecute = null)
        {
            if (coerceParameter == null)
                throw new ArgumentNullException(nameof(coerceParameter));
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            _coerceParameter = coerceParameter;
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DelegateCommand&lt;TParameter&gt;"/>.
        /// </summary>
        /// <param name="coerceParameter">The function to coerce the provided value to <typeparamref name="TParameter"/>.</param>
        /// <param name="execute">The execute function.</param>
        /// <param name="canExecute">The canExecute function.</param>
        /// <param name="target">The object to observe for change notifications.</param>
        /// <param name="propertyNames">The property names.</param>
        public DelegateCommand(Func<object, TParameter> coerceParameter, Action<TParameter> execute, Func<TParameter, bool> canExecute,
            INotifyPropertyChanged target, params string[] propertyNames)
        {
            if (coerceParameter == null)
                throw new ArgumentNullException(nameof(coerceParameter));
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            if (canExecute == null)
                throw new ArgumentNullException(nameof(canExecute));
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (propertyNames == null || propertyNames.Length == 0)
                throw new ArgumentNullException(nameof(propertyNames));

            _coerceParameter = coerceParameter;
            _execute = execute;
            _canExecute = canExecute;
            _propertyNames = propertyNames;
            _propertyChangedRegistration = target.RegisterPropertyChangedWeak(this, (t, s, e) => t.OnPropertyChanged(e));
        }

        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || Array.IndexOf(_propertyNames, e.PropertyName) >= 0)
                OnCanExecuteChanged();
        }

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        protected override bool CanExecuteCore(object parameter)
        {
            if (_canExecute == null) return true;
            var value = _coerceParameter(parameter);
            return _canExecute(value);
        }

        /// <summary>
        /// Called when the command is invoked.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        public override void Execute(object parameter)
        {
            var value = _coerceParameter(parameter);
            _execute(value);
        }
    }
}
