using Avalonia;
using Avalonia.Markup.Xaml;
using Caliburn.Light.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Demo.HelloAvalonia;

public sealed partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private IServiceProvider Configure()
    {
        var services = new ServiceCollection();

        services.AddCaliburnLight();

        services.AddTransient<MainWindowViewModel>()
            .AddMapping<MainWindow, MainWindowViewModel>();

        return services.BuildServiceProvider();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var serviceProvider = Configure();

        serviceProvider.GetRequiredService<IWindowManager>()
            .ShowWindow(serviceProvider.GetRequiredService<MainWindowViewModel>());

        base.OnFrameworkInitializationCompleted();
    }

}
