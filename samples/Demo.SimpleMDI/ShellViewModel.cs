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
        private bool _canClosePending;

        public ShellViewModel(ILoggerFactory loggerFactory, Func<TabViewModel> createTabViewModel)
            : base(loggerFactory)
        {
            if (createTabViewModel is null)
                throw new ArgumentNullException(nameof(createTabViewModel));

            _createTabViewModel = createTabViewModel;

            OpenTabCommand = DelegateCommandBuilder.NoParameter()
                .OnExecute(() => OpenTab())
                .Build();

            CloseTabCommand = DelegateCommandBuilder.WithParameter<TabViewModel>()
                .OnExecute(item => DeactivateItem(item, true))
                .Build();
        }

        public ICommand OpenTabCommand { get; }

        public ICommand CloseTabCommand { get; }

        private void OpenTab()
        {
            var tab = _createTabViewModel();
            tab.DisplayName = "Tab " + ++_count;
            ActivateItem(tab);
        }

        public override async Task<bool> CanCloseAsync()
        {
            if (_canClosePending) return false;
            _canClosePending = true;

            await Task.Delay(1000);

            _canClosePending = false;
            return true;
        }
    }
}
