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
            var viewModelBinder = new ViewModelBinder();

            var view = new MainView();
            viewModelBinder.Bind(viewModel, view, null, true);

            var activator = viewModel as IActivate;
            if (activator != null)
                activator.Activate();

            _elementHost.Child = view;
        }
    }
}
