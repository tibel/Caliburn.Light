using Caliburn.Light.WinUI;
using Microsoft.UI.Xaml.Controls;

namespace Caliburn.Light.Gallery.WinUI.Validation;

public sealed partial class SaveConfirmationView : ContentDialog
{
    public SaveConfirmationView()
    {
        InitializeComponent();

        Model = new DataContextWrapper<SaveConfirmationViewModel>(this);
    }

    public DataContextWrapper<SaveConfirmationViewModel> Model { get; }
}
