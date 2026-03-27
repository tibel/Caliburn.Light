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

    [Test]
    public async Task LocateForModel_FindsView_WhenMappingExists()
    {
        var config = new ViewModelLocatorConfiguration()
            .AddMapping<TestView1, TestViewModel1>();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());

        var view = locator.LocateForModel(new TestViewModel1(), null);
        await Assert.That(view is TestView1).IsTrue();
    }

    [Test]
    public async Task LocateForModel_FindsCorrectView_WithMultipleMappings()
    {
        var config = new ViewModelLocatorConfiguration()
            .AddMapping<TestView1, TestViewModel1>()
            .AddMapping<TestView2, TestViewModel2>();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());

        var view1 = locator.LocateForModel(new TestViewModel1(), null);
        await Assert.That(view1 is TestView1).IsTrue();

        var view2 = locator.LocateForModel(new TestViewModel2(), null);
        await Assert.That(view2 is TestView2).IsTrue();
    }

    [Test]
    public async Task LocateForModel_WithContext_FindsCorrectView()
    {
        var config = new ViewModelLocatorConfiguration()
            .AddMapping<TestView1, TestViewModel1>("detail");
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());

        var view = locator.LocateForModel(new TestViewModel1(), "detail");
        await Assert.That(view is TestView1).IsTrue();
    }

    [Test]
    public async Task LocateForModel_WrongContext_ReturnsTextBlock()
    {
        var config = new ViewModelLocatorConfiguration()
            .AddMapping<TestView1, TestViewModel1>("detail");
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());

        var view = locator.LocateForModel(new TestViewModel1(), null);
        await Assert.That(view is TextBlock).IsTrue();
    }

    [Test]
    public async Task LocateForView_ReturnsDataContext_WhenPresent()
    {
        var config = new ViewModelLocatorConfiguration();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());

        var vm = new TestViewModel1();
        var view = new TestView1 { DataContext = vm };
        var result = locator.LocateForView(view);
        await Assert.That(ReferenceEquals(result, vm)).IsTrue();
    }

    [Test]
    public async Task LocateForView_ReturnsModelFromServiceProvider_WhenNoDataContext()
    {
        var expectedVm = new TestViewModel1();
        var config = new ViewModelLocatorConfiguration()
            .AddMapping<TestView1, TestViewModel1>();
        var sp = new SimpleServiceProvider(typeof(TestViewModel1), expectedVm);
        var locator = new ViewModelLocator(config, sp);

        var view = new TestView1();
        var result = locator.LocateForView(view);
        await Assert.That(ReferenceEquals(result, expectedVm)).IsTrue();
    }

    [Test]
    public async Task LocateForView_ReturnsNull_WhenNoMappingAndNoDataContext()
    {
        var config = new ViewModelLocatorConfiguration();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());

        var view = new TestView1();
        var result = locator.LocateForView(view);
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task LocateForModel_UnmappedType_ReturnsFallbackTextBlock()
    {
        var config = new ViewModelLocatorConfiguration();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());

        var view = locator.LocateForModel(new TestViewModel1(), null);

        await Assert.That(view).IsTypeOf<TextBlock>();
        var tb = (TextBlock)view;
        await Assert.That(tb.Text).Contains("Cannot find view");
    }
}
