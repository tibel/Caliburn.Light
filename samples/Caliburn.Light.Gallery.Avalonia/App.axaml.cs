using Avalonia;
using Avalonia.Markup.Xaml;
using Caliburn.Light.Avalonia;
using Caliburn.Light.Gallery.Avalonia.Hierarchies;
using Caliburn.Light.Gallery.Avalonia.Home;
using Caliburn.Light.Gallery.Avalonia.PubSub;
using Caliburn.Light.Gallery.Avalonia.SimpleMDI;
using Caliburn.Light.Gallery.Avalonia.Threading;
using Caliburn.Light.Gallery.Avalonia.Validation;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Caliburn.Light.Gallery.Avalonia;

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

        services.AddView<ShellView, ShellViewModel>()
            .AddFunc<HomeViewModel>()
            .AddView<HomeView, HomeViewModel>();

        services.AddDemo<PubSubView, PubSubViewModel>("Pub/Sub");
        services.AddDemo<ValidationView, ValidationViewModel>("Validation")
            .AddView<SaveConfirmationView, SaveConfirmationViewModel>();
        services.AddDemo<SimpleMDIView, SimpleMDIViewModel>("Simple MDI");
        services.AddDemo<ThreadingView, ThreadingViewModel>("Threading");
        services.AddDemo<HierarchiesView, HierarchiesViewModel>("Hierarchies")
            .AddView<ChildLevel1View, ChildLevel1ViewModel>()
            .AddView<ChildLevel2View, ChildLevel2ViewModel>();

        return services.BuildServiceProvider();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var serviceProvider = Configure();

        serviceProvider.GetRequiredService<IWindowManager>()
            .ShowWindow(serviceProvider.GetRequiredService<ShellViewModel>());

        base.OnFrameworkInitializationCompleted();
    }
}
