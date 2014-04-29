using Caliburn.Light;

namespace Demo.SimpleMDI
{
    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive
    {
        int _count = 1;

        public void OpenTab()
        {
            ActivateItem(new TabViewModel {DisplayName = "Tab " + _count++});
        }
    }
}
