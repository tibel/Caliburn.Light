using Caliburn.Light;

namespace Demo.HelloWP8
{
    public class MainPageViewModel : Screen
    {
        private readonly INavigationService _navigationService;

        public MainPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public void GotoPageTwo()
        {
            _navigationService.UriFor<SecondPageViewModel>()
                .WithParam(x => x.NumberOfTabs, 5)
                .Navigate();
        }
    }
}
