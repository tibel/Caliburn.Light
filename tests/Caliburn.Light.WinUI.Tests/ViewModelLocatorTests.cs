using Microsoft.UI.Xaml.Controls;
using TUnit.Core.Executors;

namespace Caliburn.Light.WinUI.Tests;

[TestExecutor<WinUITestExecutor>]
public class ViewModelLocatorTests
{
    [Test]
    public async Task Constructor_WithValidArgs_Succeeds()
    {
        var config = new ViewModelLocatorConfiguration()
            .AddMapping<Page, SampleViewModel>();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());
        await Assert.That(locator).IsNotNull();
    }

    [Test]
    public async Task Constructor_NullConfig_Throws()
    {
        await Assert.That(() => new ViewModelLocator(null!, new SimpleServiceProvider()))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_NullServiceProvider_Throws()
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

    [Test]
    public async Task LocateForModel_WithContext_FindsCorrectView()
    {
        var config = new ViewModelLocatorConfiguration()
            .AddMapping<SampleView, SampleViewModel>("detail");
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());

        var view = locator.LocateForModel(new SampleViewModel(), "detail");
        await Assert.That(view.GetType()).IsEqualTo(typeof(SampleView));
    }

    [Test]
    public async Task LocateForModel_WrongContext_ReturnsFallbackTextBlock()
    {
        var config = new ViewModelLocatorConfiguration()
            .AddMapping<SampleView, SampleViewModel>("detail");
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());

        var view = locator.LocateForModel(new SampleViewModel(), null);
        await Assert.That(view is TextBlock).IsTrue();
    }

    [Test]
    public async Task LocateForModel_UnmappedType_ReturnsFallbackTextBlock()
    {
        var config = new ViewModelLocatorConfiguration();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());

        var view = locator.LocateForModel(new SampleViewModel(), null);
        await Assert.That(view).IsTypeOf<TextBlock>();
        var tb = (TextBlock)view;
        await Assert.That(tb.Text).Contains("Cannot find view");
    }

    [Test]
    public async Task LocateForView_ReturnsDataContext_WhenPresent()
    {
        var config = new ViewModelLocatorConfiguration();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());

        var vm = new SampleViewModel();
        var page = new Page { DataContext = vm };
        var located = locator.LocateForView(page);
        await Assert.That(ReferenceEquals(located, vm)).IsTrue();
    }

    [Test]
    public async Task LocateForView_ReturnsNull_WhenNoMappingAndNoDataContext()
    {
        var config = new ViewModelLocatorConfiguration();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());

        var page = new Page();
        var result = locator.LocateForView(page);
        await Assert.That(result).IsNull();
    }
}
