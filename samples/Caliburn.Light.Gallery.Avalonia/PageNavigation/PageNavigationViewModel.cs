using Avalonia.Controls;
using Caliburn.Light.Avalonia;

namespace Caliburn.Light.Gallery.Avalonia.PageNavigation;

public sealed class PageNavigationViewModel : ViewAware, IHaveDisplayName
{
    private readonly IViewModelLocator _viewModelLocator;

    public string? DisplayName => "Page Navigation";

    public PageNavigationViewModel(IViewModelLocator viewModelLocator)
    {
        _viewModelLocator = viewModelLocator;
    }

    protected override async void OnViewAttached(object view, string context)
    {
        base.OnViewAttached(view, context);

        if (view is UserControl userControl && userControl.Content is NavigationPage navigationPage)
        {
            new PageLifecycle(navigationPage, View.GetContext(navigationPage), _viewModelLocator);
            await navigationPage.PushAsync<Child1View>();
        }
    }
}
