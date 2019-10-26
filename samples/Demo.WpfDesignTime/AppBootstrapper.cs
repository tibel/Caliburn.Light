using Caliburn.Light;
using System.Windows;

namespace Demo.WpfDesignTime
{
    public class AppBootstrapper : BootstrapperBase
    {
        public AppBootstrapper()
        {
            LogManager.Initialize(new DebugLoggerFactory());
            Initialize();
        }

        protected override void Configure()
        {
            var container = new SimpleContainer();
            IoC.Initialize(container);

            container.RegisterSingleton<IWindowManager, WindowManager>();
            container.RegisterSingleton<IEventAggregator, EventAggregator>();
            container.RegisterSingleton<IViewModelLocator, ViewModelLocator>();

            var typeResolver = new ViewModelTypeResolver();
            typeResolver.AddMapping<ShellView, ShellViewModel>();
            typeResolver.AddMapping<NestedView, NestedViewModel>();
            container.RegisterInstance<IViewModelTypeResolver>(typeResolver);

            container.RegisterPerRequest<ShellViewModel>();
            container.RegisterPerRequest<NestedViewModel>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
