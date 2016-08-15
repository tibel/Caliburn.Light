using System;
using System.ComponentModel;
using Weakly;

namespace Caliburn.Light
{
    internal sealed class DelegateCommandImpl<TParameter> : BindableCommand
    {
        private readonly Func<object, TParameter> _coerceParameter;
        private readonly Action<TParameter> _execute;
        private readonly Func<TParameter, bool> _canExecute;
        private readonly string[] _propertyNames;
        private readonly IDisposable _propertyChangedRegistration;

        public DelegateCommandImpl(Func<object, TParameter> coerceParameter, Action<TParameter> execute, Func<TParameter, bool> canExecute, 
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
                OnCanExecuteChanged();
            }
        }

        protected override bool CanExecuteCore(object parameter)
        {
            if (_canExecute == null) return true;
            var value = _coerceParameter(parameter);
            return _canExecute(value);
        }

        public override void Execute(object parameter)
        {
            var value = _coerceParameter(parameter);
            _execute(value);
        }
    }
}
