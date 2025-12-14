namespace Caliburn.Light.Gallery.WinUI.Hierarchies;

public sealed partial class HierarchiesViewModel : Conductor<ChildLevel1ViewModel>.Collection.OneActive, IHaveDisplayName
{
    public string? DisplayName => "Hierarchies";

    public HierarchiesViewModel()
    {
        ActiveItem = new ChildLevel1ViewModel();
    }
}
