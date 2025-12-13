using System.Windows;

namespace Caliburn.Light.WPF;

/// <summary>
/// Options class for <see cref="IWindowManager.ShowMessageBox"/> method.
/// </summary>
public sealed class MessageBoxSettings
{
    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the caption text.
    /// </summary>
    public string Caption { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the buttons.
    /// </summary>
    public MessageBoxButton Button { get; set; }

    /// <summary>
    /// Gets or sets the image.
    /// </summary>
    public MessageBoxImage Image { get; set; }
}
