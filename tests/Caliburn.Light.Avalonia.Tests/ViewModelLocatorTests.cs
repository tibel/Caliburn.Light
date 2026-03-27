using Avalonia.Controls;
using TUnit.Core.Executors;

namespace Caliburn.Light.Avalonia.Tests;

[TestExecutor<AvaloniaTestExecutor>]
public class ViewModelLocatorTests
{
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
        var config = new ViewModelLocatorConfiguration();
        config.AddMapping<TextBlock, SampleViewModel>();

        var locator = new ViewModelLocator(config, new SimpleServiceProvider());
        var view = locator.LocateForModel(new SampleViewModel(), null);

        await Assert.That(view).IsTypeOf<TextBlock>();
    }

    [Test]
    public async Task LocateForModel_FindsCorrectView_WithMultipleMappings()
    {
        var config = new ViewModelLocatorConfiguration();
        config.AddMapping<TextBlock, SampleViewModel>()
              .AddMapping<Button, AnotherViewModel>();

        var locator = new ViewModelLocator(config, new SimpleServiceProvider());
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

        var locator = new ViewModelLocator(config, new SimpleServiceProvider());
        var view = locator.LocateForModel(new SampleViewModel(), "detail");

        await Assert.That(view).IsTypeOf<Button>();
    }

    [Test]
    public async Task LocateForModel_WrongContext_ReturnsFallbackTextBlock()
    {
        var config = new ViewModelLocatorConfiguration();
        config.AddMapping<Button, SampleViewModel>("detail");

        var locator = new ViewModelLocator(config, new SimpleServiceProvider());
        var view = locator.LocateForModel(new SampleViewModel(), null);

        await Assert.That(view).IsTypeOf<TextBlock>();
        var tb = (TextBlock)view;
        await Assert.That(tb.Text).Contains("Cannot find view");
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
        var view = new TextBlock { DataContext = vm };
        var result = locator.LocateForView(view);

        await Assert.That(result).IsSameReferenceAs(vm);
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
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());

        var view = new TextBlock();
        var result = locator.LocateForView(view);

        await Assert.That(result).IsNull();
    }
}
