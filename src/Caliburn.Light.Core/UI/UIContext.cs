using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Caliburn.Light
{
    /// <summary>
    /// The UI execution context.
    /// </summary>
    public static class UIContext
    {
        private static readonly SynchronizationContext _nullSynchronizationContext = new SynchronizationContext();
        private static readonly SendOrPostCallback _sendOrPostCallback = new SendOrPostCallback(OnSendOrPost);

        private static int? _managedThreadId;
        private static SynchronizationContext _synchronizationContext = _nullSynchronizationContext;
        private static TaskScheduler _taskScheduler = TaskScheduler.Default;

        /// <summary>
        /// Gets whether the <see cref="UIContext"/> is initialized.
        /// </summary>
        public static bool IsInitialized => !ReferenceEquals(_synchronizationContext, _nullSynchronizationContext);

        /// <summary>
        /// Initializes the <see cref="UIContext"/>.
        /// </summary>
        public static void Initialize()
        {
            Initialize(Environment.CurrentManagedThreadId, SynchronizationContext.Current, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Initializes the <see cref="UIContext"/>.
        /// </summary>
        /// <param name="managedThreadId">The Id of the UI thread. Use null to allow any thread.</param>
        /// <param name="synchronizationContext">The <see cref="SynchronizationContext"/> associated with the UI context.</param>
        /// <param name="taskScheduler">The <see cref="System.Threading.Tasks.TaskScheduler"/> associated with the UI context.</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void Initialize(int? managedThreadId, SynchronizationContext synchronizationContext, TaskScheduler taskScheduler)
        {
            _managedThreadId = managedThreadId;
            _synchronizationContext = synchronizationContext ?? _nullSynchronizationContext;
            _taskScheduler = taskScheduler ?? TaskScheduler.Default;
        }

        /// <summary>
        /// Determines whether the calling thread is the thread associated with the UI context. 
        /// </summary>
        /// <returns></returns>
        public static bool CheckAccess()
        {
            return !_managedThreadId.HasValue || _managedThreadId == Environment.CurrentManagedThreadId;
        }

        /// <summary>
        /// Verifies that the calling thread has access to the UI context.
        /// </summary>
        public static void VerifyAccess()
        {
            if (!CheckAccess())
                throw new InvalidOperationException("The calling thread does not have access to the UI context.");
        }

        /// <summary>
        /// Queues the specified work to run on the UI thread.
        /// </summary>
        /// <param name="action">The work to execute asynchronously.</param>
        public static void BeginInvoke(Action action)
        {
            _synchronizationContext.Post(_sendOrPostCallback, action);
        }

        /// <summary>
        /// Queues the specified work to run on the UI thread.
        /// </summary>
        /// <param name="callback">A <see cref="WaitCallback"/> that represents the method to be executed.</param>
        /// <param name="state">An object containing data to be used by the method.</param>
        public static void BeginInvoke(SendOrPostCallback callback, object state)
        {
            _synchronizationContext.Post(callback, state);
        }

        private static void OnSendOrPost(object obj)
        {
            var action = (Action)obj;
            action();
        }

        private const TaskCreationOptions CreationOptions = TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler;

        /// <summary>
        /// Queues the specified work to run on the UI thread and returns a <see cref="Task"/> handle for that work.
        /// </summary>
        /// <param name="action">The work to execute asynchronously.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute on the UI thread.</returns>
        public static Task Run(Action action)
        {
            return Task.Factory.StartNew(action, default, CreationOptions, _taskScheduler);
        }

        /// <summary>
        /// Queues the specified work to run on the UI thread and returns a proxy for the <see cref="Task&lt;TResult&gt;"/> returned by function.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the proxy task.</typeparam>
        /// <param name="function">The work to execute asynchronously</param>
        /// <returns>A <see cref="Task&lt;TResult&gt;"/> that represents a proxy for the <see cref="Task&lt;TResult&gt;"/> returned by function.</returns>
        public static Task<TResult> Run<TResult>(Func<Task<TResult>> function)
        {
            return Task.Factory.StartNew(function, default, CreationOptions, _taskScheduler).Unwrap();
        }

        /// <summary>
        /// Queues the specified work to run on the UI thread and returns a proxy for the task returned by function.
        /// </summary>
        /// <param name="function">The work to execute asynchronously</param>
        /// <returns>A task that represents a proxy for the task returned by function.</returns>
        public static Task Run(Func<Task> function)
        {
            return Task.Factory.StartNew(function, default, CreationOptions, _taskScheduler).Unwrap();
        }

        /// <summary>
        /// Queues the specified work to run on the UI thread and returns a <see cref="Task&lt;TResult&gt;"/> handle for that work.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="function">The work to execute asynchronously</param>
        /// <returns>A <see cref="Task&lt;TResult&gt;"/> that represents the work queued to execute in the UI thread.</returns>
        public static Task<TResult> Run<TResult>(Func<TResult> function)
        {
            return Task.Factory.StartNew(function, default, CreationOptions, _taskScheduler);
        }

        /// <summary>
        /// The <see cref="System.Threading.Tasks.TaskScheduler"/> associated with the UI context.
        /// </summary>
        public static TaskScheduler TaskScheduler
        {
            get { return _taskScheduler; }
        }
    }
}
