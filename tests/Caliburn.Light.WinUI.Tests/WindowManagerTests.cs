using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TUnit.Core.Executors;

namespace Caliburn.Light.WinUI.Tests;

internal sealed class TestableWindowManager : WindowManager
{
    public TestableWindowManager(IViewModelLocator locator) : base(locator) { }

    public Window CallEnsureWindow(object viewModel, UIElement view)
        => EnsureWindow(viewModel, view);

    public static Window? TestGetWindow(object? viewModel) => GetWindow(viewModel);
}

[TestExecutor<WinUITestExecutor>]
public class WindowManagerTests
{
    [Test]
    public async Task Constructor_WithValidLocator_DoesNotThrow()
    {
        var config = new ViewModelLocatorConfiguration()
            .AddMapping<Page, SampleViewModel>();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());
        var wm = new TestableWindowManager(locator);
        await Assert.That(wm).IsNotNull();
    }

    [Test]
    public async Task Constructor_NullLocator_Throws()
    {
        await Assert.That(() => new TestableWindowManager(null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShowWindow_NullViewModel_Throws()
    {
        var config = new ViewModelLocatorConfiguration();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());
        var wm = new TestableWindowManager(locator);

        await Assert.That(() => wm.ShowWindow(null!, null))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task EnsureWindow_SetsContentToView()
    {
        var config = new ViewModelLocatorConfiguration()
            .AddMapping<Page, SampleViewModel>();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());
        var wm = new TestableWindowManager(locator);

        var textBlock = new TextBlock { Text = "test content" };
        var window = wm.CallEnsureWindow(new SampleViewModel(), textBlock);
        await Assert.That(ReferenceEquals(window.Content, textBlock)).IsTrue();
    }

    [Test]
    public async Task EnsureWindow_SetsWindowOnView()
    {
        var config = new ViewModelLocatorConfiguration()
            .AddMapping<Page, SampleViewModel>();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());
        var wm = new TestableWindowManager(locator);

        var textBlock = new TextBlock();
        var window = wm.CallEnsureWindow(new SampleViewModel(), textBlock);
        var attached = View.GetWindow(textBlock);
        await Assert.That(ReferenceEquals(attached, window)).IsTrue();
    }

    [Test]
    public async Task GetWindow_ReturnsNull_WhenViewModelIsNull()
    {
        var result = TestableWindowManager.TestGetWindow(null);
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task GetWindow_ReturnsNull_WhenViewModelHasNoViews()
    {
        var result = TestableWindowManager.TestGetWindow(new Screen());
        await Assert.That(result).IsNull();
    }
}
