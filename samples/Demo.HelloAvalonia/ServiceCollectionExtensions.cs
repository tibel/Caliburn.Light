using Avalonia.Controls;
using Caliburn.Light;
using Caliburn.Light.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.ComponentModel;

namespace Demo.HelloAvalonia;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCaliburnLight(this IServiceCollection services)
        => services.AddSingleton<IWindowManager, WindowManager>()
            .AddSingleton<IEventAggregator, EventAggregator>()
            .AddSingleton<IViewModelLocator, ViewModelLocator>()
            .AddTransient(c => c.GetRequiredService<IOptions<ViewModelLocatorConfiguration>>().Value);

    public static IServiceCollection AddMapping<TView, TViewModel>(this IServiceCollection services, string? context = null)
        where TView : Control
        where TViewModel : class, INotifyPropertyChanged
        => services.Configure<ViewModelLocatorConfiguration>(c => c.AddMapping<TView, TViewModel>(context));
}
