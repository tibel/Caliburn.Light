using Caliburn.Light;
using Caliburn.Light.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Demo.HelloSpecialValues;

public sealed partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    public App()
    {
        InitializeComponent();
    }

    [MemberNotNull(nameof(_serviceProvider))]
    private void Configure()
    {
        var services = new ServiceCollection();

        services.AddCaliburnLight();

        services.AddTransient<OverviewViewModel>()
            .AddMapping<OverviewView, OverviewViewModel>()
            .AddMapping<CharacterView, CharacterViewModel>();

        _serviceProvider = services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Configure();

        base.OnLaunched(args);

        _serviceProvider.GetRequiredService<IWindowManager>()
            .ShowWindow(_serviceProvider.GetRequiredService<OverviewViewModel>());
    }
}
