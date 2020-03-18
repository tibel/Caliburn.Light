using System.Collections.Generic;

namespace Caliburn.Light.WPF
{
    /// <summary>
    /// A service that manages windows.
    /// </summary>
    public interface IWindowManager
    {
        /// <summary>
        /// Shows a modal dialog for the specified model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The context.</param>
        /// <param name="settings">The optional dialog settings.</param>
        /// <returns>The dialog result.</returns>
        bool? ShowDialog(object viewModel, string context = null, IDictionary<string, object> settings = null);

        /// <summary>
        /// Shows a non-modal window for the specified model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The context.</param>
        /// <param name="settings">The optional window settings.</param>
        void ShowWindow(object viewModel, string context = null, IDictionary<string, object> settings = null);

        /// <summary>
        /// Shows a popup at the current mouse position.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The view context.</param>
        /// <param name="settings">The optional popup settings.</param>
        void ShowPopup(object viewModel, string context = null, IDictionary<string, object> settings = null);
    }
}
