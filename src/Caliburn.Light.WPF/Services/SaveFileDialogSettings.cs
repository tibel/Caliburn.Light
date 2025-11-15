namespace Caliburn.Light.WPF;

/// <summary>
/// Settings for a file save dialog.
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
}
