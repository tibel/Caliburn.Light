using Caliburn.Light;
using System.Reflection;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Demo.HelloEventAggregator
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : CaliburnApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
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

            _container.RegisterSingleton<INavigationService, FrameAdapter>();
            _container.RegisterSingleton<ISuspensionManager, SuspensionManager>();
            _container.RegisterSingleton<IEventAggregator, EventAggregator>();
            _container.RegisterSingleton<IViewModelLocator, ViewModelLocator>();
            _container.RegisterSingleton<IViewModelBinder, ViewModelBinder>();

            var typeResolver = new NameBasedViewModelTypeResolver();
            typeResolver.AddAssembly(typeof(App).GetTypeInfo().Assembly);
            _container.RegisterInstance<IViewModelTypeResolver>(typeResolver);

            _container.RegisterSingleton<MainPageViewModel>();
            _container.RegisterPerRequest<PublisherViewModel>();
            _container.RegisterPerRequest<SubscriberViewModel>();
        }

        protected override void PrepareViewFirst(Frame rootFrame)
        {
            _container.RegisterInstance(rootFrame);
            _container.GetInstance<INavigationService>();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            if (e.PreviousExecutionState == ApplicationExecutionState.Running)
                return;

            DisplayRootViewFor<MainPageViewModel>();
        }
    }
}
