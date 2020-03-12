using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Light;
using Caliburn.Light.WPF;

namespace Demo.ExceptionHandling
{
    public class AppBootstrapper
    {
        private SimpleContainer _container;

        public void Initialize()
        {
            ViewHelper.Initialize(ViewAdapter.Instance);
            LogManager.Initialize(new DebugLoggerFactory());

            Application.Current.DispatcherUnhandledException += (_, e) => OnUnhandledException(e);
            Application.Current.Startup += (_, e) => OnStartup(e);

            _container = new SimpleContainer();

            _container.RegisterSingleton<IWindowManager, WindowManager>();
            _container.RegisterSingleton<IEventAggregator, EventAggregator>();
            _container.RegisterSingleton<IViewModelLocator, ViewModelLocator>();

            var typeResolver = new ViewModelTypeResolver()
                .AddMapping<ShellView, ShellViewModel>();
            _container.RegisterInstance<IViewModelTypeResolver>(typeResolver);

            _container.RegisterPerRequest<ShellViewModel>();
        }

        private void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            Debug.WriteLine(">>> Dispatcher - {0}", e.Exception);
        }

        private void OnStartup(StartupEventArgs e)
        {
            _container.GetInstance<IWindowManager>()
                .ShowWindow(_container.GetInstance<ShellViewModel>());
        }
    }
}
