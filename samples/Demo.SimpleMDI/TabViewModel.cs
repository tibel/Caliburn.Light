using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Light;

namespace Demo.SimpleMDI
{
    public class TabViewModel : Screen, IChild, IHaveDisplayName
    {
        private object _parent;
        private string _displayName;

        public TabViewModel()
        {
            CloseCommand = DelegateCommandBuilder.NoParameter()
                .OnExecute(TryCloseAsync)
                .Build();
        }

        public ICommand CloseCommand { get; }

        public object Parent
        {
            get { return _parent; }
            set { SetProperty(ref _parent, value); }
        }

        public string DisplayName
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value); }
        }

        public override string ToString()
        {
            return DisplayName;
        }

        public override async Task<bool> CanCloseAsync()
        {
            await Task.Delay(500);
            return true;
        }
    }
}
