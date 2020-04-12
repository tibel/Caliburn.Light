using System;
using System.Windows.Forms;

namespace Demo.WinFormsInterop
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            var bootstrapper = new InteropBootstrapper(elementHost1);
            bootstrapper.ShowView<MainViewModel>();

            base.OnLoad(e);
        }
    }
}
