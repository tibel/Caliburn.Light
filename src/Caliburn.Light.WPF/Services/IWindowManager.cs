using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace Caliburn.Light.WPF;

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
    /// <param name="ownerViewModel">The owner view model.</param>
    /// <param name="context">The context.</param>
    Task ShowDialog(object viewModel, object ownerViewModel, string? context = null);

    /// <summary>
    /// Attempts to bring the window to the foreground and activates it.
    /// </summary>
    /// <param name="viewModel">The view model of the window.</param>
    /// <returns>true if the window was successfully activated; otherwise, false.</returns>
    bool Activate(object viewModel);

    /// <summary>
    /// Shows a message box.
    /// </summary>
    /// <param name="options">The message box options.</param>
    /// <param name="ownerViewModel">The owner view model.</param>
    /// <returns>The message box result.</returns>
    Task<MessageBoxResult> ShowMessageBoxDialog(MessageBoxDialogOptions options, object ownerViewModel);

    /// <summary>
    /// Shows a file open dialog.
    /// </summary>
    /// <param name="options">The dialog options.</param>
    /// <param name="ownerViewModel">The owner view model.</param>
    /// <returns>A list of selected files.</returns>
    Task<IReadOnlyList<string>> ShowOpenFileDialog(OpenFileDialogOptions options, object ownerViewModel);

    /// <summary>
    /// Shows a file save dialog.
    /// </summary>
    /// <param name="options">The dialog options.</param>
    /// <param name="ownerViewModel">The owner view model.</param>
    /// <returns>The selected file.</returns>
    Task<string> ShowSaveFileDialog(SaveFileDialogOptions options, object ownerViewModel);

    /// <summary>
    /// Shows an open folder dialog.
    /// </summary>
    /// <param name="options">The dialog options.</param>
    /// <param name="ownerViewModel">The owner view model.</param>
    /// <returns>A list of selected folders.</returns>
    Task<IReadOnlyList<string>> ShowOpenFolderDialog(OpenFolderDialogOptions options, object ownerViewModel);
}
