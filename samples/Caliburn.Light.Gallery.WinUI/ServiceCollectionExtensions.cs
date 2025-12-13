using Caliburn.Light.Gallery.WinUI.Home;
using Caliburn.Light.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml;
using System;
using System.ComponentModel;

namespace Caliburn.Light.Gallery.WinUI;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCaliburnLight(this IServiceCollection services)
        => services.AddSingleton<IWindowManager, WindowManager>()
            .AddSingleton<IEventAggregator, EventAggregator>()
            .AddSingleton<IViewModelLocator, ViewModelLocator>()
            .AddTransient(c => c.GetRequiredService<IOptions<ViewModelLocatorConfiguration>>().Value);

    public static IServiceCollection AddMapping<TView, TViewModel>(this IServiceCollection services, string? context = null)
        where TView : UIElement
        where TViewModel : class, INotifyPropertyChanged
        => services.Configure<ViewModelLocatorConfiguration>(c => c.AddMapping<TView, TViewModel>(context));

    public static IServiceCollection AddFunc<TService>(this IServiceCollection services)
        where TService : class
        => services.AddTransient<Func<TService>>(c => () => c.GetRequiredService<TService>());

    public static IServiceCollection AddHomeItem<TViewModel>(this IServiceCollection services, string displayName)
        where TViewModel : class, INotifyPropertyChanged
        => services.AddTransient(c => new HomeItemViewModel(displayName, () => c.GetRequiredService<TViewModel>()));

    public static IServiceCollection AddView<TView, TViewModel>(this IServiceCollection services)
        where TView : UIElement
        where TViewModel : class, INotifyPropertyChanged
        => services.AddMapping<TView, TViewModel>()
            .AddTransient<TView>()
            .AddTransient<TViewModel>();

    public static IServiceCollection AddDemo<TView, TViewModel>(this IServiceCollection services, string displayName)
        where TView : UIElement
        where TViewModel : class, INotifyPropertyChanged
        => services.AddView<TView, TViewModel>()
            .AddHomeItem<TViewModel>(displayName);
}
