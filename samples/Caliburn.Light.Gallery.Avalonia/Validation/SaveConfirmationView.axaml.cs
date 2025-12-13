using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Caliburn.Light.Gallery.Avalonia.Validation;

public sealed partial class SaveConfirmationView : Window
{
    public SaveConfirmationView()
    {
        InitializeComponent();
    }

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
