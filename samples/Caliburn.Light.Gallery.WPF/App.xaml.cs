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
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace Caliburn.Light.Gallery.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public sealed partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    [MemberNotNull(nameof(_serviceProvider))]
    private void Configure()
    {
        var services = new ServiceCollection();

        services.AddCaliburnLight();

        services.AddView<ShellView, ShellViewModel>()
            .AddFunc<HomeViewModel>()
            .AddView<HomeView, HomeViewModel>();

        services.AddDemo<PubSubView, PubSubViewModel>("Pub/Sub");
        services.AddDemo<ValidationView, ValidationViewModel>("Validation");
        services.AddDemo<SimpleMDIView, SimpleMDIViewModel>("Simple MDI");
        services.AddDemo<ThreadingView, ThreadingViewModel>("Threading");
        services.AddDemo<HierarchiesView, HierarchiesViewModel>("Hierarchies")
            .AddView<ChildLevel1View, ChildLevel1ViewModel>()
            .AddView<ChildLevel2View, ChildLevel2ViewModel>();
        services.AddDemo<PageNavigationView, PageNavigationViewModel>("Page Navigation")
            .AddView<Child1View, Child1ViewModel>()
            .AddView<Child2View, Child2ViewModel>();

        _serviceProvider = services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        Configure();

        base.OnStartup(e);

        _serviceProvider.GetRequiredService<IWindowManager>()
            .ShowWindow(_serviceProvider.GetRequiredService<ShellViewModel>());
    }
}
