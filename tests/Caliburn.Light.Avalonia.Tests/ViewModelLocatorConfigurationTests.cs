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
        var config = new ViewModelLocatorConfiguration();
        config.AddMapping<TextBlock, SampleViewModel>()
              .AddMapping<Button, SampleViewModel>("detail");

        var serviceProvider = new SimpleServiceProvider();
        var locator = new ViewModelLocator(config, serviceProvider);

        var view = locator.LocateForModel(new SampleViewModel(), null);
        await Assert.That(view).IsTypeOf<TextBlock>();

        var detailView = locator.LocateForModel(new SampleViewModel(), "detail");
        await Assert.That(detailView).IsTypeOf<Button>();
    }
}

public class AnotherViewModel : System.ComponentModel.INotifyPropertyChanged
{
#pragma warning disable CS0067
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
}
