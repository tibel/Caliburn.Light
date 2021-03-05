using Caliburn.Light.WinUI;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Demo.HelloSpecialValues
{
    public sealed partial class CharacterView : UserControl
    {
        public CharacterView()
        {
            InitializeComponent();

            Model = new DataContextWrapper<CharacterViewModel>(this);
        }

        public DataContextWrapper<CharacterViewModel> Model { get; }
    }
}
