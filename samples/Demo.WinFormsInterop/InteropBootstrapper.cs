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

            var activator = viewModel as IActivate;
            if (activator != null)
                activator.Activate();

            _elementHost.Child = view;
        }
    }
}
