namespace Caliburn.Light.Gallery.WinUI.Hierarchies;

public sealed partial class ChildLevel1ViewModel : Conductor<Screen>.Collection.OneActive
{
    public ChildLevel1ViewModel()
    {
        ActiveItem = new ChildLevel2ViewModel();
    }
}
