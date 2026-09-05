using System.Threading.Tasks;
using System.Windows.Input;

namespace Caliburn.Light.Gallery.WPF.SimpleMDI;

public sealed class TabViewModel : Screen, IHaveDisplayName
{
    private string? _displayName;

    public TabViewModel()
    {
        CloseCommand = DelegateCommandBuilder.NoParameter()
            .OnExecute(TryCloseAsync)
            .Build();
    }

    public ICommand CloseCommand { get; }

    public string? DisplayName
    {
        get { return _displayName; }
        set { SetProperty(ref _displayName, value); }
    }

    public override string? ToString()
    {
        return DisplayName;
    }

    public override async Task<bool> CanCloseAsync()
    {
        await Task.Delay(500);
        return true;
    }
}
