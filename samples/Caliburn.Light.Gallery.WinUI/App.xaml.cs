using Caliburn.Light.Gallery.WinUI.Hierarchies;
using Caliburn.Light.Gallery.WinUI.Home;
using Caliburn.Light.Gallery.WinUI.PageNavigation;
using Caliburn.Light.Gallery.WinUI.PubSub;
using Caliburn.Light.Gallery.WinUI.SimpleMDI;
using Caliburn.Light.Gallery.WinUI.SpecialValues;
using Caliburn.Light.Gallery.WinUI.Threading;
using Caliburn.Light.Gallery.WinUI.Validation;
using Caliburn.Light.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Caliburn.Light.Gallery.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
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
        services.AddDemo<SpecialValuesView, SpecialValuesViewModel>("Special Values")
            .AddMapping<CharacterView, CharacterViewModel>()
            .AddMapping<CharacterDialogView, CharacterViewModel>("dialog");

        _serviceProvider = services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Configure();

        _serviceProvider.GetRequiredService<IWindowManager>()
            .ShowWindow(_serviceProvider.GetRequiredService<ShellViewModel>());
    }
}
