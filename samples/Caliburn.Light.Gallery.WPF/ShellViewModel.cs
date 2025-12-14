using Caliburn.Light.Gallery.WPF.Home;
using System;
using System.Windows.Input;

namespace Caliburn.Light.Gallery.WPF;

public sealed class ShellViewModel : Conductor<IHaveDisplayName>, IHaveDisplayName
{
    public ICommand HomeCommand { get; }

    public string? DisplayName => "Caliburn.Light Gallery WPF";

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
