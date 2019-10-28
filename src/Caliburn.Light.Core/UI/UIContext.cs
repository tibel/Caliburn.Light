using System;
using System.Threading;
using System.Threading.Tasks;

namespace Caliburn.Light
{
    /// <summary>
    /// The UI execution context.
    /// </summary>
    public static class UIContext
    {
        private static IUIContext _uiContext;

        /// <summary>
        /// Gets whether the <see cref="UIContext"/> is initialized.
        /// </summary>
        public static bool IsInitialized => _uiContext is object;

        /// <summary>
        /// Verifies that <see cref="UIContext"/> is initialized.
        /// </summary>
        public static void VerifyInitialized()
        {
            if (!IsInitialized)
                throw new InvalidOperationException(nameof(UIContext) + " is not initialized.");
        }

        /// <summary>
        /// Initializes the <see cref="UIContext"/>.
        /// </summary>
        public static void Initialize(IUIContext uiContext)
        {
            _uiContext = uiContext;
        }

        /// <summary>
        /// Determines whether the calling thread is the thread associated with the UI context. 
        /// </summary>
        /// <returns></returns>
        public static bool CheckAccess()
        {
            VerifyInitialized();
            return _uiContext.CheckAccess();
        }

        /// <summary>
        /// Verifies that the calling thread has access to the UI context.
        /// </summary>
        public static void VerifyAccess()
        {
            VerifyInitialized();
            _uiContext.VerifyAccess();
        }

        /// <summary>
        /// Queues the specified work to run on the UI thread.
        /// </summary>
        /// <param name="action">The work to execute asynchronously.</param>
        public static void BeginInvoke(Action action)
        {
            VerifyInitialized();
            _uiContext.BeginInvoke(action);
        }

        /// <summary>
        /// Queues the specified work to run on the UI thread.
        /// </summary>
        /// <param name="callback">A <see cref="WaitCallback"/> that represents the method to be executed.</param>
        /// <param name="state">An object containing data to be used by the method.</param>
        public static void BeginInvoke(SendOrPostCallback callback, object state)
        {
            VerifyInitialized();
            _uiContext.BeginInvoke(callback, state);
        }

        /// <summary>
        /// Queues the specified work to run on the UI thread and returns a <see cref="Task"/> handle for that work.
        /// </summary>
        /// <param name="action">The work to execute asynchronously.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute on the UI thread.</returns>
        public static Task Run(Action action)
        {
            VerifyInitialized();
            return _uiContext.Run(action);
        }

        /// <summary>
        /// Queues the specified work to run on the UI thread and returns a proxy for the <see cref="Task&lt;TResult&gt;"/> returned by function.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the proxy task.</typeparam>
        /// <param name="function">The work to execute asynchronously</param>
        /// <returns>A <see cref="Task&lt;TResult&gt;"/> that represents a proxy for the <see cref="Task&lt;TResult&gt;"/> returned by function.</returns>
        public static Task<TResult> Run<TResult>(Func<Task<TResult>> function)
        {
            VerifyInitialized();
            return _uiContext.Run(function);
        }

        /// <summary>
        /// Queues the specified work to run on the UI thread and returns a proxy for the task returned by function.
        /// </summary>
        /// <param name="function">The work to execute asynchronously</param>
        /// <returns>A task that represents a proxy for the task returned by function.</returns>
        public static Task Run(Func<Task> function)
        {
            VerifyInitialized();
            return _uiContext.Run(function);
        }

        /// <summary>
        /// Queues the specified work to run on the UI thread and returns a <see cref="Task&lt;TResult&gt;"/> handle for that work.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="function">The work to execute asynchronously</param>
        /// <returns>A <see cref="Task&lt;TResult&gt;"/> that represents the work queued to execute in the UI thread.</returns>
        public static Task<TResult> Run<TResult>(Func<TResult> function)
        {
            VerifyInitialized();
            return _uiContext.Run(function);
        }

        /// <summary>
        /// The <see cref="System.Threading.Tasks.TaskScheduler"/> associated with the UI context.
        /// </summary>
        public static TaskScheduler TaskScheduler
        {
            get
            {
                VerifyInitialized();
                return _uiContext.TaskScheduler;
            }
        }
    }
}
