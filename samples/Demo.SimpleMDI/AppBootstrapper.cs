using Caliburn.Light;
using System.Windows;

namespace Demo.SimpleMDI
{
    public class AppBootstrapper : BootstrapperBase
    {
        private SimpleContainer _container;

        public AppBootstrapper()
        {
            LogManager.Initialize(new DebugLoggerFactory());
            Initialize();
        }

        protected override void Configure()
        {
            _container = new SimpleContainer();

            _container.RegisterSingleton<IWindowManager, WindowManager>();
            _container.RegisterSingleton<IEventAggregator, EventAggregator>();
            _container.RegisterSingleton<IViewModelLocator, ViewModelLocator>();

            var typeResolver = new ViewModelTypeResolver()
                .AddMapping<ShellView, ShellViewModel>()
                .AddMapping<TabView, TabViewModel>();
            _container.RegisterInstance<IViewModelTypeResolver>(typeResolver);

            _container.RegisterPerRequest<ShellViewModel>();
            _container.RegisterPerRequest<TabViewModel>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _container.GetInstance<IWindowManager>()
                .ShowWindow(_container.GetInstance<ShellViewModel>());
        }
    }
}
