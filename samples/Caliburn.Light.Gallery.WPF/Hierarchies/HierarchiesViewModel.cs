using Caliburn.Light;

namespace Caliburn.Light.Gallery.WPF.Hierarchies
{
    public sealed class HierarchiesViewModel : Conductor<ChildLevel1ViewModel>.Collection.OneActive, IHaveDisplayName
    {
        public string? DisplayName => "Hierarchies";

        public HierarchiesViewModel()
        {
            ActiveItem = new ChildLevel1ViewModel();
        }
    }
}
