using Caliburn.Light;
using System.Windows;

namespace Demo.WinFormsInterop
{
    public class MainViewModel : BindableObject
    {
        string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (SetProperty(ref _name, value))
                    RaisePropertyChanged(() => CanSayHello);
            }
        }

        public bool CanSayHello
        {
            get { return !string.IsNullOrWhiteSpace(Name); }
        }

        public void SayHello()
        {
            MessageBox.Show(string.Format("Hello {0}!", Name)); //Don't do this in real life :)
        }
    }
}
