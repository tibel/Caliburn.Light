using Caliburn.Light;

namespace Demo.HelloWP8
{
    public class SecondPageViewModel : Screen
    {
        private int _numberOfTabs;

        public int NumberOfTabs
        {
            get { return _numberOfTabs; }
            set { SetProperty(ref _numberOfTabs, value); }
        }
    }
}
