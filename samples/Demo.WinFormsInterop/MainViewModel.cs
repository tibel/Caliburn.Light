using Caliburn.Light;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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

        public ICommand SayHelloCommand { get; private set; }

        private Task SayHello()
        {
            var message = new MessageBoxCoTask(string.Format("Hello {0}!", Name));
            return message.ExecuteAsync();
        }
    }
}
