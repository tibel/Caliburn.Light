using Caliburn.Light;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Demo.SimpleMDI
{
    public class ShellViewModel : Conductor<TabViewModel>.Collection.OneActive
    {
        private readonly Func<TabViewModel> _createTabViewModel;
        private int _count;

        public ShellViewModel(Func<TabViewModel> createTabViewModel)
        {
            if (createTabViewModel is null)
                throw new ArgumentNullException(nameof(createTabViewModel));

            _createTabViewModel = createTabViewModel;

            OpenTabCommand = DelegateCommandBuilder.NoParameter()
                .OnExecute(() => OpenTabAsync())
                .Build();

            CloseTabCommand = DelegateCommandBuilder.WithParameter<TabViewModel>()
                .OnExecute(item => DeactivateItemAsync(item, true))
                .Build();
        }

        public ICommand OpenTabCommand { get; }

        public ICommand CloseTabCommand { get; }

        private Task OpenTabAsync()
        {
            var tab = _createTabViewModel();
            tab.DisplayName = "Tab " + ++_count;
            return ActivateItemAsync(tab);
        }

        public override async Task<bool> CanCloseAsync()
        {
            await base.CanCloseAsync();
            await Task.Delay(500);
            return true;
        }
    }
}
