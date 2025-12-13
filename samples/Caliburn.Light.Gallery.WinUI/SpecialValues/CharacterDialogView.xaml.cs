using Caliburn.Light.WinUI;
using Microsoft.UI.Xaml.Controls;

namespace Caliburn.Light.Gallery.WinUI.SpecialValues;

public sealed partial class CharacterDialogView : ContentDialog
{
    public CharacterDialogView()
    {
        InitializeComponent();

        Model = new DataContextWrapper<CharacterViewModel>(this);
    }

    public DataContextWrapper<CharacterViewModel> Model { get; }
}
