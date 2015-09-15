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
            typeResolver.Register<ShellView, ShellViewModel>();
            typeResolver.Register<TabView, TabViewModel>();

            //var typeResolver = new NameBasedViewModelTypeResolver();
            //typeResolver.AddAssembly(typeof(AppBootstrapper).Assembly);

            _container.RegisterInstance<IViewModelTypeResolver>(typeResolver);
            _container.RegisterPerRequest<IServiceLocator>(null, c => c);
            _container.RegisterSingleton<IViewModelLocator, ViewModelLocator>();
            _container.RegisterSingleton<IViewModelBinder, ViewModelBinder>();

            _container.RegisterPerRequest<ShellViewModel>();
            _container.RegisterPerRequest<ShellView>();
            _container.RegisterPerRequest<TabViewModel>();
            _container.RegisterPerRequest<TabView>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
