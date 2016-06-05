using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Caliburn.Light
{
    public interface IPageAdapter
    {
        void OnNavigatingFrom(Page page, NavigatingCancelEventArgs e);

        void OnNavigatedFrom(Page page, NavigationEventArgs e);

        void OnNavigatedTo(Page page, NavigationEventArgs e);
    }
}
