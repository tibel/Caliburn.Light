using Microsoft.Win32;

namespace Caliburn.Light.WPF;

/// <summary>
/// Options class for <see cref="IWindowManager.ShowSaveFileDialog"/> method.
/// </summary>
public class SaveFileDialogSettings
{
    /// <summary>
    /// Gets or sets the dialog title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the initial file name.
    /// </summary>
    public string InitialFileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file type filter.
    /// </summary>
    public string FileTypeFilter { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the initial directory.
    /// </summary>
    public string InitialDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default file extension.
    /// </summary>
    public string DefaultFileExtension { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the user is asked for permission if the file will be overridden.
    /// </summary>
    public bool PromptForOverwrite { get; set; }

    /// <summary>
    /// Gets or sets whether the user is asked for permission if a new file will be created.
    /// </summary>
    public bool PromptForCreate { get; set; }

    /// <summary>
    /// Applies the current configuration settings to the specified <see cref="SaveFileDialog"/> instance.
    /// </summary>
    /// <param name="dialog">The <see cref="SaveFileDialog"/> to which the settings will be applied.</param>
    public void ApplyTo(SaveFileDialog dialog)
    {
        dialog.RestoreDirectory = true;
        dialog.AddExtension = true;
        dialog.CheckPathExists = true;

        dialog.Filter = FileTypeFilter;
        dialog.DefaultExt = DefaultFileExtension;

        dialog.Title = Title;
        dialog.FileName = InitialFileName;
        dialog.CreatePrompt = PromptForCreate;
        dialog.OverwritePrompt = PromptForOverwrite;
        dialog.InitialDirectory = InitialDirectory;
    }
}
