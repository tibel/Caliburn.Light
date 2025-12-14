using Caliburn.Light;
using System.Windows.Input;

namespace Caliburn.Light.Gallery.WinUI.Validation;

public sealed partial class SaveConfirmationViewModel : Screen, IHaveDisplayName
{
    public SaveConfirmationViewModel()
    {
        OkCommand = DelegateCommandBuilder.NoParameter()
            .OnExecute(() => TryCloseAsync())
            .Build();
    }

    public string? DisplayName => "Save";

    public string Message => "Your changes were saved.";

    public ICommand OkCommand { get; }
}
