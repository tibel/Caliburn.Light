using Caliburn.Light;
using Caliburn.Light.WPF;
using System.Windows;

namespace Demo.HelloEventAggregator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private SimpleContainer _container;

        public App()
        {
            _container = new SimpleContainer();

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
            base.OnStartup(e);

            _container.GetRequiredInstance<IWindowManager>()
                .ShowWindow(_container.GetRequiredInstance<ShellViewModel>());
        }
    }
}
