using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Weakly;

namespace Caliburn.Light
{
    internal sealed class AsyncDelegateCommandImpl<TParameter> : AsyncCommand
    {
        private readonly Func<object, TParameter> _coerceParameter;
        private readonly Func<TParameter, Task> _execute;
        private readonly Func<TParameter, bool> _canExecute;
        private readonly string[] _propertyNames;
        private readonly IDisposable _propertyChangedRegistration;

        public AsyncDelegateCommandImpl(Func<object, TParameter> coerceParameter, Func<TParameter, Task> execute, Func<TParameter, bool> canExecute, 
            INotifyPropertyChanged target, string[] propertyNames)
        {
            _coerceParameter = coerceParameter;
            _execute = execute;
            _canExecute = canExecute;
            _propertyNames = propertyNames;

            if (target != null)
            {
                _propertyChangedRegistration = target.RegisterPropertyChangedWeak(this, (t, s, e) => t.OnPropertyChanged(e));
            }
        }

        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || Array.IndexOf(_propertyNames, e.PropertyName) >= 0)
            {
                RaiseCanExecuteChanged();
            }
        }

        protected override bool CanExecuteCore(object parameter)
        {
            if (IsExecuting) return false;
            if (_canExecute == null) return true;
            var value = _coerceParameter(parameter);
            return _canExecute(value);
        }

        protected override Task ExecuteAsync(object parameter)
        {
            var value = _coerceParameter(parameter);
            return _execute(value);
        }
    }
}
