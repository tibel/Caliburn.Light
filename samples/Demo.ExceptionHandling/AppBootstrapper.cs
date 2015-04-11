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
            IoC.Initialize(_container);

            _container.RegisterSingleton<IWindowManager, WindowManager>();
            _container.RegisterSingleton<IEventAggregator, EventAggregator>();
            _container.RegisterPerRequest<ShellView>();
            _container.RegisterPerRequest<ShellViewModel>();
        }

        protected override void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            Debug.WriteLine(">>> Dispatcher - {0}", e.Exception);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
