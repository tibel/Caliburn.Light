using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Caliburn.Light.Gallery.WPF.PageNavigation;

public sealed class Child1ViewModel : Screen
{
    public ICommand NavigateCommand { get; }

    public Child1ViewModel()
    {
        NavigateCommand = DelegateCommandBuilder.NoParameter()
            .OnExecute(Navigate)
            .Build();
    }

    private void Navigate()
    {
        if (((IViewAware)this).GetView() is Page page)
            page.NavigationService?.Navigate(new Uri("PageNavigation/Child2View.xaml", UriKind.Relative));
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
