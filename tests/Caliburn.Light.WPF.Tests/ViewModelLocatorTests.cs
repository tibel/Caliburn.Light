using System.Windows.Controls;
using TUnit.Core.Executors;

namespace Caliburn.Light.WPF.Tests;

[TestExecutor<WpfTestExecutor>]
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
    public async Task LocateForModel_UnmappedType_ReturnsFallbackTextBlock()
    {
        var config = new ViewModelLocatorConfiguration();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());

        var view = locator.LocateForModel(new TestViewModel1(), null);

        await Assert.That(view).IsTypeOf<TextBlock>();
        var tb = (TextBlock)view;
        await Assert.That(tb.Text).Contains("Cannot find view");
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
}
