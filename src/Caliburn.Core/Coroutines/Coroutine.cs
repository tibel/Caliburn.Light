using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Caliburn.Light
{
    /// <summary>
    /// Manages coroutine execution.
    /// </summary>
    public static class Coroutine
    {
        /// <summary>
        /// Executes an <see cref="ICoTask"/> asynchronous.
        /// </summary>
        /// <param name="coTask">The coroutine to execute.</param>
        /// <param name="context">The context to execute the coroutine within.</param>
        /// <returns>A task that represents the asynchronous coroutine.</returns>
        public static Task ExecuteAsync(this ICoTask coTask, CoroutineExecutionContext context = null)
        {
            return InternalExecuteAsync<object>(coTask, context);
        }

        /// <summary>
        /// Executes an <see cref="ICoTask&lt;TResult&gt;"/> asynchronous.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="coTask">The coroutine to execute.</param>
        /// <param name="context">The context to execute the coroutine within.</param>
        /// <returns>A task that represents the asynchronous coroutine.</returns>
        public static Task<TResult> ExecuteAsync<TResult>(this ICoTask<TResult> coTask,
            CoroutineExecutionContext context = null)
        {
            return InternalExecuteAsync<TResult>(coTask, context);
        }

        private static Task<TResult> InternalExecuteAsync<TResult>(ICoTask coTask, CoroutineExecutionContext context)
        {
            var taskSource = new TaskCompletionSource<TResult>();

            EventHandler<CoTaskCompletedEventArgs> completed = null;
            completed = (s, e) =>
            {
                ((ICoTask)s).Completed -= completed;

                if (e.Error != null)
                    taskSource.TrySetException(e.Error);
                else if (e.WasCancelled)
                    taskSource.TrySetCanceled();
                else
                {
                    var rr = s as ICoTask<TResult>;
                    taskSource.TrySetResult(rr != null ? rr.Result : default(TResult));
                }
            };

            try
            {
                IoC.InjectProperties(coTask);
                coTask.Completed += completed;
                coTask.BeginExecute(context ?? new CoroutineExecutionContext());
            }
            catch (Exception ex)
            {
                coTask.Completed -= completed;
                taskSource.TrySetException(ex);
            }

            return taskSource.Task;
        }

        #region AsCoTask

        /// <summary>
        /// Encapsulates a sequence of <see cref="ICoTask"/> inside a coroutine.
        /// </summary>
        /// <param name="enumerator">The enumerator.</param>
        /// <returns>The coroutine that encapsulates the sequence.</returns>
        public static ICoTask AsCoTask(this IEnumerator<ICoTask> enumerator)
        {
            return new SequentialCoTask(enumerator);
        }

        /// <summary>
        /// Encapsulates a <see cref="Action"/> inside a coroutine.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>The coroutine that encapsulates the action.</returns>
        public static ICoTask AsCoTask(this Action action)
        {
            return new DelegateCoTask(action);
        }

        /// <summary>
        /// Encapsulates a <see cref="Func&lt;TResult&gt;"/> inside a coroutine.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action.</param>
        /// <returns>The coroutine that encapusulates the action.</returns>
        public static ICoTask<TResult> AsCoTask<TResult>(this Func<TResult> action)
        {
            return new DelegateCoTask<TResult>(action);
        }

        /// <summary>
        /// Encapsulates a <see cref="Task"/> inside a couroutine.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>The coroutine that encapsulates the task.</returns>
        public static ICoTask AsCoTask(this Task task)
        {
            return new TaskDecoratorCoTask(task);
        }

        /// <summary>
        /// Encapsulates a <see cref="Task&lt;TResult&gt;"/> inside a couroutine.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="task">The task.</param>
        /// <returns>The coroutine that encapsulates the task.</returns>
        public static ICoTask<TResult> AsCoTask<TResult>(this Task<TResult> task)
        {
            return new TaskDecoratorCoTask<TResult>(task);
        }

        #endregion

        #region Decorators

        /// <summary>
        /// Adds behavior to the CoTask which is executed when the <paramref name ="coTask"/> was cancelled.
        /// </summary>
        /// <param name="coTask">The CoTask to decorate.</param>
        /// <param name="coroutine">The coroutine to execute when <paramref name="coTask"/> was canceled.</param>
        /// <returns></returns>
        public static ICoTask WhenCancelled(this ICoTask coTask, Func<ICoTask> coroutine)
        {
            return new ContinueCoTaskDecorator(coTask, coroutine);
        }

        /// <summary>
        /// Overrides <see cref="CoTaskCompletedEventArgs.WasCancelled"/> of the decorated <paramref name="coTask"/> instance.
        /// </summary>
        /// <param name="coTask">The CoTask to decorate.</param>
        /// <returns></returns>
        public static ICoTask OverrideCancel(this ICoTask coTask)
        {
            return new OverrideCancelCoTaskDecorator(coTask);
        }

        /// <summary>
        /// Overrides <see cref="CoTaskCompletedEventArgs.WasCancelled"/> of the decorated <paramref name="coTask"/> instance.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="coTask">The CoTask to decorate.</param>
        /// <param name="cancelResult">The canceled result value.</param>
        /// <returns></returns>
        public static ICoTask<TResult> OverrideCancel<TResult>(this ICoTask<TResult> coTask, TResult cancelResult = default(TResult))
        {
            return new OverrideCancelCoTaskDecorator<TResult>(coTask, cancelResult);
        }

        /// <summary>
        /// Rescues <typeparamref name="TException"/> from the decorated <paramref name="coTask"/> by executing a <paramref name="rescue"/> coroutine.
        /// </summary>
        /// <typeparam name = "TException">The type of the exception we want to perform the rescue on.</typeparam>
        /// <param name="coTask">The CoTask to decorate.</param>
        /// <param name="rescue">The rescue coroutine.</param>
        /// <param name="cancelCoTask">Set to true to cancel the CoTask after executing rescue.</param>
        /// <returns></returns>
        public static ICoTask Rescue<TException>(this ICoTask coTask, Func<TException, ICoTask> rescue,
            bool cancelCoTask = true)
            where TException : Exception
        {
            return new RescueCoTaskDecorator<TException>(coTask, rescue, cancelCoTask);
        }

        /// <summary>
        /// Rescues any exception from the decorated <paramref name="coTask"/> by executing a <paramref name="rescue"/> coroutine.
        /// </summary>
        /// <param name="coTask">The CoTask to decorate.</param>
        /// <param name="rescue">The rescue coroutine.</param>
        /// <param name="cancelCoTask">Set to true to cancel the CoTask after executing rescue.</param>
        /// <returns></returns>
        public static ICoTask Rescue(this ICoTask coTask, Func<Exception, ICoTask> rescue,
            bool cancelCoTask = true)
        {
            return Rescue<Exception>(coTask, rescue, cancelCoTask);
        }

        #endregion
    }
}
