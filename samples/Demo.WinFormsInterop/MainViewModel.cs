using Caliburn.Light;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Demo.WinFormsInterop
{
    public class MainViewModel : BindableObject
    {
        private string _name;

        public MainViewModel()
        {
            SayHelloCommand = new AsyncDelegateCommand(() => SayHello(), () => !string.IsNullOrWhiteSpace(Name), this, nameof(Name));
        }

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public ICommand SayHelloCommand { get; private set; }

        private Task SayHello()
        {
            var message = new MessageBoxCoTask(string.Format("Hello {0}!", Name));
            return message.ExecuteAsync();
        }
    }
}
