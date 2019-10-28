using System;

namespace Caliburn.Light
{
    /// <summary>
    /// The helper a view-model uses to interact with the view.
    /// </summary>
    public static class ViewHelper
    {
        private static IViewAdapter _viewAdapter;

        /// <summary>
        /// Gets whether the <see cref="ViewHelper"/> is initialized.
        /// </summary>
        public static bool IsInitialized => _viewAdapter is object;

        /// <summary>
        /// Verifies that <see cref="ViewHelper"/> is initialized.
        /// </summary>
        public static void VerifyInitialized()
        {
            if (!IsInitialized)
                throw new InvalidOperationException(nameof(ViewHelper) + " is not initialized.");
        }

        /// <summary>
        /// Initializes the <see cref="ViewHelper"/>.
        /// </summary>
        /// <param name="viewAdapter">The adapter to interact with view elements.</param>
        public static void Initialize(IViewAdapter viewAdapter)
        {
            _viewAdapter = viewAdapter;
        }

        /// <summary>
        /// Indicates whether or not the framework is running in the context of a designer.
        /// </summary>
        public static bool IsInDesignTool
        {
            get { return !IsInitialized || _viewAdapter.IsInDesignTool; }
        }

        /// <summary>
        /// Used to retrieve the root, non-framework-created view.
        /// </summary>
        /// <param name="view">The view to search.</param>
        /// <returns>The root element that was not created by the framework.</returns>
        public static object GetFirstNonGeneratedView(object view)
        {
            VerifyInitialized();
            return _viewAdapter.GetFirstNonGeneratedView(view);
        }

        /// <summary>
        /// Executes the handler the fist time the view is loaded.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="handler">The handler.</param>
        public static void ExecuteOnFirstLoad(object view, Action<object> handler)
        {
            VerifyInitialized();
            _viewAdapter.ExecuteOnFirstLoad(view, handler);
        }

        /// <summary>
        /// Executes the handler the next time the view's LayoutUpdated event fires.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="handler">The handler.</param>
        public static void ExecuteOnLayoutUpdated(object view, Action<object> handler)
        {
            VerifyInitialized();
            _viewAdapter.ExecuteOnLayoutUpdated(view, handler);
        }

        /// <summary>
        /// Tries to close the specified view.
        /// </summary>
        /// <param name="view">The view to close.</param>
        /// <param name="dialogResult">The dialog result.</param>
        /// <returns>true, when close could be initiated; otherwise false.</returns>
        public static bool TryClose(object view, bool? dialogResult)
        {
            VerifyInitialized();
            return _viewAdapter.TryClose(view, dialogResult);
        }

        /// <summary>
        /// Gets the command parameter of the view.
        /// This can be <see cref="P:ICommandSource.CommandParameter"/> or 'cal:Bind.CommandParameter'.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <returns>The command parameter.</returns>
        public static object GetCommandParameter(object view)
        {
            VerifyInitialized();
            return _viewAdapter.GetCommandParameter(view);
        }
    }
}
