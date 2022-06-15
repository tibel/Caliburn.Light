using Caliburn.Light;
using Caliburn.Light.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Demo.HelloSpecialValues
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private SimpleContainer _container;
        private Window _window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        private void Configure()
        {
            _container = new SimpleContainer();

            _container.RegisterSingleton<IFrameAdapter, FrameAdapter>();
            _container.RegisterSingleton<IEventAggregator, EventAggregator>();
            _container.RegisterSingleton<IViewModelLocator, ViewModelLocator>();

            var typeResolver = new ViewModelTypeResolver()
                .AddMapping<CharacterView, CharacterViewModel>()
                .AddMapping<MainPage, MainPageViewModel>();
            _container.RegisterInstance<IViewModelTypeResolver>(typeResolver);

            _container.RegisterSingleton<MainPageViewModel>();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();

            // Start the framework
            Configure();

            // Get the root frame
            var rootFrame = (Frame)_window.Content;
            rootFrame.NavigationFailed += OnNavigationFailed;

            // Attach the framework
            _container.GetInstance<IFrameAdapter>().AttachTo(rootFrame);

            // Navigate to the first page
            rootFrame.Navigate(typeof(MainPage), args.Arguments);

            _window.Activate();
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        public Window MainWindow => _window;
    }
}
