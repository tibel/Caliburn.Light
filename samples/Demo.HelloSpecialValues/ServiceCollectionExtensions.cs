using Caliburn.Light;
using Caliburn.Light.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml;
using System.ComponentModel;

namespace Demo.HelloSpecialValues;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCaliburnLight(this IServiceCollection services)
        => services.AddSingleton<IWindowManager, WindowManager>()
            .AddSingleton<IEventAggregator, EventAggregator>()
            .AddSingleton<IViewModelLocator, ViewModelLocator>()
            .AddSingleton<IViewModelTypeResolver, ViewModelTypeResolver>()
            .AddTransient(c => c.GetRequiredService<IOptions<ViewModelTypeConfiguration>>().Value);

    public static IServiceCollection AddMapping<TView, TViewModel>(this IServiceCollection services, string? context = null)
        where TView : UIElement
        where TViewModel : class, INotifyPropertyChanged
        => services.Configure<ViewModelTypeConfiguration>(c => c.AddMapping<TView, TViewModel>(context));
}
