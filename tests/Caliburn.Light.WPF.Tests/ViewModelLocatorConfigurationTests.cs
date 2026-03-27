using System.ComponentModel;
using System.Windows.Controls;
using TUnit.Core.Executors;

namespace Caliburn.Light.WPF.Tests;

// Shared test types used across test files
public class TestView1 : UserControl { }
public class TestView2 : UserControl { }

public class TestViewModel1 : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged { add { } remove { } }
}

public class TestViewModel2 : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged { add { } remove { } }
}

internal class SimpleServiceProvider : IServiceProvider
{
    private readonly Dictionary<Type, object> _services = new();

    public SimpleServiceProvider() { }

    public SimpleServiceProvider(Type type, object instance)
    {
        _services[type] = instance;
    }

    public object? GetService(Type serviceType)
    {
        return _services.TryGetValue(serviceType, out var service) ? service : null;
    }
}

[TestExecutor<WpfTestExecutor>]
public class ViewModelLocatorConfigurationTests
{
    [Test]
    public async Task AddMapping_ReturnsSelf_ForFluentChaining()
    {
        var config = new ViewModelLocatorConfiguration();
        var result = config.AddMapping<TestView1, TestViewModel1>();
        await Assert.That(ReferenceEquals(result, config)).IsTrue();
    }

    [Test]
    public async Task AddMapping_MultipleMappings_CanChainFluently()
    {
        var config = new ViewModelLocatorConfiguration();
        var result = config
            .AddMapping<TestView1, TestViewModel1>()
            .AddMapping<TestView2, TestViewModel2>();
        await Assert.That(ReferenceEquals(result, config)).IsTrue();
    }

    [Test]
    public async Task AddMapping_WithContext_ReturnsSelf()
    {
        var config = new ViewModelLocatorConfiguration();
        var result = config.AddMapping<TestView1, TestViewModel1>("detail");
        await Assert.That(ReferenceEquals(result, config)).IsTrue();
    }

    [Test]
    public async Task AddMapping_SameViewDifferentContexts_DoesNotThrow()
    {
        var config = new ViewModelLocatorConfiguration();
        config.AddMapping<TestView1, TestViewModel1>()
              .AddMapping<TestView1, TestViewModel1>("context1")
              .AddMapping<TestView2, TestViewModel2>("context2");
        await Assert.That(config).IsNotNull();
    }
}
