using Caliburn.Light;
using Caliburn.Light.WPF;
using System.Windows.Forms.Integration;

namespace Demo.WinFormsInterop
{
    public sealed class InteropBootstrapper
    {
        private readonly ElementHost _elementHost;

        public InteropBootstrapper(ElementHost elementHost)
        {
            _elementHost = elementHost;
        }

        public void Initialize()
        {
            ViewHelper.Initialize(ViewAdapter.Instance);
            LogManager.Initialize(new DebugLoggerFactory());

            var container = new SimpleContainer();

            container.RegisterSingleton<IWindowManager, WindowManager>();
            container.RegisterSingleton<IViewModelLocator, ViewModelLocator>();
            container.RegisterSingleton<IViewModelTypeResolver, ViewModelTypeResolver>();

            var viewModel = new MainViewModel(container.GetInstance<IWindowManager>());

            var view = new MainView();

            Bind.SetDataContext(view, true);

            view.DataContext = viewModel;

            _elementHost.Child = view;
        }
    }
}
