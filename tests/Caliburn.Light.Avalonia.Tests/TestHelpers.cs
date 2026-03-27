namespace Caliburn.Light.Avalonia.Tests;

/// <summary>
/// Simple view model for testing.
/// </summary>
public class SampleViewModel : System.ComponentModel.INotifyPropertyChanged
{
#pragma warning disable CS0067
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
}

/// <summary>
/// Minimal service provider that returns null for all services, or a stored mapping.
/// </summary>
internal sealed class SimpleServiceProvider : IServiceProvider
{
    private readonly Dictionary<Type, object> _services = new();

    public SimpleServiceProvider() { }

    public SimpleServiceProvider(Type type, object instance)
    {
        _services[type] = instance;
    }

    public object? GetService(Type serviceType) =>
        _services.TryGetValue(serviceType, out var svc) ? svc : null;
}
