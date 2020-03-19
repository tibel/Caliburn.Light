using Caliburn.Light.WPF;
using System;
using System.Windows;
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
            var windowManager = new WindowManager(new NullViewModelLocator());

            var viewModel = new MainViewModel(windowManager);

            var view = new MainView();

            Bind.SetDataContext(view, true);

            view.DataContext = viewModel;

            _elementHost.Child = view;
        }

        private sealed class NullViewModelLocator : IViewModelLocator
        {
            public UIElement LocateForModel(object model, string context)
            {
                throw new NotImplementedException();
            }

            public object LocateForView(UIElement view)
            {
                throw new NotImplementedException();
            }
        }
    }
}
