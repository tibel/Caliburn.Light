using Caliburn.Light;
using Caliburn.Light.WPF;
using System.Windows;
using System.Windows.Forms.Integration;

namespace Demo.WinFormsInterop;

public sealed class InteropBootstrapper
{
    private readonly ElementHost _elementHost;
    private readonly SimpleContainer _container;

    public InteropBootstrapper(ElementHost elementHost)
    {
        _elementHost = elementHost;

        _container = new SimpleContainer();

        _container.RegisterSingleton<IWindowManager, WindowManager>();
        _container.RegisterSingleton<IViewModelLocator, ViewModelLocator>();
        _container.RegisterSingleton<IViewModelTypeResolver, ViewModelTypeResolver>();

        _container.RegisterInstance(ViewModelTypeMapping.Create<MainView, MainViewModel>());

        _container.RegisterPerRequest<MainViewModel>();
    }

    public void ShowView<TViewModel>(string? context = null)
        where TViewModel : class
    {
        var viewModel = _container.GetRequiredInstance<TViewModel>();

        var view = _container.GetRequiredInstance<IViewModelLocator>()
            .LocateForModel(viewModel, context);

        View.SetBind(view, true);

        if (view is FrameworkElement fe)
            fe.DataContext = viewModel;

        _elementHost.Child = view;
    }
}
