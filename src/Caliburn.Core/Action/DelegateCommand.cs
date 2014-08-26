using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Weakly;

namespace Caliburn.Light
{
    /// <summary>
    /// Wraps a ViewModel method (with guard) in an <see cref="ICommand"/>.
    /// </summary>
    /// <remarks>When using a property as guard then <see cref="CanExecuteChanged"/> is raised automatically.</remarks>
    public sealed class DelegateCommand : ICommand
    {
        private readonly object _target;
        private readonly MethodInfo _method;
        private readonly MethodInfo _guard;
        private readonly string _guardName;
        private readonly IDisposable _propertyChangedRegistration;
        private readonly WeakEventSource _canExecuteChangedSource = new WeakEventSource();

        /// <summary>
        /// Creates a new <see cref="DelegateCommand"/> from the specified <paramref name="action"/>.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>The new <see cref="DelegateCommand"/>.</returns>
        public static DelegateCommand FromAction(Action action)
        {
            return CreateInternal(action);
        }

        /// <summary>
        /// Creates a new <see cref="DelegateCommand"/> from the specified <paramref name="action"/>.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="action">The action.</param>
        /// <returns>The new <see cref="DelegateCommand"/>.</returns>
        public static DelegateCommand FromAction<T>(Action<T> action)
        {
            return CreateInternal(action);
        }

        /// <summary>
        /// Creates a new <see cref="DelegateCommand"/> from the specified <paramref name="function"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="function">The function.</param>
        /// <returns>The new <see cref="DelegateCommand"/>.</returns>
        public static DelegateCommand FromFunc<TResult>(Func<TResult> function)
        {
            return CreateInternal(function);
        }

        /// <summary>
        /// Creates a new <see cref="DelegateCommand"/> from the specified <paramref name="function"/>.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="function">The function.</param>
        /// <returns>The new <see cref="DelegateCommand"/>.</returns>
        public static DelegateCommand FromFunc<T, TResult>(Func<T, TResult> function)
        {
            return CreateInternal(function);
        }

        private static DelegateCommand CreateInternal(Delegate action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            if (action.Target == null)
                throw new ArgumentException("Method cannot be static.", "action");

            return new DelegateCommand(action.Target, action.GetMethodInfo());
        }

        private DelegateCommand(object target, MethodInfo method)
        {
            _target = target;
            _method = method;

            _guardName = "Can" + _method.Name;

            _guard = ParameterBinder.FindGuardMethod(_target, _method);
            if (_guard != null) return;

            var inpc = _target as INotifyPropertyChanged;
            if (inpc == null) return;

            var property = _target.GetType().GetRuntimeProperty(_guardName);
            if (property == null) return;

            _guard = property.GetMethod;
            _propertyChangedRegistration = WeakEventHandler.Register<PropertyChangedEventArgs>(inpc, "PropertyChanged", OnPropertyChanged);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == _guardName)
            {
                RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object parameter)
        {
            var function = DynamicDelegate.From(_method);

            var context = parameter as CoroutineExecutionContext ?? new CoroutineExecutionContext();
            context.Target = _target;

            var finalValues = ParameterBinder.DetermineParameters(context, new[] { parameter }, _method.GetParameters());
            var returnValue = function(_target, finalValues);
            if (returnValue == null) return;

            var enumerable = returnValue as IEnumerable<ICoTask>;
            if (enumerable != null)
                returnValue = enumerable.GetEnumerator();

            var enumerator = returnValue as IEnumerator<ICoTask>;
            if (enumerator != null)
                returnValue = enumerator.AsCoTask();

            var coTask = returnValue as ICoTask;
            if (coTask != null)
                returnValue = coTask.OverrideCancel().ExecuteAsync(context);

            var task = returnValue as Task;
            if (task != null)
                PropagateExceptions(task);
        }

        private static async void PropagateExceptions(Task task)
        {
            await task.ConfigureAwait(false);
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            if (_guard == null) return true;

            var function = DynamicDelegate.From(_guard);

            var context = parameter as CoroutineExecutionContext ?? new CoroutineExecutionContext();
            context.Target = _target;

            var finalValues = ParameterBinder.DetermineParameters(context, new[] {parameter}, _guard.GetParameters());
            return (bool) function(_target, finalValues);
        }

        /// <summary>
        /// Occurs when changes occur that affect whether the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { _canExecuteChangedSource.Add(value); }
            remove { _canExecuteChangedSource.Remove(value); }
        }

        /// <summary>
        /// Raises <see cref="CanExecuteChanged"/> on the UI thread so every command invoker can requery to check if the command can execute.
        /// <remarks>Note that this will trigger the execution of <see cref="CanExecute"/> once for each invoker.</remarks>
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            if (UIContext.CheckAccess())
                _canExecuteChangedSource.Raise(this, EventArgs.Empty);
            else
                PropagateExceptions(UIContext.Run(() => _canExecuteChangedSource.Raise(this, EventArgs.Empty)));
        }
    }
}
