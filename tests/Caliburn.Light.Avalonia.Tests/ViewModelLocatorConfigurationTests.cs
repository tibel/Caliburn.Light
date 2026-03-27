using Avalonia.Controls;
using TUnit.Core;
using TUnit.Core.Executors;

namespace Caliburn.Light.Avalonia.Tests;

[TestExecutor<AvaloniaTestExecutor>]
public class ViewModelLocatorConfigurationTests
{
    [Test]
    public async Task AddMapping_ReturnsSameInstance_FluentChain()
    {
        var config = new ViewModelLocatorConfiguration();
        var result = config.AddMapping<TextBlock, SampleViewModel>();

        await Assert.That(result).IsSameReferenceAs(config);
    }

    [Test]
    public async Task AddMapping_MultipleChainedCalls_ReturnsSameInstance()
    {
        var config = new ViewModelLocatorConfiguration();
        var result = config
            .AddMapping<TextBlock, SampleViewModel>()
            .AddMapping<Button, AnotherViewModel>();

        await Assert.That(result).IsSameReferenceAs(config);
    }

    [Test]
    public async Task AddMapping_WithContext_ReturnsSameInstance()
    {
        var config = new ViewModelLocatorConfiguration();
        var result = config.AddMapping<TextBlock, SampleViewModel>("detail");

        await Assert.That(result).IsSameReferenceAs(config);
    }

    [Test]
    public async Task AddMapping_WithAndWithoutContext_BothRegistered()
    {
        // Verify that adding the same view model type with different contexts
        // doesn't throw and the config can be used with a locator
        var config = new ViewModelLocatorConfiguration();
        config.AddMapping<TextBlock, SampleViewModel>()
              .AddMapping<Button, SampleViewModel>("detail");

        var serviceProvider = new SimpleServiceProvider();
        var locator = new ViewModelLocator(config, serviceProvider);

        // Default context resolves TextBlock
        var view = locator.LocateForModel(new SampleViewModel(), null);
        await Assert.That(view).IsTypeOf<TextBlock>();

        // "detail" context resolves Button
        var detailView = locator.LocateForModel(new SampleViewModel(), "detail");
        await Assert.That(detailView).IsTypeOf<Button>();
    }

    [Test]
    public async Task LocateForModel_UnmappedType_ReturnsFallbackTextBlock()
    {
        var config = new ViewModelLocatorConfiguration();
        var serviceProvider = new SimpleServiceProvider();
        var locator = new ViewModelLocator(config, serviceProvider);

        var view = locator.LocateForModel(new SampleViewModel(), null);
        var viewType = view.GetType().Name;
        var text = view is TextBlock tb ? tb.Text : null;

        // When no mapping is found, ViewModelLocator returns a TextBlock with an error message
        await Assert.That(viewType).IsEqualTo("TextBlock");
        await Assert.That(text).Contains("Cannot find view");
    }

    [Test]
    public async Task LocateForModel_FindsView_WhenMappingExists()
    {
        var config = new ViewModelLocatorConfiguration();
        config.AddMapping<TextBlock, SampleViewModel>();

        var serviceProvider = new SimpleServiceProvider();
        var locator = new ViewModelLocator(config, serviceProvider);

        var view = locator.LocateForModel(new SampleViewModel(), null);

        await Assert.That(view).IsTypeOf<TextBlock>();
    }

    [Test]
    public async Task LocateForModel_FindsCorrectView_WithMultipleMappings()
    {
        var config = new ViewModelLocatorConfiguration();
        config.AddMapping<TextBlock, SampleViewModel>()
              .AddMapping<Button, AnotherViewModel>();

        var serviceProvider = new SimpleServiceProvider();
        var locator = new ViewModelLocator(config, serviceProvider);

        var view1 = locator.LocateForModel(new SampleViewModel(), null);
        var view2 = locator.LocateForModel(new AnotherViewModel(), null);

        await Assert.That(view1).IsTypeOf<TextBlock>();
        await Assert.That(view2).IsTypeOf<Button>();
    }

    [Test]
    public async Task LocateForModel_WithContext_FindsCorrectView()
    {
        var config = new ViewModelLocatorConfiguration();
        config.AddMapping<Button, SampleViewModel>("detail");

        var serviceProvider = new SimpleServiceProvider();
        var locator = new ViewModelLocator(config, serviceProvider);

        var view = locator.LocateForModel(new SampleViewModel(), "detail");

        await Assert.That(view).IsTypeOf<Button>();
    }

    [Test]
    public async Task LocateForModel_WrongContext_ReturnsFallbackTextBlock()
    {
        var config = new ViewModelLocatorConfiguration();
        config.AddMapping<Button, SampleViewModel>("detail");

        var serviceProvider = new SimpleServiceProvider();
        var locator = new ViewModelLocator(config, serviceProvider);

        var view = locator.LocateForModel(new SampleViewModel(), null);
        var viewType = view.GetType().Name;
        var text = view is TextBlock tb ? tb.Text : null;

        await Assert.That(viewType).IsEqualTo("TextBlock");
        await Assert.That(text).Contains("Cannot find view");
    }

    [Test]
    public async Task LocateForView_ReturnsDataContext_WhenPresent()
    {
        var config = new ViewModelLocatorConfiguration();
        var serviceProvider = new SimpleServiceProvider();
        var locator = new ViewModelLocator(config, serviceProvider);

        var vm = new SampleViewModel();
        var view = new TextBlock { DataContext = vm };
        var result = locator.LocateForView(view);

        await Assert.That(result).IsTypeOf<SampleViewModel>();
    }

    [Test]
    public async Task LocateForView_ReturnsModelFromServiceProvider_WhenNoDataContext()
    {
        var expectedVm = new SampleViewModel();
        var config = new ViewModelLocatorConfiguration()
            .AddMapping<TextBlock, SampleViewModel>();
        var sp = new SimpleServiceProvider(typeof(SampleViewModel), expectedVm);
        var locator = new ViewModelLocator(config, sp);

        var view = new TextBlock();
        var result = locator.LocateForView(view);

        await Assert.That(result).IsSameReferenceAs(expectedVm);
    }

    [Test]
    public async Task LocateForView_ReturnsNull_WhenNoMappingAndNoDataContext()
    {
        var config = new ViewModelLocatorConfiguration();
        var serviceProvider = new SimpleServiceProvider();
        var locator = new ViewModelLocator(config, serviceProvider);

        var view = new TextBlock();
        var result = locator.LocateForView(view);

        await Assert.That(result).IsNull();
    }
}

public class AnotherViewModel : System.ComponentModel.INotifyPropertyChanged
{
#pragma warning disable CS0067
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
}
