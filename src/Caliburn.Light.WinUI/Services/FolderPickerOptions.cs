using Microsoft.Windows.Storage.Pickers;

namespace Caliburn.Light.WinUI;

/// <summary>
/// Options class for <see cref="IWindowManager.ShowFolderPickerAsync"/> method.
/// </summary>
public class FolderPickerOptions
{
    /// <summary>
    /// Specifies the text displayed on the commit button. If not specified, the system default text is used.
    /// </summary>
    public string CommitButtonText { get; set; } = string.Empty;

    /// <summary>
    /// Specifies the initial location of the folder picker when it is opened.
    /// </summary>
    public PickerLocationId SuggestedStartLocation { get; set; } = PickerLocationId.Unspecified;

    /// <summary>
    /// The view mode of the file picker. This property is used to specify how items are displayed in the folder picker.
    /// </summary>
    public PickerViewMode ViewMode { get; set; }

    /// <summary>
    /// Applies the current configuration settings to the specified <see cref="FolderPicker"/> instance.
    /// </summary>
    /// <param name="picker">The <see cref="FolderPicker"/> to which the settings will be applied.</param>
    public virtual void ApplyTo(FolderPicker picker)
    {
        picker.CommitButtonText = CommitButtonText;
        picker.SuggestedStartLocation = SuggestedStartLocation;
        picker.ViewMode = ViewMode;
    }
}
