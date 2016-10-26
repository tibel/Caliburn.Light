using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Caliburn.Light
{
    /// <summary>
    /// Helper to create completed, canceled and faulted tasks.
    /// </summary>
    public static class TaskHelper
    {
        #region Common Tasks

        private readonly static Task CompletedTask = Task.FromResult<object>(null);

        /// <summary>
        /// Gets an already completed task.
        /// </summary>
        /// <returns>The completed task.</returns>
        public static Task Completed()
        {
            return CompletedTask;
        }

        private static class CanceledTask<TResult>
        {
            public static readonly Task<TResult> Task = CreateCanceled();

            private static Task<TResult> CreateCanceled()
            {
                var tcs = new TaskCompletionSource<TResult>();
                tcs.TrySetCanceled();
                return tcs.Task;
            }
        }

        /// <summary>
        /// Gets an already canceled task.
        /// </summary>
        /// <returns>The canceled task.</returns>
        public static Task Canceled()
        {
            return CanceledTask<object>.Task;
        }

        /// <summary>
        /// Gets an already canceled task.
        /// </summary>
        /// <returns>The canceled task.</returns>
        public static Task<TResult> Canceled<TResult>()
        {
            return CanceledTask<TResult>.Task;
        }

        /// <summary>
        /// Creates a task that is fauled with the specified exception.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the task.</typeparam>
        /// <param name="ex">The exception.</param>
        /// <returns>The faulted task.</returns>
        public static Task<TResult> Faulted<TResult>(Exception ex)
        {
            var tcs = new TaskCompletionSource<TResult>();
            tcs.TrySetException(ex);
            return tcs.Task;
        }

        /// <summary>
        /// Creates a task that is fauled with the specified exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>The faulted task.</returns>
        public static Task Faulted(Exception ex)
        {
            return Faulted<object>(ex);
        }

        #endregion

        #region Exception handling

        /// <summary>
        /// Suppresses default exception handling of a Task that would otherwise reraise the exception on the finalizer thread.
        /// </summary>
        /// <param name="task">The Task to be monitored.</param>
        /// <returns>The original Task.</returns>
        public static Task IgnoreExceptions(this Task task)
        {
            // ReSharper disable once UnusedVariable
            task.ContinueWith(t => { var ignored = t.Exception; },
                default(CancellationToken),
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default);
            return task;
        }

        /// <summary>
        /// Suppresses default exception handling of a Task that would otherwise reraise the exception on the finalizer thread.
        /// </summary>
        /// <param name="task">The Task to be monitored.</param>
        /// <returns>The original Task.</returns>
        public static Task<T> IgnoreExceptions<T>(this Task<T> task)
        {
            return (Task<T>)((Task)task).IgnoreExceptions();
        }

        /// <summary>
        /// Fails immediately when an exception is encountered.
        /// </summary>
        /// <param name="task">The Task to be monitored.</param>
        /// <returns>The original Task.</returns>
        public static Task FailFastOnException(this Task task)
        {
            task.ContinueWith(t => Environment.FailFast("A task faulted.", t.Exception),
                default(CancellationToken),
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default);
            return task;
        }

        /// <summary
        /// >Fails immediately when an exception is encountered.
        /// </summary>
        /// <param name="task">The Task to be monitored.</param>
        /// <returns>The original Task.</returns>
        public static Task<T> FailFastOnException<T>(this Task<T> task)
        {
            return (Task<T>)((Task)task).FailFastOnException();
        }

        /// <summary>
        /// Occurs when a faulted <see cref="Task"/> is observed.
        /// </summary>
        public static event EventHandler<TaskEventArgs> TaskFaulted;

        private static void OnTaskFaulted(Task task)
        {
            TaskFaulted?.Invoke(null, new TaskEventArgs(task));
        }

        /// <summary>
        /// Triggers the <see cref="TaskFaulted"/> event immediately when an exception is encountered.
        /// </summary>
        /// <param name="task">The Task to be monitored.</param>
        /// <returns>The original Task.</returns>
        public static Task ObserveException(this Task task)
        {
            task.ContinueWith(OnTaskFaulted,
                default(CancellationToken),
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default);
            return task;
        }

        /// <summary>
        /// Triggers the <see cref="TaskFaulted"/> event immediately when an exception is encountered.
        /// </summary>
        /// <param name="task">The Task to be monitored.</param>
        /// <returns>The original Task.</returns>
        public static Task<T> ObserveException<T>(this Task<T> task)
        {
            return (Task<T>)((Task)task).ObserveException();
        }

        /// <summary>
        /// Propagates any exceptions that occurred on the specified task.
        /// </summary>
        /// <param name="task">The Task whose exceptions are to be propagated.</param>
        public static void PropagateExceptions(this Task task)
        {
            if (!task.IsCompleted)
                throw new InvalidOperationException("The task has not completed.");

            if (task.IsFaulted)
                ExceptionDispatchInfo.Capture(task.Exception.InnerException).Throw();
        }

        /// <summary>
        /// Propagates any exceptions that occurred on the specified task to the specified synchronization context.
        /// </summary>
        /// <param name="task">The Task whose exceptions are to be propagated.</param>
        /// <param name="synchronizationContext">The SynchronizationContext that should get the exception.</param>
        public static void PropagateExceptionsTo(this Task task, SynchronizationContext synchronizationContext)
        {
            if (!task.IsCompleted)
                throw new InvalidOperationException("The task has not completed.");

            if (task.IsFaulted)
            {
                var exceptionInfo = ExceptionDispatchInfo.Capture(task.Exception.InnerException);
                synchronizationContext.Post(state => ((ExceptionDispatchInfo)state).Throw(), exceptionInfo);
            }
        }

        #endregion

        /// <summary>
        /// Occurs when a <see cref="Task"/> is watched.
        /// </summary>
        public static event EventHandler<TaskEventArgs> TaskWatched;

        /// <summary>
        /// Triggers the <see cref="TaskWatched"/> event for the supplied <paramref name="task"/>.
        /// </summary>
        /// <param name="task">The task to watch.</param>
        public static void Watch(this Task task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            TaskWatched?.Invoke(null, new TaskEventArgs(task));
        }

        /// <summary>
        /// Waits for the task to complete execution, returning the task's final status.
        /// </summary>
        /// <param name="task">The task for which to wait.</param>
        /// <returns>The completion status of the task.</returns>
        /// <remarks>Unlike Wait, this method will not throw an exception if the task ends in the Faulted or Canceled state.</remarks>
        public static TaskStatus WaitForCompletionStatus(this Task task)
        {
            ((IAsyncResult)task).AsyncWaitHandle.WaitOne();
            return task.Status;
        }

        /// <summary>
        /// Don't await the supplied task. Use this inside async methods to avoid compiler warnings.
        /// </summary>
        /// <param name="task">The Task to be monitored.</param>
        public static void DontAwait(this Task task) { }
    }
}
