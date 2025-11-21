using Caliburn.Light;
using Caliburn.Light.WPF;
using System;
using System.Linq;
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
    }

    public ViewModelLocatorConfiguration Configuration => _container.Configuration;

    public void Show<TViewModel>(string? context = null)
        where TViewModel : class
    {
        var viewModel = _container.GetService(typeof(TViewModel))!;

        var view = _container.ViewModelLocator
            .LocateForModel(viewModel, context);

        View.SetViewModelLocator(view, _container.ViewModelLocator);
        View.SetBind(view, true);

        if (view is FrameworkElement fe)
            fe.DataContext = viewModel;

        _elementHost.Child = view;
    }

    private sealed class SimpleContainer : IServiceProvider
    {
        private readonly EventAggregator _eventAggregator;
        private readonly WindowManager _windowManager;

        public SimpleContainer()
        {
            _eventAggregator = new();
            Configuration = new();
            ViewModelLocator = new(Configuration, this);
            _windowManager = new(ViewModelLocator);
        }

        public ViewModelLocatorConfiguration Configuration { get; }

        public ViewModelLocator ViewModelLocator { get; }

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IWindowManager))
                return _windowManager;

            if (serviceType == typeof(IEventAggregator))
                return _eventAggregator;

            var constructor = serviceType.GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .First(c => c.IsPublic);

            var args = constructor.GetParameters()
                .Select(info => GetService(info.ParameterType))
                .ToArray();

            return Activator.CreateInstance(serviceType, args);
        }
    }
}
