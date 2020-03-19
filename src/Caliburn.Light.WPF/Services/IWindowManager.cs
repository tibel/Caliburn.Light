using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace Caliburn.Light.WPF
{
    /// <summary>
    /// A service that manages windows.
    /// </summary>
    public interface IWindowManager
    {
        /// <summary>
        /// Shows a non-modal window for the specified model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The context.</param>
        void ShowWindow(object viewModel, string context = null);

        /// <summary>
        /// Shows a modal window for the specified model.
        /// </summary>
        /// <param name="ownerViewModel">The owner view model.</param>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The context.</param>
        Task ShowDialog(object ownerViewModel, object viewModel, string context = null);

        /// <summary>
        /// Attempts to bring the window to the foreground and activates it.
        /// </summary>
        /// <param name="viewModel">The view model of the window.</param>
        /// <returns>true if the window was successfully activated; otherwise, false.</returns>
        bool Activate(object viewModel);

        /// <summary>
        /// Shows a popup at the current mouse position.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The view context.</param>
        void ShowPopup(object viewModel, string context = null);

        /// <summary>
        /// Shows a message box.
        /// </summary>
        /// <param name="ownerViewModel">The owner view model.</param>
        /// <param name="settings">The message box settings.</param>
        /// <returns>The message box result.</returns>
        Task<MessageBoxResult> ShowMessageBox(object ownerViewModel, MessageBoxSettings settings);

        /// <summary>
        /// Shows a file open dialog.
        /// </summary>
        /// <param name="ownerViewModel">The owner view model.</param>
        /// <param name="settings">The open file dialog settings.</param>
        /// <returns>A list of selected files.</returns>
        Task<IReadOnlyList<string>> ShowOpenFileDialog(object ownerViewModel, OpenFileDialogSettings settings);

        /// <summary>
        /// Shows a file save dialog.
        /// </summary>
        /// <param name="ownerViewModel">The owner view model.</param>
        /// <param name="settings">The save file dialog settings.</param>
        /// <returns>The selected file.</returns>
        Task<string> ShowSaveFileDialog(object ownerViewModel, SaveFileDialogSettings settings);
    }
}
