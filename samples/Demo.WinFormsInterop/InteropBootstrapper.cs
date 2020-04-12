using Caliburn.Light;
using Caliburn.Light.WPF;
using System.Windows;
using System.Windows.Forms.Integration;

namespace Demo.WinFormsInterop
{
    public sealed class InteropBootstrapper
    {
        private readonly ElementHost _elementHost;
        private readonly SimpleContainer _container;

        public InteropBootstrapper(ElementHost elementHost)
        {
            _elementHost = elementHost;

            ViewHelper.Initialize(ViewAdapter.Instance);
            LogManager.Initialize(new DebugLoggerFactory());

            _container = new SimpleContainer();

            _container.RegisterSingleton<IWindowManager, WindowManager>();
            _container.RegisterSingleton<IViewModelLocator, ViewModelLocator>();

            var viewModelTypeResolver = new ViewModelTypeResolver()
                .AddMapping<MainView, MainViewModel>();

            _container.RegisterInstance<IViewModelTypeResolver>(viewModelTypeResolver);

            _container.RegisterPerRequest<MainViewModel>();
        }

        public void ShowView<TViewModel>(string context = null)
            where TViewModel : class
        {
            var viewModel = _container.GetInstance<TViewModel>();

            var view = _container.GetInstance<IViewModelLocator>()
                .LocateForModel(viewModel, context);

            View.SetBind(view, true);

            if (view is FrameworkElement fe)
                fe.DataContext = viewModel;

            _elementHost.Child = view;
        }
    }
}
