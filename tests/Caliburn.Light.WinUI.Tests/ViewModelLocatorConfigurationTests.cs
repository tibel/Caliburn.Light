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

    [Test]
    public async Task ViewModelLocator_Constructor_WithValidArgs_Succeeds()
    {
        var config = new ViewModelLocatorConfiguration()
            .AddMapping<Page, SampleViewModel>();
        var sp = new SimpleServiceProvider();
        var locator = new ViewModelLocator(config, sp);
        await Assert.That(locator).IsNotNull();
    }

    [Test]
    public async Task ViewModelLocator_Constructor_NullConfig_Throws()
    {
        await Assert.That(() => new ViewModelLocator(null!, new SimpleServiceProvider()))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ViewModelLocator_Constructor_NullServiceProvider_Throws()
    {
        var config = new ViewModelLocatorConfiguration();
        await Assert.That(() => new ViewModelLocator(config, null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task LocateForModel_FindsView_WhenMappingExists()
    {
        var config = new ViewModelLocatorConfiguration()
            .AddMapping<SampleView, SampleViewModel>();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());
        var view = locator.LocateForModel(new SampleViewModel(), null);
        await Assert.That(view.GetType()).IsEqualTo(typeof(SampleView));
    }

    [Test]
    public async Task LocateForModel_FindsCorrectView_WithMultipleMappings()
    {
        var config = new ViewModelLocatorConfiguration()
            .AddMapping<SampleView, SampleViewModel>()
            .AddMapping<AnotherView, AnotherViewModel>();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());
        var view1 = locator.LocateForModel(new SampleViewModel(), null);
        var view2 = locator.LocateForModel(new AnotherViewModel(), null);
        await Assert.That(view1.GetType()).IsEqualTo(typeof(SampleView));
        await Assert.That(view2.GetType()).IsEqualTo(typeof(AnotherView));
    }
}
