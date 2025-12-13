using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Caliburn.Light.WinUI;

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
    /// Shows a <see cref="ContentControl"/> for the specified model.
    /// </summary>
    /// <param name="viewModel">The view model.</param>
    /// <param name="ownerViewModel">The owner view model.</param>
    /// <param name="context">The context.</param>
    /// <returns>The content dialog result.</returns>
    Task<ContentDialogResult> ShowContentDialog(object viewModel, object ownerViewModel, string? context = null);

    /// <summary>
    /// Displays an open file picker dialog.
    /// </summary>
    /// <param name="options">The dialog options.</param>
    /// <param name="ownerViewModel">The owner view model.</param>
    /// <returns>A list of selected files.</returns>
    Task<IReadOnlyList<PickFileResult>> ShowFileOpenPickerAsync(FileOpenPickerOptions options, object ownerViewModel);

    /// <summary>
    /// Displays a save file picker dialog.
    /// </summary>
    /// <param name="options">The dialog options.</param>
    /// <param name="ownerViewModel">The owner view model.</param>
    /// <returns>The selected file</returns>
    Task<PickFileResult?> ShowFileSavePickerAsync(FileSavePickerOptions options, object ownerViewModel);

    /// <summary>
    /// Displays an open folder picker dialog.
    /// </summary>
    /// <param name="options">The dialog options.</param>
    /// <param name="ownerViewModel">The owner view model.</param>
    /// <returns>The selected folder.</returns>
    Task<PickFolderResult?> ShowFolderPickerAsync(FolderPickerOptions options, object ownerViewModel);
}
