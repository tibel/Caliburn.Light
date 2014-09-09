using Caliburn.Light;
using System.Threading.Tasks;

namespace Demo.SimpleMDI
{
    public class ShellViewModel : Conductor<TabViewModel>.Collection.OneActive
    {
        private int _count = 1;
        private bool _canClosePending;

        public void OpenTab()
        {
            var tab = IoC.GetInstance<TabViewModel>();
            tab.DisplayName = "Tab " + _count++;
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
