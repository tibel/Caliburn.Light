using Caliburn.Light.WinUI;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Caliburn.Light.Gallery.WinUI.PageNavigation;

public sealed partial class PageNavigationViewModel : ViewAware, IHaveDisplayName
{
    private readonly IViewModelLocator _viewModelLocator;

    public string? DisplayName => "Page Navigation";

    public PageNavigationViewModel(IViewModelLocator viewModelLocator)
    {
        _viewModelLocator = viewModelLocator;
    }

    protected override void OnViewAttached(object view, string context)
    {
        base.OnViewAttached(view, context);

        if (view is UserControl userControl && userControl.Content is Frame frame)
        {
            new PageLifecycle(frame, View.GetContext(frame), _viewModelLocator).NavigationService
                .Navigate(typeof(Child1View));
        }
    }
}
