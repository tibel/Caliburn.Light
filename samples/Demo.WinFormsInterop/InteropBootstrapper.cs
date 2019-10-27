using Caliburn.Light;
using System.Windows.Forms.Integration;

namespace Demo.WinFormsInterop
{
    public class InteropBootstrapper : BootstrapperBase
    {
        private readonly ElementHost _elementHost;

        public InteropBootstrapper(ElementHost elementHost)
        {
            _elementHost = elementHost;
        }

        protected override void Configure()
        {
            var viewModel = new MainViewModel();

            var view = new MainView();

            view.DataContext = viewModel;
            if (viewModel is IViewAware viewAware)
                viewAware.AttachView(view, null);

            if (viewModel is IActivate activator)
                activator.Activate();

            _elementHost.Child = view;
        }
    }
}
