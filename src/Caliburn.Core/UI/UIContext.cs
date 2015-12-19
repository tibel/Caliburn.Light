using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Caliburn.Light
{
    /// <summary>
    /// Shared UI context
    /// </summary>
    public static class UIContext
    {
        private static int? _managedThreadId;
        private static TaskScheduler _taskScheduler;
        private static IViewAdapter _viewAdapter;

        /// <summary>
        /// Initializes the <see cref="UIContext"/>.
        /// </summary>
        /// <param name="viewAdapter">The adapter to interact with view elements.</param>
        public static void Initialize(IViewAdapter viewAdapter)
        {
            Initialize(viewAdapter, Environment.CurrentManagedThreadId, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Initializes the <see cref="UIContext"/>.
        /// </summary>
        /// <param name="viewAdapter">The adapter to interact with view elements. Can be null if not needed.</param>
        /// <param name="managedThreadId">The Id of the UI thread. Use null to allow any thread.</param>
        /// <param name="taskScheduler">The <see cref="TaskScheduler"/> associated with the UI context. Can be null if not needed.</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void Initialize(IViewAdapter viewAdapter, int? managedThreadId, TaskScheduler taskScheduler)
        {
            LogManager.GetLogger(typeof(UIContext)).Info("Initialize");
            _viewAdapter = viewAdapter;
            _managedThreadId = managedThreadId;
            _taskScheduler = taskScheduler;
        }

        #region Thread

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
            {
                throw new InvalidOperationException("The calling thread does not have access to the UI context.");
            }
        }

        /// <summary>
        /// The <see cref="TaskScheduler"/> associated with the UI context.
        /// </summary>
        public static TaskScheduler TaskScheduler
        {
            get { return _taskScheduler ?? TaskScheduler.Current; }
        }

        private const TaskCreationOptions CreationOptions = TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler;

        /// <summary>
        /// Queues the specified work to run on the UI thread and returns a <see cref="Task"/> handle for that work.
        /// </summary>
        /// <param name="action">The work to execute asynchronously.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute on the UI thread.</returns>
        public static Task Run(Action action)
        {
            return Task.Factory.StartNew(action, default(CancellationToken), CreationOptions, TaskScheduler);
        }

        /// <summary>
        /// Queues the specified work to run on the UI thread and returns a proxy for the <see cref="Task&lt;TResult&gt;"/> returned by function.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the proxy task.</typeparam>
        /// <param name="function">The work to execute asynchronously</param>
        /// <returns>A <see cref="Task&lt;TResult&gt;"/> that represents a proxy for the <see cref="Task&lt;TResult&gt;"/> returned by function.</returns>
        public static Task<TResult> Run<TResult>(Func<Task<TResult>> function)
        {
            return Task.Factory.StartNew(function, default(CancellationToken), CreationOptions, TaskScheduler).Unwrap();
        }

        /// <summary>
        /// Queues the specified work to run on the UI thread and returns a proxy for the task returned by function.
        /// </summary>
        /// <param name="function">The work to execute asynchronously</param>
        /// <returns>A task that represents a proxy for the task returned by function.</returns>
        public static Task Run(Func<Task> function)
        {
            return Task.Factory.StartNew(function, default(CancellationToken), CreationOptions, TaskScheduler).Unwrap();
        }

        /// <summary>
        /// Queues the specified work to run on the UI thread and returns a <see cref="Task&lt;TResult&gt;"/> handle for that work.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="function">The work to execute asynchronously</param>
        /// <returns>A <see cref="Task&lt;TResult&gt;"/> that represents the work queued to execute in the UI thread.</returns>
        public static Task<TResult> Run<TResult>(Func<TResult> function)
        {
            return Task.Factory.StartNew(function, default(CancellationToken), CreationOptions, TaskScheduler);
        }

        #endregion

        #region View

        private static IViewAdapter ViewAdapter
        {
            get { return _viewAdapter ?? NullViewAdapter.Instance; }
        }

        /// <summary>
        /// Indicates whether or not the framework is running in the context of a designer.
        /// </summary>
        public static bool IsInDesignTool
        {
            get { return ViewAdapter.IsInDesignTool; }
        }

        /// <summary>
        /// Used to retrieve the root, non-framework-created view.
        /// </summary>
        /// <param name="view">The view to search.</param>
        /// <returns>The root element that was not created by the framework.</returns>
        public static object GetFirstNonGeneratedView(object view)
        {
            return ViewAdapter.GetFirstNonGeneratedView(view);
        }

        /// <summary>
        /// Executes the handler the fist time the view is loaded.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="handler">The handler.</param>
        public static void ExecuteOnFirstLoad(object view, Action<object> handler)
        {
            ViewAdapter.ExecuteOnFirstLoad(view, handler);
        }

        /// <summary>
        /// Executes the handler the next time the view's LayoutUpdated event fires.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="handler">The handler.</param>
        public static void ExecuteOnLayoutUpdated(object view, Action<object> handler)
        {
            ViewAdapter.ExecuteOnLayoutUpdated(view, handler);
        }

        /// <summary>
        /// Tries to close the specified view.
        /// </summary>
        /// <param name="view">The view to close.</param>
        /// <param name="dialogResult">The dialog result.</param>
        /// <returns>true, when close could be initiated; otherwise false.</returns>
        public static bool TryClose(object view, bool? dialogResult)
        {
            return ViewAdapter.TryClose(view, dialogResult);
        }

        /// <summary>
        /// Gets the command parameter of the view.
        /// This can be <see cref="P:ICommandSource.CommandParameter"/> or 'cal:Bind.CommandParameter' (UAP only).
        /// </summary>
        /// <param name="view">The view.</param>
        /// <returns>The command parameter.</returns>
        public static object GetCommandParameter(object view)
        {
            return ViewAdapter.GetCommandParameter(view);
        }

        #endregion
    }
}
