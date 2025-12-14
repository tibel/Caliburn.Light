using Caliburn.Light.Gallery.WinUI.Home;
using System;
using System.Windows.Input;

namespace Caliburn.Light.Gallery.WinUI;

public sealed partial class ShellViewModel : Conductor<IHaveDisplayName>, IHaveDisplayName
{
    public ICommand HomeCommand { get; }

    public string? DisplayName => "Caliburn.Light Gallery WinUI";

    public ShellViewModel(Func<HomeViewModel> createHomeViewModel)
    {
        HomeCommand = DelegateCommandBuilder.NoParameter()
            .OnExecute(() => ActivateItemAsync(createHomeViewModel.Invoke()))
            .OnCanExecute(() => ActiveItem is not HomeViewModel)
            .Observe(this, nameof(ActiveItem))
            .Build();

        ActiveItem = createHomeViewModel.Invoke();
    }
}
