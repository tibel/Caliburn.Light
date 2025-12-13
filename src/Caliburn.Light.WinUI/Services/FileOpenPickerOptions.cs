using Microsoft.Windows.Storage.Pickers;
using System.Collections.Generic;

namespace Caliburn.Light.WinUI;

/// <summary>
/// Options class for <see cref="IWindowManager.ShowFileOpenPickerAsync"/> method.
/// </summary>
public class FileOpenPickerOptions
{
    /// <summary>
    /// Specifies the text displayed on commit button. If not specified, system default text is used.
    /// </summary>
    public string CommitButtonText { get; set; } = string.Empty;

    /// <summary>
    /// Specifies the file extensions filters. If not specified, the control defaults to all types (*.*).
    /// </summary>
    public IList<string>? FileTypeFilter { get; set; }

    /// <summary>
    /// Specifies the initial location of the file picker when it is opened.
    /// </summary>
    public PickerLocationId SuggestedStartLocation { get; set; } = PickerLocationId.Unspecified;

    /// <summary>
    /// The view mode of the file picker. This property is used to specify how items are displayed in the file picker.
    /// </summary>
    public PickerViewMode ViewMode { get; set; }

    /// <summary>
    /// Gets or sets an option indicating whether file open picker allows users to select multiple files.
    /// </summary>
    public bool AllowMultiple { get; set; }

    /// <summary>
    /// Applies the current configuration settings to the specified <see cref="FileOpenPicker"/> instance.
    /// </summary>
    /// <param name="picker">The <see cref="FileOpenPicker"/> to which the settings will be applied.</param>
    public virtual void ApplyTo(FileOpenPicker picker)
    {
        picker.CommitButtonText = CommitButtonText;

        if (FileTypeFilter is not null)
        {
            foreach (var filter in FileTypeFilter)
                picker.FileTypeFilter.Add(filter);
        }

        picker.SuggestedStartLocation = SuggestedStartLocation;
        picker.ViewMode = ViewMode;
    }
}
