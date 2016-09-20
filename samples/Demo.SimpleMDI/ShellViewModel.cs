﻿using Caliburn.Light;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Demo.SimpleMDI
{
    public class ShellViewModel : Conductor<TabViewModel>.Collection.OneActive
    {
        private readonly Func<TabViewModel> _createTabViewModel;
        private int _count = 0;
        private bool _canClosePending;

        public ShellViewModel(Func<TabViewModel> createTabViewModel)
        {
            if (createTabViewModel == null)
                throw new ArgumentNullException(nameof(createTabViewModel));

            _createTabViewModel = createTabViewModel;

            OpenTabCommand = new DelegateCommand(() => OpenTab());

            CloseTabCommand = new DelegateCommand<TabViewModel>(CoerceParameter<TabViewModel>.Default, item => DeactivateItem(item, true));
        }

        public ICommand OpenTabCommand { get; private set; }

        public ICommand CloseTabCommand { get; private set; }

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
