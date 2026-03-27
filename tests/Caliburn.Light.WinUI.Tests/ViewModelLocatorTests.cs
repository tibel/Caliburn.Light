using Microsoft.UI.Xaml.Controls;
using TUnit.Core.Executors;

namespace Caliburn.Light.WinUI.Tests;

[TestExecutor<WinUITestExecutor>]
public class ViewModelLocatorTests
{
    [Test]
    public async Task LocateForModel_FindsView_WhenMappingExists()
    {
        var config = new ViewModelLocatorConfiguration()
            .AddMapping<Page, SampleViewModel>();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());

        var view = locator.LocateForModel(new SampleViewModel(), null);
        await Assert.That(view is Page).IsTrue();
    }

    [Test]
    public async Task LocateForModel_FindsCorrectView_WithMultipleMappings()
    {
        var config = new ViewModelLocatorConfiguration()
            .AddMapping<Page, SampleViewModel>()
            .AddMapping<UserControl, AnotherViewModel>();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());

        var view1 = locator.LocateForModel(new SampleViewModel(), null);
        var view2 = locator.LocateForModel(new AnotherViewModel(), null);

        await Assert.That(view1 is Page).IsTrue();
        await Assert.That(view2 is UserControl).IsTrue();
    }

    [Test]
    public async Task LocateForModel_WithContext_FindsCorrectView()
    {
        var config = new ViewModelLocatorConfiguration()
            .AddMapping<Page, SampleViewModel>("detail");
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());

        var view = locator.LocateForModel(new SampleViewModel(), "detail");
        await Assert.That(view is Page).IsTrue();
    }

    [Test]
    public async Task LocateForModel_WrongContext_ReturnsFallbackTextBlock()
    {
        var config = new ViewModelLocatorConfiguration()
            .AddMapping<Page, SampleViewModel>("detail");
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());

        var view = locator.LocateForModel(new SampleViewModel(), null);
        await Assert.That(view is TextBlock).IsTrue();
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
