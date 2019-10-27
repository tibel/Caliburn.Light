﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Caliburn.Light
{
    /// <summary>
    /// The UI execution context.
    /// </summary>
    public static class UIContext
    {
        private static IUIContext _uiContext = NullUIContext.Instance;

        /// <summary>
        /// Gets whether the <see cref="UIContext"/> is initialized.
        /// </summary>
        public static bool IsInitialized => !ReferenceEquals(_uiContext, NullUIContext.Instance);

        /// <summary>
        /// Initializes the <see cref="UIContext"/>.
        /// </summary>
        public static void Initialize(IUIContext uiContext)
        {
            _uiContext = uiContext ?? NullUIContext.Instance;
        }

        /// <summary>
        /// Determines whether the calling thread is the thread associated with the UI context. 
        /// </summary>
        /// <returns></returns>
        public static bool CheckAccess()
        {
            return _uiContext.CheckAccess();
        }

        /// <summary>
        /// Verifies that the calling thread has access to the UI context.
        /// </summary>
        public static void VerifyAccess()
        {
            _uiContext.VerifyAccess();
        }

        /// <summary>
        /// Queues the specified work to run on the UI thread.
        /// </summary>
        /// <param name="action">The work to execute asynchronously.</param>
        public static void BeginInvoke(Action action)
        {
            _uiContext.BeginInvoke(action);
        }

        /// <summary>
        /// Queues the specified work to run on the UI thread.
        /// </summary>
        /// <param name="callback">A <see cref="WaitCallback"/> that represents the method to be executed.</param>
        /// <param name="state">An object containing data to be used by the method.</param>
        public static void BeginInvoke(SendOrPostCallback callback, object state)
        {
            _uiContext.BeginInvoke(callback, state);
        }

        /// <summary>
        /// Queues the specified work to run on the UI thread and returns a <see cref="Task"/> handle for that work.
        /// </summary>
        /// <param name="action">The work to execute asynchronously.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute on the UI thread.</returns>
        public static Task Run(Action action)
        {
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
            return _uiContext.Run(function);
        }

        /// <summary>
        /// Queues the specified work to run on the UI thread and returns a proxy for the task returned by function.
        /// </summary>
        /// <param name="function">The work to execute asynchronously</param>
        /// <returns>A task that represents a proxy for the task returned by function.</returns>
        public static Task Run(Func<Task> function)
        {
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
            return _uiContext.Run(function);
        }

        /// <summary>
        /// The <see cref="System.Threading.Tasks.TaskScheduler"/> associated with the UI context.
        /// </summary>
        public static TaskScheduler TaskScheduler => _uiContext.TaskScheduler;
    }
}
