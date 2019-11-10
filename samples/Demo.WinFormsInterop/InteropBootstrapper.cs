using Caliburn.Light;
using Caliburn.Light.WPF;
using System.Windows.Forms.Integration;

namespace Demo.WinFormsInterop
{
    public class InteropBootstrapper
    {
        private readonly ElementHost _elementHost;

        public InteropBootstrapper(ElementHost elementHost)
        {
            _elementHost = elementHost;
        }

        public void Initialize()
        {
            var viewModel = new MainViewModel();

            var view = new MainView();

            view.DataContext = viewModel;
            if (viewModel is IViewAware viewAware)
                viewAware.AttachView(view, null);

            if (viewModel is IActivatable activator)
                activator.ActivateAsync().Observe();

            _elementHost.Child = view;
        }
    }
}
