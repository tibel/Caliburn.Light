using Caliburn.Light.WinUI;
using Microsoft.UI.Xaml.Controls;

namespace Caliburn.Light.Gallery.WinUI.SpecialValues;

public sealed partial class CharacterView : UserControl
{
    public CharacterView()
    {
        InitializeComponent();

        Model = new DataContextWrapper<CharacterViewModel>(this);
    }

    public DataContextWrapper<CharacterViewModel> Model { get; }
}
