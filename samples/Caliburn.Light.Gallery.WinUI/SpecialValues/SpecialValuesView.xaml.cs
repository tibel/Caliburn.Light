using Caliburn.Light.WinUI;
using Microsoft.UI.Xaml.Controls;

namespace Caliburn.Light.Gallery.WinUI.SpecialValues;

public sealed partial class SpecialValuesView : UserControl
{
    public SpecialValuesView()
    {
        InitializeComponent();

        Model = new DataContextWrapper<SpecialValuesViewModel>(this);
    }

    public DataContextWrapper<SpecialValuesViewModel> Model { get; }
}
