using Caliburn.Light;
using Caliburn.Light.WPF;
using System.Windows;
using System.Windows.Threading;

namespace Demo.HelloEventAggregator
{
    public class AppBootstrapper : BootstrapperBase
    {
        private SimpleContainer _container;

        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            LogManager.Initialize(new DebugLoggerFactory());

            _container = new SimpleContainer();

            _container.RegisterInstance<IUIContext>(new DispatcherUIContext(Dispatcher.CurrentDispatcher));
            _container.RegisterSingleton<IEventAggregator, EventAggregator>();

            _container.RegisterSingleton<IWindowManager, WindowManager>();
            _container.RegisterSingleton<IViewModelLocator, ViewModelLocator>();

            var typeResolver = new ViewModelTypeResolver()
                .AddMapping<ShellView, ShellViewModel>()
                .AddMapping<PublisherView, PublisherViewModel>()
                .AddMapping<SubscriberView, SubscriberViewModel>();
            _container.RegisterInstance<IViewModelTypeResolver>(typeResolver);

            _container.RegisterPerRequest<ShellViewModel>();
            _container.RegisterPerRequest<PublisherViewModel>();
            _container.RegisterPerRequest<SubscriberViewModel>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _container.GetInstance<IWindowManager>()
                .ShowWindow(_container.GetInstance<ShellViewModel>());
        }
    }
}
