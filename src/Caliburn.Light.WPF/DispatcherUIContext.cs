﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Caliburn.Light.WPF
{
    /// <summary>
    /// A UI execution context based on <see cref="Dispatcher"/>.
    /// </summary>
    public sealed class DispatcherUIContext : IUIContext
    {
        private readonly Dispatcher _dispatcher;
        private TaskScheduler _taskScheduler;

        /// <summary>
        /// Initializes the <see cref="DispatcherUIContext"/>.
        /// </summary>
        public DispatcherUIContext(Dispatcher dispatcher)
        {
            if (dispatcher is null)
                throw new ArgumentNullException(nameof(dispatcher));

            _dispatcher = dispatcher;
        }

        /// <summary>
        /// Determines whether the calling thread is the thread associated with the UI context. 
        /// </summary>
        public bool CheckAccess()
        {
            return _dispatcher.CheckAccess();
        }

        /// <summary>
        /// Verifies that the calling thread has access to the UI context.
        /// </summary>
        public void VerifyAccess()
        {
            _dispatcher.VerifyAccess();
        }

        /// <summary>
        /// Queues the specified work to run on the UI thread.
        /// </summary>
        /// <param name="action">The work to execute asynchronously.</param>
        public void BeginInvoke(Action action)
        {
            _dispatcher.BeginInvoke(action);
        }

        /// <summary>
        /// Queues the specified work to run on the UI thread.
        /// </summary>
        /// <param name="callback">A <see cref="WaitCallback"/> that represents the method to be executed.</param>
        /// <param name="state">An object containing data to be used by the method.</param>
        public void BeginInvoke(SendOrPostCallback callback, object state)
        {
            _dispatcher.BeginInvoke(callback, state);
        }

        /// <summary>
        /// Queues the specified work to run on the UI thread and returns a <see cref="Task"/> handle for that work.
        /// </summary>
        /// <param name="action">The work to execute asynchronously.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute on the UI thread.</returns>
        public Task Run(Action action)
        {
            return _dispatcher.InvokeAsync(action).Task;
        }

        /// <summary>
        /// Queues the specified work to run on the UI thread and returns a proxy for the <see cref="Task&lt;TResult&gt;"/> returned by function.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the proxy task.</typeparam>
        /// <param name="function">The work to execute asynchronously</param>
        /// <returns>A <see cref="Task&lt;TResult&gt;"/> that represents a proxy for the <see cref="Task&lt;TResult&gt;"/> returned by function.</returns>
        public Task<TResult> Run<TResult>(Func<Task<TResult>> function)
        {
            return RunCore(function).Unwrap();
        }

        /// <summary>
        /// Queues the specified work to run on the UI thread and returns a proxy for the task returned by function.
        /// </summary>
        /// <param name="function">The work to execute asynchronously</param>
        /// <returns>A task that represents a proxy for the task returned by function.</returns>
        public Task Run(Func<Task> function)
        {
            return RunCore(function).Unwrap();
        }

        /// <summary>
        /// Queues the specified work to run on the UI thread and returns a <see cref="Task&lt;TResult&gt;"/> handle for that work.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="function">The work to execute asynchronously</param>
        /// <returns>A <see cref="Task&lt;TResult&gt;"/> that represents the work queued to execute in the UI thread.</returns>
        public Task<TResult> Run<TResult>(Func<TResult> function)
        {
            return RunCore(function);
        }

        private Task<TResult> RunCore<TResult>(Func<TResult> function)
        {
            return _dispatcher.InvokeAsync(function).Task;
        }

        /// <summary>
        /// The <see cref="System.Threading.Tasks.TaskScheduler"/> associated with the UI context.
        /// </summary>
        public TaskScheduler TaskScheduler => _taskScheduler ?? (_taskScheduler = new DispatcherTaskScheduler(_dispatcher));
    }
}
