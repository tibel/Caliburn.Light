using Caliburn.Light;
using Caliburn.Light.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics.CodeAnalysis;

namespace Demo.HelloSpecialValues
{
    public sealed partial class App : Application
    {
        private SimpleContainer? _container;
        private Window? _window;

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

            _container.RegisterInstance(ViewModelTypeMapping.Create<OverviewView, OverviewViewModel>());
            _container.RegisterInstance(ViewModelTypeMapping.Create<CharacterView, CharacterViewModel>());
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = new Window();
            _window.Title = "Demo.HelloSpecialValues";
            _window.Content = new ContentControl();

            // Start the framework
            Configure();

            var contentControl = (ContentControl)_window.Content;
            View.SetCreate(contentControl, true);
            View.SetViewModelLocator(contentControl, _container.GetRequiredInstance<IViewModelLocator>());

            // Set the initial ViewModel
            contentControl.DataContext = new OverviewViewModel();

            _window.Activate();
        }

        public Window? MainWindow => _window;
    }
}
