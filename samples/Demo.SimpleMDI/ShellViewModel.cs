using System.Threading.Tasks;
using Caliburn.Light;

namespace Demo.SimpleMDI
{
    public class ShellViewModel : Conductor<TabViewModel>.Collection.OneActive
    {
        int _count = 1;

        public void OpenTab()
        {
            var tab = IoC.GetInstance<TabViewModel>();
            tab.DisplayName = "Tab " + _count++;
            ActivateItem(tab);
        }

        public override async Task<bool> CanCloseAsync()
        {
            await Task.Delay(1000);
            return true;
        }
    }
}
