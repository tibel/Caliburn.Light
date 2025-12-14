using Microsoft.Windows.Storage.Pickers;
using System.Collections.Generic;

namespace Caliburn.Light.WinUI;

/// <summary>
/// Options class for <see cref="IWindowManager.ShowFileSavePickerAsync"/> method.
/// </summary>
public class FileSavePickerOptions
{
    /// <summary>
    /// Specifies the text displayed on commit button. If not specified, the system default is used.
    /// </summary>
    public string CommitButtonText { get; set; } = string.Empty;

    /// <summary>
    /// Specifies the default file extension, which will be appended after the default file name. If it's not specified, nothing will be appended after the default file name.
    /// </summary>
    public string DefaultFileExtension { get; set; } = string.Empty;

    /// <summary>
    /// The categorized extensions types. If not specified, allow All Files *.* is used.
    /// </summary>
    public IDictionary<string, IList<string>>? FileTypeChoices { get; set; }

    /// <summary>
    /// Specifies the default file name to display when the picker is opened. If not specified, the system default value is used.
    /// </summary>
    public string SuggestedFileName { get; set; } = string.Empty;

    /// <summary>
    /// The folder that is suggested for the user to save the file.
    /// </summary>
    public string SuggestedFolder { get; set; } = string.Empty;

    /// <summary>
    /// Specifies the initial location of the file picker when it is opened.
    /// </summary>
    public PickerLocationId SuggestedStartLocation { get; set; } = PickerLocationId.Unspecified;

    /// <summary>
    /// Applies the current configuration settings to the specified <see cref="FileSavePicker"/> instance.
    /// </summary>
    /// <param name="picker">The <see cref="FileSavePicker"/> to which the settings will be applied.</param>
    public virtual void ApplyTo(FileSavePicker picker)
    {
        picker.CommitButtonText = CommitButtonText;
        picker.DefaultFileExtension = DefaultFileExtension;

        if (FileTypeChoices is not null)
        {
            foreach (var choice in FileTypeChoices)
                picker.FileTypeChoices.Add(choice);
        }

        picker.SuggestedFileName = SuggestedFileName;
        picker.SuggestedFolder = SuggestedFolder;
        picker.SuggestedStartLocation = SuggestedStartLocation;
    }
}
