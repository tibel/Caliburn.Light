using Caliburn.Light;
using Caliburn.Light.WinUI;
using Microsoft.UI.Xaml;
using System.Diagnostics.CodeAnalysis;

namespace Demo.HelloSpecialValues;

public sealed partial class App : Application
{
    private SimpleContainer? _container;

    public App()
    {
        InitializeComponent();
    }

    [MemberNotNull(nameof(_container))]
    private void Configure()
    {
        _container = new SimpleContainer();

        _container.RegisterSingleton<IWindowManager, WindowManager>();
        _container.RegisterSingleton<IEventAggregator, EventAggregator>();
        _container.RegisterSingleton<IViewModelLocator, ViewModelLocator>();
        _container.RegisterSingleton<IViewModelTypeResolver, ViewModelTypeResolver>();

        _container.RegisterPerRequest<OverviewViewModel>();

        var configuration = new ViewModelTypeConfiguration()
            .AddMapping<OverviewView, OverviewViewModel>()
            .AddMapping<CharacterView, CharacterViewModel>();

        _container.RegisterInstance(configuration);
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Configure();

        _container.GetRequiredInstance<IWindowManager>()
            .ShowWindow(_container.GetRequiredInstance<OverviewViewModel>());
    }
}
