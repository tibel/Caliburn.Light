using System;
using System.ComponentModel;
using Weakly;

namespace Caliburn.Light
{
    internal sealed class DelegateCommandImpl<TParameter> : IDelegateCommand
    {
        private readonly Action<TParameter> _execute;
        private readonly Func<TParameter, bool> _canExecute;
        private readonly string[] _propertyNames;
        private readonly IDisposable _propertyChangedRegistration;
        private readonly WeakEventSource _canExecuteChangedSource = new WeakEventSource();

        public DelegateCommandImpl(Action<TParameter> execute, Func<TParameter, bool> canExecute, INotifyPropertyChanged target, string[] propertyNames)
        {
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

        public void Execute(object parameter)
        {
            var value = CoerceParameter(parameter);
            _execute(value);
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null) return true;
            var value = CoerceParameter(parameter);
            return _canExecute(value);
        }

        private TParameter CoerceParameter(object parameter)
        {
            if (parameter == null)
                return default(TParameter);

            var specialValue = parameter as ISpecialValue;
            if (specialValue != null)
                parameter = specialValue.Resolve(new CoroutineExecutionContext());

            if (parameter is TParameter)
                return (TParameter) parameter;

            return (TParameter) ParameterBinder.CoerceValue(typeof (TParameter), parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { _canExecuteChangedSource.Add(value); }
            remove { _canExecuteChangedSource.Remove(value); }
        }

        public void RaiseCanExecuteChanged()
        {
            if (UIContext.CheckAccess())
                _canExecuteChangedSource.Raise(this, EventArgs.Empty);
            else
                UIContext.Run(() => _canExecuteChangedSource.Raise(this, EventArgs.Empty)).ObserveException();
        }
    }
}
