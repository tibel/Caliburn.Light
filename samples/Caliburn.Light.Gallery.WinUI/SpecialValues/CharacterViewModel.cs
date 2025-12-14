using Caliburn.Light;

namespace Caliburn.Light.Gallery.WinUI.SpecialValues;

public sealed partial class CharacterViewModel : BindableObject
{
    public CharacterViewModel(string name, string image)
    {
        Name = name;
        Image = image;
    }

    public string Name { get; }

    public string Image { get; }
}
