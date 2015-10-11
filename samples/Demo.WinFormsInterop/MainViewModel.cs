using Caliburn.Light;
using System.Windows;

namespace Demo.WinFormsInterop
{
    public class MainViewModel : BindableObject
    {
        private string _name;

        public MainViewModel()
        {
            SayHelloCommand = DelegateCommand.NoParameter()
                .OnExecute(() => SayHello())
                .OnCanExecute(() => !string.IsNullOrWhiteSpace(Name))
                .Observe(this, nameof(Name))
                .Build();
        }

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public IDelegateCommand SayHelloCommand { get; private set; }

        private void SayHello()
        {
            MessageBox.Show(string.Format("Hello {0}!", Name)); //Don't do this in real life :)
        }
    }
}
