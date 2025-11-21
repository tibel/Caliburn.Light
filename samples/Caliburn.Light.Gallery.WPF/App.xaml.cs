using Caliburn.Light.Gallery.WPF.Hierarchies;
using Caliburn.Light.Gallery.WPF.Home;
using Caliburn.Light.Gallery.WPF.PageNavigation;
using Caliburn.Light.Gallery.WPF.PubSub;
using Caliburn.Light.Gallery.WPF.SimpleMDI;
using Caliburn.Light.Gallery.WPF.Threading;
using Caliburn.Light.Gallery.WPF.Validation;
using Caliburn.Light.WPF;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace Caliburn.Light.Gallery.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public sealed partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();

        services.AddCaliburnLight();

        services.AddView<ShellView, ShellViewModel>();
        services.AddView<HomeView, HomeViewModel>();
        services.AddFunc<HomeViewModel>();

        services.AddDemo<PubSubView, PubSubViewModel>("Pub/Sub");
        services.AddDemo<ValidationView, ValidationViewModel>("Validation");
        services.AddDemo<SimpleMDIView, SimpleMDIViewModel>("Simple MDI");
        services.AddDemo<ThreadingView, ThreadingViewModel>("Threading");

        services.AddDemo<HierarchiesView, HierarchiesViewModel>("Hierarchies");
        services.AddView<ChildLevel1View, ChildLevel1ViewModel>();
        services.AddView<ChildLevel2View, ChildLevel2ViewModel>();

        services.AddDemo<PageNavigationView, PageNavigationViewModel>("Page Navigation");
        services.AddView<Child1View, Child1ViewModel>();
        services.AddView<Child2View, Child2ViewModel>();

        _serviceProvider = services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _serviceProvider.GetRequiredService<IWindowManager>()
            .ShowWindow(_serviceProvider.GetRequiredService<ShellViewModel>());
    }
}
