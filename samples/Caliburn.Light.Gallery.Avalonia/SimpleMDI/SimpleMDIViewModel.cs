using Caliburn.Light;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Caliburn.Light.Gallery.Avalonia.SimpleMDI;

public sealed class SimpleMDIViewModel : Conductor<TabViewModel>.Collection.OneActive, IHaveDisplayName
{
    private readonly Func<TabViewModel> _createTabViewModel;
    private int _count;

    public string? DisplayName => "Simple MDI";

    public SimpleMDIViewModel()
    {
        _createTabViewModel = () => new TabViewModel();

        OpenTabCommand = DelegateCommandBuilder.NoParameter()
            .OnExecute(OpenTabAsync)
            .Build();
    }

    public ICommand OpenTabCommand { get; }

    private Task OpenTabAsync()
    {
        var tab = _createTabViewModel();
        tab.DisplayName = "Tab " + ++_count;
        return ActivateItemAsync(tab);
    }

    public override async Task<bool> CanCloseAsync()
    {
        await base.CanCloseAsync();
        await Task.Delay(500);
        return true;
    }
}
