using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using TUnit.Core.Executors;

namespace Caliburn.Light.WinUI.Tests;

public class SampleViewModel : INotifyPropertyChanged
{
#pragma warning disable CS0067
    public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
}

public class AnotherViewModel : INotifyPropertyChanged
{
#pragma warning disable CS0067
    public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
}

public class SampleView : Page { }

public class AnotherView : Page { }

[TestExecutor<WinUITestExecutor>]
public class ViewModelLocatorConfigurationTests
{
    [Test]
    public async Task AddMapping_ReturnsSelf_ForFluentChaining()
    {
        var config = new ViewModelLocatorConfiguration();
        var result = config.AddMapping<Page, SampleViewModel>();
        await Assert.That(object.ReferenceEquals(result, config)).IsTrue();
    }

    [Test]
    public async Task AddMapping_MultipleMappings_ReturnsFluentChain()
    {
        var config = new ViewModelLocatorConfiguration();
        var result = config
            .AddMapping<Page, SampleViewModel>()
            .AddMapping<UserControl, AnotherViewModel>();
        await Assert.That(object.ReferenceEquals(result, config)).IsTrue();
    }

    [Test]
    public async Task AddMapping_WithContext_ReturnsSelf()
    {
        var config = new ViewModelLocatorConfiguration();
        var result = config.AddMapping<Page, SampleViewModel>("detail");
        await Assert.That(object.ReferenceEquals(result, config)).IsTrue();
    }

    [Test]
    public async Task AddMapping_SameViewDifferentContexts_DoesNotThrow()
    {
        var config = new ViewModelLocatorConfiguration();
        config.AddMapping<Page, SampleViewModel>()
              .AddMapping<Page, SampleViewModel>("context1")
              .AddMapping<Page, AnotherViewModel>("context2");
        await Assert.That(config).IsNotNull();
    }

    [Test]
    public async Task AddMapping_ManyMappings_DoesNotThrow()
    {
        var config = new ViewModelLocatorConfiguration();
        config.AddMapping<Page, SampleViewModel>()
              .AddMapping<Page, SampleViewModel>("ctx1")
              .AddMapping<Page, AnotherViewModel>("ctx2")
              .AddMapping<UserControl, SampleViewModel>()
              .AddMapping<UserControl, AnotherViewModel>("ctx3");
        await Assert.That(config).IsNotNull();
    }

    [Test]
    public async Task AddMapping_NullContext_IsValid()
    {
        var config = new ViewModelLocatorConfiguration();
        var result = config.AddMapping<Page, SampleViewModel>(null);
        await Assert.That(object.ReferenceEquals(result, config)).IsTrue();
    }
}
