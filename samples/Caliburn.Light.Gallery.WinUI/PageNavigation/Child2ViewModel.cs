using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Caliburn.Light.Gallery.WinUI.PageNavigation;

public sealed partial class Child2ViewModel : Screen
{
    public ICommand NavigateCommand { get; }

    public Child2ViewModel()
    {
        NavigateCommand = DelegateCommandBuilder.NoParameter()
            .OnExecute(Navigate)
            .Build();
    }

    private void Navigate()
    {
        if (((IViewAware)this).GetView() is Page page && page.Frame is Frame frame)
            frame.Navigate(typeof(Child1View));
    }

    protected override Task OnActivateAsync()
    {
        return Task.Delay(100);
    }

    protected override Task OnDeactivateAsync(bool close)
    {
        return Task.Delay(100);
    }

    public override async Task<bool> CanCloseAsync()
    {
        await Task.Delay(100);
        return true;
    }
}
