using System;
using System.Threading;
using System.Threading.Tasks;

namespace Caliburn.Light
{
    /// <summary>
    /// The UI execution context.
    /// </summary>
    public interface IUIContext
    {
        /// <summary>
        /// Determines whether the calling thread is the thread associated with the UI context. 
        /// </summary>
        bool CheckAccess();

        /// <summary>
        /// Verifies that the calling thread has access to the UI context.
        /// </summary>
        void VerifyAccess();

        /// <summary>
        /// Queues the specified work to run on the UI thread.
        /// </summary>
        /// <param name="action">The work to execute asynchronously.</param>
        void BeginInvoke(Action action);

        /// <summary>
        /// Queues the specified work to run on the UI thread.
        /// </summary>
        /// <param name="callback">A <see cref="WaitCallback"/> that represents the method to be executed.</param>
        /// <param name="state">An object containing data to be used by the method.</param>
        void BeginInvoke(SendOrPostCallback callback, object state);

        /// <summary>
        /// Queues the specified work to run on the UI thread and returns a <see cref="Task"/> handle for that work.
        /// </summary>
        /// <param name="action">The work to execute asynchronously.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute on the UI thread.</returns>
        Task Run(Action action);

        /// <summary>
        /// Queues the specified work to run on the UI thread and returns a proxy for the <see cref="Task&lt;TResult&gt;"/> returned by function.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the proxy task.</typeparam>
        /// <param name="function">The work to execute asynchronously</param>
        /// <returns>A <see cref="Task&lt;TResult&gt;"/> that represents a proxy for the <see cref="Task&lt;TResult&gt;"/> returned by function.</returns>
        Task<TResult> Run<TResult>(Func<Task<TResult>> function);

        /// <summary>
        /// Queues the specified work to run on the UI thread and returns a proxy for the task returned by function.
        /// </summary>
        /// <param name="function">The work to execute asynchronously</param>
        /// <returns>A task that represents a proxy for the task returned by function.</returns>
        Task Run(Func<Task> function);

        /// <summary>
        /// Queues the specified work to run on the UI thread and returns a <see cref="Task&lt;TResult&gt;"/> handle for that work.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="function">The work to execute asynchronously</param>
        /// <returns>A <see cref="Task&lt;TResult&gt;"/> that represents the work queued to execute in the UI thread.</returns>
        Task<TResult> Run<TResult>(Func<TResult> function);

        /// <summary>
        /// The <see cref="System.Threading.Tasks.TaskScheduler"/> associated with the UI context.
        /// </summary>
        TaskScheduler TaskScheduler { get; }
    }
}
