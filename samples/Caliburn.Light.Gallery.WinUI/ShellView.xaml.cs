using Caliburn.Light.WinUI;
using Microsoft.UI.Xaml.Controls;

namespace Caliburn.Light.Gallery.WinUI;

/// <summary>
/// The main shell window
/// </summary>
public sealed partial class ShellView : UserControl
{
    public ShellView()
    {
        InitializeComponent();

        Model = new DataContextWrapper<ShellViewModel>(this);
    }

    public DataContextWrapper<ShellViewModel> Model { get; }
}
