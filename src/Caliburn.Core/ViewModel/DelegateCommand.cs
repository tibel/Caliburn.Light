using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Weakly;

namespace Caliburn.Light
{
    /// <summary>
    /// Wraps a ViewModel method (with guard) in an <see cref="ICommand"/>.
    /// </summary>
    public sealed class DelegateCommand : ICommand
    {
        private readonly WeakReference _targetReference;
        private readonly MethodInfo _method;
        private readonly WeakFunc<bool> _canExecute;
        private readonly WeakEventSource _canExecuteChangedSource = new WeakEventSource();
        private readonly string _guardName;

        /// <summary>
        /// Creates a new <see cref="DelegateCommand"/> from the specified <paramref name="action"/>.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>The new <see cref="DelegateCommand"/>.</returns>
        public static DelegateCommand Create(Action action)
        {
            return CreateInternal(action);
        }

        /// <summary>
        /// Creates a new <see cref="DelegateCommand"/> from the specified <paramref name="action"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action.</param>
        /// <returns>The new <see cref="DelegateCommand"/>.</returns>
        public static DelegateCommand Create<TResult>(Func<TResult> action)
        {
            return CreateInternal(action);
        }

        private static DelegateCommand CreateInternal(Delegate action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            if (action.Target == null)
                throw new ArgumentException("Method cannot be static.", "action");
            if (action.GetMethodInfo().IsClosure())
                throw new ArgumentException("A closure cannot be used.", "action");

            return new DelegateCommand(action.Target, action.GetMethodInfo());
        }

        private DelegateCommand(object target, MethodInfo method)
        {
            _targetReference = new WeakReference(target);
            _method = method;

            _guardName = "Can" + _method.Name;
            var property = target.GetType().GetRuntimeProperty(_guardName);
            if (property == null) return;

            var inpc = target as INotifyPropertyChanged;
            if (inpc == null) return;

            _canExecute = new WeakFunc<bool>(inpc, property.GetMethod);
            WeakEventHandler.Register<PropertyChangedEventArgs>(inpc, "PropertyChanged", OnPropertyChanged);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == _guardName)
            {
                if (UIContext.CheckAccess())
                    _canExecuteChangedSource.Raise(this, EventArgs.Empty);
                else
                    Task.Factory.StartNew(() => _canExecuteChangedSource.Raise(this, EventArgs.Empty),
                        CancellationToken.None, TaskCreationOptions.None, UIContext.TaskScheduler);
            }
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object parameter)
        {
            var target = _targetReference.Target;
            if (target == null) return;

            var execute = DynamicDelegate.From(_method);
            var returnValue = execute(target, new object[0]);
            if (returnValue == null) return;

            var enumerable = returnValue as IEnumerable<ICoTask>;
            if (enumerable != null)
            {
                returnValue = enumerable.GetEnumerator();
            }

            var enumerator = returnValue as IEnumerator<ICoTask>;
            if (enumerator != null)
            {
                returnValue = enumerator.AsCoTask();
            }

            var coTask = returnValue as ICoTask;
            if (coTask != null)
            {
                var context = parameter as CoroutineExecutionContext ?? new CoroutineExecutionContext();
                context.Target = target;
                coTask.ExecuteAsync(context);
            }
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute.Invoke();
        }

        /// <summary>
        /// Occurs when changes occur that affect whether the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { _canExecuteChangedSource.Add(value); }
            remove { _canExecuteChangedSource.Remove(value); }
        }
    }
}
