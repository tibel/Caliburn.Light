using Caliburn.Light.Gallery.Avalonia.Home;
using System;
using System.Windows.Input;

namespace Caliburn.Light.Gallery.Avalonia;

public sealed class ShellViewModel : Conductor<IHaveDisplayName>, IHaveDisplayName
{
    public ICommand HomeCommand { get; }

    public string? DisplayName => "Caliburn.Light Gallery Avalonia";

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
