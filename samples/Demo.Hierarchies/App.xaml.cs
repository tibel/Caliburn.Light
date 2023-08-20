using Caliburn.Light;
using Caliburn.Light.WPF;
using System.Windows;

namespace Demo.Hierarchies
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

            _container.RegisterSingleton<IWindowManager, WindowManager>();
            _container.RegisterSingleton<IEventAggregator, EventAggregator>();
            _container.RegisterSingleton<IViewModelLocator, ViewModelLocator>();

            var typeResolver = new ViewModelTypeResolver()
                .AddMapping<ShellView, ShellViewModel>()
                .AddMapping<ChildLevel1View, ChildLevel1ViewModel>()
                .AddMapping<ChildLevel2View, ChildLevel2ViewModel>();
            _container.RegisterInstance<IViewModelTypeResolver>(typeResolver);

            _container.RegisterPerRequest<ShellViewModel>();
            _container.RegisterPerRequest<ChildLevel1ViewModel>();
            _container.RegisterPerRequest<ChildLevel2ViewModel>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _container.GetInstance<IWindowManager>()
                .ShowWindow(_container.GetInstance<ShellViewModel>());
        }
    }
}
