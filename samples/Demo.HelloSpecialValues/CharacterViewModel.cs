using Caliburn.Light;

namespace Demo.HelloSpecialValues
{
    public sealed class CharacterViewModel : BindableObject
    {
        public CharacterViewModel(string name, string image)
        {
            Name = name;
            Image = image;
        }

        public string Name { get; }

        public string Image { get; }
    }
}
