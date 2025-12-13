using Caliburn.Light;

namespace Caliburn.Light.Gallery.Avalonia.Validation;

public sealed class SaveConfirmationViewModel : BindableObject, IHaveDisplayName
{
    public string? DisplayName => "Save";
    
    public string Message => "Your changes were saved.";
}
