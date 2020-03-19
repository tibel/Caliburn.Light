using Caliburn.Light;
using Caliburn.Light.WPF;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Demo.WinFormsInterop
{
    public sealed class MainViewModel : ViewAware
    {
        private readonly IWindowManager _windowManager;
        private string _name;

        public MainViewModel(IWindowManager windowManager)
        {
            if (windowManager is null)
                throw new ArgumentNullException(nameof(windowManager));

            _windowManager = windowManager;

            SayHelloCommand = DelegateCommandBuilder.NoParameter()
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

        public ICommand SayHelloCommand { get; }

        private Task SayHello()
        {
            var settings = new MessageBoxSettings
            {
                Text = string.Format("Hello {0}!", Name)
            };

            return _windowManager.ShowMessageBox(this, settings);
        }
    }
}
