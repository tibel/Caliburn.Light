using Caliburn.Light;

namespace Demo.Hierarchies
{
    public class ChildLevel1ViewModel : Conductor<Screen>.Collection.OneActive
    {
        public ChildLevel1ViewModel(ChildLevel2ViewModel child)
        {
            ActiveItem = child;
        }
    }
}
