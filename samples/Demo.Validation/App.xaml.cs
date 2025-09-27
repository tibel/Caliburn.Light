using Caliburn.Light;
using Caliburn.Light.WPF;
using System.Windows;

namespace Demo.Validation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly SimpleContainer _container;

        public App()
        {
            _container = new SimpleContainer();

            _container.RegisterSingleton<IWindowManager, WindowManager>();
            _container.RegisterSingleton<IViewModelLocator, ViewModelLocator>();

            var typeResolver = new ViewModelTypeResolver()
                .AddMapping<MainWindow, MainViewModel>();
            _container.RegisterInstance<IViewModelTypeResolver>(typeResolver);

            _container.RegisterPerRequest<MainViewModel>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _container.GetRequiredInstance<IWindowManager>()
                .ShowWindow(_container.GetRequiredInstance<MainViewModel>());
        }
    }
}
