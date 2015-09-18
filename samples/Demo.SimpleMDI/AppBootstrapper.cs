using Caliburn.Light;
using System.Windows;

namespace Demo.SimpleMDI
{
    public class AppBootstrapper : BootstrapperBase
    {
        private SimpleContainer _container;

        public AppBootstrapper()
        {
            LogManager.Initialize(type => new DebugLogger(type));
            Initialize();
        }

        protected override void Configure()
        {
            _container = new SimpleContainer();
            IoC.Initialize(_container);

            _container.RegisterSingleton<IWindowManager, WindowManager>();
            _container.RegisterSingleton<IEventAggregator, EventAggregator>();

            var typeResolver = new ViewModelTypeResolver();
            typeResolver.AddMapping<ShellView, ShellViewModel>();
            typeResolver.AddMapping<TabView, TabViewModel>();

            //var typeResolver = new NameBasedViewModelTypeResolver();
            //typeResolver.AddAssembly(typeof(AppBootstrapper).Assembly);

            _container.RegisterInstance<IViewModelTypeResolver>(typeResolver);
            _container.RegisterSingleton<IViewModelLocator, ViewModelLocator>();
            _container.RegisterSingleton<IViewModelBinder, ViewModelBinder>();

            _container.RegisterPerRequest<ShellViewModel>();
            _container.RegisterPerRequest<TabViewModel>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
