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
        void ShowWindow(object viewModel, string? context = null);

        /// <summary>
        /// Shows a modal window for the specified model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The context.</param>
        /// <param name="ownerViewModel">The owner view model.</param>
        Task ShowDialog(object viewModel, string context, object ownerViewModel);

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
        void ShowPopup(object viewModel, string? context = null);

        /// <summary>
        /// Shows a message box.
        /// </summary>
        /// <param name="settings">The message box settings.</param>
        /// <param name="ownerViewModel">The owner view model.</param>
        /// <returns>The message box result.</returns>
        Task<MessageBoxResult> ShowMessageBox(MessageBoxSettings settings, object ownerViewModel);

        /// <summary>
        /// Shows a file open dialog.
        /// </summary>
        /// <param name="settings">The open file dialog settings.</param>
        /// <param name="ownerViewModel">The owner view model.</param>
        /// <returns>A list of selected files.</returns>
        Task<IReadOnlyList<string>> ShowOpenFileDialog(OpenFileDialogSettings settings, object ownerViewModel);

        /// <summary>
        /// Shows a file save dialog.
        /// </summary>
        /// <param name="settings">The save file dialog settings.</param>
        /// <param name="ownerViewModel">The owner view model.</param>
        /// <returns>The selected file.</returns>
        Task<string> ShowSaveFileDialog(SaveFileDialogSettings settings, object ownerViewModel);
    }
}
