using Microsoft.Win32;

namespace Caliburn.Light.WPF;

/// <summary>
/// Options class for <see cref="IWindowManager.ShowOpenFolderDialog"/> method.
/// </summary>
public sealed class OpenFolderDialogOptions
{
    /// <summary>
    /// Gets or sets the dialog title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether it is allows to select multiple folders.
    /// </summary>
    public bool Multiselect { get; set; }

    /// <summary>
    /// Gets or sets the initial directory.
    /// </summary>
    public string InitialDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Applies the current configuration settings to the specified <see cref="OpenFolderDialog"/> instance.
    /// </summary>
    /// <param name="dialog">The <see cref="OpenFolderDialog"/> to which the settings will be applied.</param>
    public void ApplyTo(OpenFolderDialog dialog)
    {
        dialog.Title = Title;
        dialog.Multiselect = Multiselect;
        dialog.InitialDirectory = InitialDirectory;
    }
}
