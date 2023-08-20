using Caliburn.Light;

namespace Demo.Hierarchies
{
    public class ShellViewModel : Conductor<ChildLevel1ViewModel>.Collection.OneActive
    {
        public ShellViewModel(ChildLevel1ViewModel child)
        {
            ActiveItem = child;
        }
    }
}
