using Caliburn.Light;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Controls;

namespace Demo.HelloSpecialValues
{
    public sealed partial class App : CaliburnApplication
    {
        public App()
        {
            InitializeComponent();
            LogManager.Initialize(t => new DebugLogger(t));
        }

        private SimpleContainer _container;

        protected override void Configure()
        {
            _container = new SimpleContainer();
            IoC.Initialize(_container);

            _container.RegisterSingleton<IEventAggregator, EventAggregator>();
            _container.RegisterSingleton<MainPageViewModel>();
        }

        protected override void PrepareViewFirst(Frame rootFrame)
        {
            _container.RegisterInstance<INavigationService>(new FrameAdapter(rootFrame));
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (args.PreviousExecutionState == ApplicationExecutionState.Running)
                return;

            DisplayRootViewFor<MainPageViewModel>();
        }
    }
}
