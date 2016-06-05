using Caliburn.Light;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Demo.HelloEventAggregator
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            IoC.GetInstance<IPageAdapter>().OnNavigatingFrom(this, e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            IoC.GetInstance<IPageAdapter>().OnNavigatedFrom(this, e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            IoC.GetInstance<IPageAdapter>().OnNavigatedTo(this, e);
        }
    }
}
