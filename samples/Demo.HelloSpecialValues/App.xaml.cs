using Caliburn.Light;
using Caliburn.Light.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics.CodeAnalysis;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Demo.HelloSpecialValues
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        private SimpleContainer? _container;
        private Window? _window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        [MemberNotNull(nameof(_container))]
        private void Configure()
        {
            _container = new SimpleContainer();

            _container.RegisterSingleton<IEventAggregator, EventAggregator>();
            _container.RegisterSingleton<IViewModelLocator, ViewModelLocator>();
            _container.RegisterSingleton<IViewModelTypeResolver, ViewModelTypeResolver>();

            _container.RegisterInstance(ViewModelTypeMapping.Create<MainPage, MainPageViewModel>());
            _container.RegisterInstance(ViewModelTypeMapping.Create<CharacterView, CharacterViewModel>());

            _container.RegisterPerRequest<MainPageViewModel>();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Title = "Demo.HelloSpecialValues";

            // Start the framework
            Configure();

            // Get the root frame
            var rootFrame = (Frame)_window.Content;
            rootFrame.NavigationFailed += OnNavigationFailed;

            // Attach the framework
            _ = new PageLifecycle(rootFrame, null, _container.GetRequiredInstance<IViewModelLocator>());

            // Navigate to the first page
            rootFrame.Navigate(typeof(MainPage), args.Arguments);

            _window.Activate();
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        public Window? MainWindow => _window;
    }
}
