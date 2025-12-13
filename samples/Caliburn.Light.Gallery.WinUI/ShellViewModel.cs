using Caliburn.Light.Gallery.WinUI.Home;
using Caliburn.Light.WinUI;
using System;
using System.Windows.Input;

namespace Caliburn.Light.Gallery.WinUI;

public sealed class ShellViewModel : Conductor<IHaveDisplayName>, IHaveDisplayName
{
    public ICommand HomeCommand { get; }

    public string? DisplayName => "Caliburn.Light Gallery";

    public IViewModelLocator ViewModelLocator { get; }

    public ShellViewModel(Func<HomeViewModel> createHomeViewModel, IViewModelLocator viewModelLocator)
    {
        ViewModelLocator = viewModelLocator;

        HomeCommand = DelegateCommandBuilder.NoParameter()
            .OnExecute(() => ActivateItemAsync(createHomeViewModel.Invoke()))
            .OnCanExecute(() => ActiveItem is not HomeViewModel)
            .Observe(this, nameof(ActiveItem))
            .Build();

        ActiveItem = createHomeViewModel.Invoke();
    }
}
