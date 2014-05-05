using System;
using System.Collections.Generic;

namespace Caliburn.Light
{
    /// <summary>
    /// The interface a view-model uses to interact with the view.
    /// </summary>
    public interface IUIView
    {
        /// <summary>
        /// Used to retrieve the root, non-framework-created view.
        /// </summary>
        /// <param name="view">The view to search.</param>
        /// <returns>The root element that was not created by the framework.</returns>
        object GetFirstNonGeneratedView(object view);

        /// <summary>
        /// Executes the handler the fist time the view is loaded.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="handler">The handler.</param>
        void ExecuteOnFirstLoad(object view, Action<object> handler);

        /// <summary>
        /// Executes the handler the next time the view's LayoutUpdated event fires.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="handler">The handler.</param>
        void ExecuteOnLayoutUpdated(object view, Action<object> handler);

        /// <summary>
        /// Tries to close the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model to close.</param>
        /// <param name="views">The associated views.</param>
        /// <param name="dialogResult">The dialog result.</param>
        /// <returns>An <see cref="Action"/> to close the view model.</returns>
        void TryClose(object viewModel, ICollection<object> views, bool? dialogResult);
    }
}
