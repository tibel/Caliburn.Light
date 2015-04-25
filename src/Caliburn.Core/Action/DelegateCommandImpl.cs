using System;
using System.ComponentModel;
using Weakly;

namespace Caliburn.Light
{
    internal sealed class DelegateCommandImpl<TTarget, TParameter> : IDelegateCommand
        where TTarget : class
    {
        private readonly TTarget _target;
        private readonly Action<TTarget, TParameter> _execute;
        private readonly Func<TTarget, TParameter, bool> _canExecute;
        private readonly string _propertyName;
        private readonly IDisposable _propertyChangedRegistration;
        private readonly WeakEventSource _canExecuteChangedSource = new WeakEventSource();

        public DelegateCommandImpl(TTarget target, Action<TTarget, TParameter> execute, Func<TTarget, TParameter, bool> canExecute, string propertyName)
        {
            _target = target;
            _execute = execute;
            _canExecute = canExecute;
            _propertyName = propertyName;

            var inpc = _target as INotifyPropertyChanged;
            if (inpc != null)
            {
                _propertyChangedRegistration = inpc.RegisterPropertyChangedWeak(this, (t, s, e) => t.OnPropertyChanged(e));
            }
        }

        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == _propertyName)
            {
                RaiseCanExecuteChanged();
            }
        }

        public void Execute(object parameter)
        {
            var value = CoerceParameter(parameter);
            _execute(_target, value);
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null) return true;
            var value = CoerceParameter(parameter);
            return _canExecute(_target, value);
        }

        private TParameter CoerceParameter(object parameter)
        {
            if (parameter == null)
                return default(TParameter);

            var context = parameter as CoroutineExecutionContext ?? new CoroutineExecutionContext();
            context.Target = _target;

            var specialValue = parameter as ISpecialValue;
            if (specialValue != null)
                parameter = specialValue.Resolve(context);

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
