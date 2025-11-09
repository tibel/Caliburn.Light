using Caliburn.Light;
using Caliburn.Light.WPF;
using System;
using System.Windows.Controls;

namespace Caliburn.Light.Gallery.WPF.PageNavigation
{
    public sealed class PageNavigationViewModel : ViewAware, IHaveDisplayName
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
                new PageLifecycle(frame.NavigationService, View.GetContext(frame), _viewModelLocator).NavigationService
                    .Navigate(new Uri("PageNavigation/Child1View.xaml", UriKind.Relative));
            }
        }
    }
}
