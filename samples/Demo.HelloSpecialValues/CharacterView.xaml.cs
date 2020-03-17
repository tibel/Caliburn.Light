using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Demo.HelloSpecialValues
{
    public sealed partial class CharacterView : UserControl
    {
        public CharacterView()
        {
            InitializeComponent();
        }

        public CharacterViewModel Model => DataContext as CharacterViewModel;
    }
}
