using Caliburn.Light;
using System;
using System.Reflection;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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
        }

        private SimpleContainer _container;

        protected override void Configure()
        {
            LogManager.Initialize(new DebugLoggerFactory());

            _container = new SimpleContainer();

            _container.RegisterSingleton<IFrameAdapter, FrameAdapter>();
            _container.RegisterInstance<IUIContext>(new DispatcherUIContext(Window.Current.Dispatcher));
            _container.RegisterSingleton<IEventAggregator, EventAggregator>();
            _container.RegisterSingleton<IViewModelLocator, ViewModelLocator>();

            var typeResolver = new NameBasedViewModelTypeResolver()
                .AddAssembly(typeof(App).GetTypeInfo().Assembly);
            _container.RegisterInstance<IViewModelTypeResolver>(typeResolver);

            _container.RegisterSingleton<MainPageViewModel>();
            _container.RegisterPerRequest<PublisherViewModel>();
            _container.RegisterPerRequest<SubscriberViewModel>();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters
                DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            // Start the framework
            Initialize();

            var rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame is null)
            {
                // Create a Frame to act as the navigation context
                rootFrame = new Frame();
                _container.GetInstance<IFrameAdapter>().AttachTo(rootFrame);

                // Set the default language
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content is null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation parameter
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
    }
}
