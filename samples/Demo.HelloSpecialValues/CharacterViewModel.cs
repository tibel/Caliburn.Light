using Caliburn.Light;

namespace Demo.HelloSpecialValues
{
    public class CharacterViewModel : BindableObject
    {
        public CharacterViewModel(string name, string image)
        {
            Name = name;
            Image = image;
        }

        public string Name
        {
            get;
            private set;
        }

        public string Image
        {
            get;
            private set;
        }
    }
}
