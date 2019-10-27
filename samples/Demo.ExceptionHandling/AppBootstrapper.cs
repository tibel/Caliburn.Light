using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Light;

namespace Demo.ExceptionHandling
{
    public class AppBootstrapper : BootstrapperBase
    {
        private SimpleContainer _container;

        protected override void Configure()
        {
            _container = new SimpleContainer();

            _container.RegisterSingleton<IWindowManager, WindowManager>();
            _container.RegisterSingleton<IEventAggregator, EventAggregator>();
            _container.RegisterSingleton<IViewModelLocator, ViewModelLocator>();

            var typeResolver = new ViewModelTypeResolver()
                .AddMapping<ShellView, ShellViewModel>();
            _container.RegisterInstance<IViewModelTypeResolver>(typeResolver);

            _container.RegisterPerRequest<ShellViewModel>();
        }

        protected override void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            Debug.WriteLine(">>> Dispatcher - {0}", e.Exception);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _container.GetInstance<IWindowManager>()
                .ShowWindow(_container.GetInstance<ShellViewModel>());
        }
    }
}
