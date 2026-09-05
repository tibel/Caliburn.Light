using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Caliburn.Light.Gallery.WinUI.Home;

public sealed partial class HomeViewModel : Conductor<HomeItemViewModel>.Collection.AllActive, IHaveDisplayName
{
    public HomeViewModel(IEnumerable<HomeItemViewModel> items)
    {
        OpenCommand = DelegateCommandBuilder.WithParameter<HomeItemViewModel>()
            .OnExecute(OpenAsync)
            .Build();

        Items.AddRange(items);
    }

    public string? DisplayName => "Demos";

    public ICommand? OpenCommand { get; }

    private Task OpenAsync(HomeItemViewModel? item)
    {
        if (item is null || Parent is not IConductor conductor)
            return Task.CompletedTask;

        return conductor.ActivateItemAsync(item.CreateInstance());
    }
}
