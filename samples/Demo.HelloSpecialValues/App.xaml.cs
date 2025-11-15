using Caliburn.Light;
using Caliburn.Light.WinUI;
using Microsoft.UI.Xaml;
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
            Configure();

            _window = new Window();
            _window.Title = "Demo.HelloSpecialValues";

            var viewModel = new OverviewViewModel();
            var view = _container.GetRequiredInstance<IViewModelLocator>().LocateForModel(viewModel, null);

            ((FrameworkElement)view).DataContext = viewModel;
            _window.Content = view;
            _ = new WindowLifecycle(_window, null, false);

            _window.Activate();
        }
    }
}
