using Caliburn.Light.WinUI;
using Microsoft.UI.Xaml.Controls;

namespace Demo.HelloSpecialValues
{
    public sealed partial class OverviewView : UserControl
    {
        public OverviewView()
        {
            InitializeComponent();

            Model = new DataContextWrapper<OverviewViewModel>(this);
        }

        public DataContextWrapper<OverviewViewModel> Model { get; }
    }
}
