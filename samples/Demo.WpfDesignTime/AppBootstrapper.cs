using Caliburn.Light;
using System.Windows;

namespace Demo.WpfDesignTime
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

            var typeResolver = new ViewModelTypeResolver();
            typeResolver.AddMapping<ShellView, ShellViewModel>();
            typeResolver.AddMapping<NestedView, NestedViewModel>();
            _container.RegisterInstance<IViewModelTypeResolver>(typeResolver);

            _container.RegisterPerRequest<ShellViewModel>();
            _container.RegisterPerRequest<NestedViewModel>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _container.ShowWindowFor<ShellViewModel>();
        }
    }
}
