using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Caliburn.Light.Avalonia;

namespace Caliburn.Light.Gallery.Avalonia.PageNavigation;

public sealed class Child1ViewModel : Screen
{
    public ICommand NavigateCommand { get; }

    public Child1ViewModel()
    {
        NavigateCommand = DelegateCommandBuilder.NoParameter()
            .OnExecute(NavigateAsync)
            .Build();
    }

    private async Task NavigateAsync()
    {
        if (((IViewAware)this).GetView() is ContentPage page)
            await page.Navigation!.PushAsync<Child2View>();
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
