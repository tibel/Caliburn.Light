using Microsoft.Win32;

namespace Caliburn.Light.WPF;

/// <summary>
/// Options class for <see cref="IWindowManager.ShowOpenFileDialog"/> method.
/// </summary>
public class OpenFileDialogOptions
{
    /// <summary>
    /// Gets or sets the dialog title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether it is allows to select multiple files.
    /// </summary>
    public bool Multiselect { get; set; }

    /// <summary>
    /// Gets or sets the file type filter.
    /// </summary>
    public string FileTypeFilter { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the initial directory.
    /// </summary>
    public string InitialDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Applies the current configuration settings to the specified <see cref="OpenFileDialog"/> instance.
    /// </summary>
    /// <param name="dialog">The <see cref="OpenFileDialog"/> to which the settings will be applied.</param>
    public virtual void ApplyTo(OpenFileDialog dialog)
    {
        dialog.RestoreDirectory = true;
        dialog.CheckFileExists = true;
        dialog.CheckPathExists = true;

        dialog.Multiselect = Multiselect;
        dialog.Filter = FileTypeFilter;
        dialog.Title = Title;
        dialog.InitialDirectory = InitialDirectory;
    }
}
