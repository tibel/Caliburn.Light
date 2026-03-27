using Avalonia.Controls;
using TUnit.Core;
using TUnit.Core.Executors;

namespace Caliburn.Light.Avalonia.Tests;

[TestExecutor<AvaloniaTestExecutor>]
public class WindowManagerTests
{
    [Test]
    public async Task Constructor_WithValidLocator_DoesNotThrow()
    {
        var config = new ViewModelLocatorConfiguration();
        var sp = new SimpleServiceProvider();
        var locator = new ViewModelLocator(config, sp);
        var manager = new WindowManager(locator);

        await Assert.That(manager).IsNotNull();
    }

    [Test]
    public async Task Constructor_NullLocator_Throws()
    {
        await Assert.That(() => new WindowManager(null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task EnsureWindow_NonWindowView_WrapsInWindow()
    {
        var config = new ViewModelLocatorConfiguration();
        config.AddMapping<TextBlock, SampleViewModel>();
        var sp = new SimpleServiceProvider();
        var locator = new ViewModelLocator(config, sp);
        var manager = new TestableWindowManager(locator);

        var textBlock = new TextBlock();
        var window = manager.CallEnsureWindow(new SampleViewModel(), textBlock);

        var isWindow = window is Window;
        var contentType = window.Content?.GetType().Name ?? "null";
        var isGenerated = View.GetIsGenerated(window);

        await Assert.That(isWindow).IsTrue();
        await Assert.That(contentType).IsEqualTo("TextBlock");
        await Assert.That(isGenerated).IsTrue();
    }

    [Test]
    public async Task EnsureWindow_WindowView_ReturnsSameWindow()
    {
        var config = new ViewModelLocatorConfiguration();
        var sp = new SimpleServiceProvider();
        var locator = new ViewModelLocator(config, sp);
        var manager = new TestableWindowManager(locator);

        var window = new Window();
        var result = manager.CallEnsureWindow(new SampleViewModel(), window);
        var areSame = ReferenceEquals(window, result);

        await Assert.That(areSame).IsTrue();
    }

    [Test]
    public async Task EnsureWindow_NonWindow_SetsIsGenerated()
    {
        var config = new ViewModelLocatorConfiguration();
        var sp = new SimpleServiceProvider();
        var locator = new ViewModelLocator(config, sp);
        var manager = new TestableWindowManager(locator);

        var textBlock = new TextBlock();
        var window = manager.CallEnsureWindow(new SampleViewModel(), textBlock);
        var isGenerated = View.GetIsGenerated(window);

        await Assert.That(isGenerated).IsTrue();
    }

    [Test]
    public async Task EnsureWindow_Window_IsGeneratedFalse()
    {
        var config = new ViewModelLocatorConfiguration();
        var sp = new SimpleServiceProvider();
        var locator = new ViewModelLocator(config, sp);
        var manager = new TestableWindowManager(locator);

        var window = new Window();
        var result = manager.CallEnsureWindow(new SampleViewModel(), window);
        var isGenerated = View.GetIsGenerated(result);

        await Assert.That(isGenerated).IsFalse();
    }

    [Test]
    public async Task ShowWindow_NullViewModel_Throws()
    {
        var config = new ViewModelLocatorConfiguration();
        var sp = new SimpleServiceProvider();
        var locator = new ViewModelLocator(config, sp);
        var manager = new WindowManager(locator);

        await Assert.That(() => manager.ShowWindow(null!, null))
            .Throws<ArgumentNullException>();
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
        var screen = new TestScreen();
        var result = TestableWindowManager.TestGetWindow(screen);

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task Activate_NullViewModel_Throws()
    {
        var config = new ViewModelLocatorConfiguration();
        var sp = new SimpleServiceProvider();
        var manager = new WindowManager(new ViewModelLocator(config, sp));

        await Assert.That(() => manager.Activate(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Activate_ViewModelWithNoWindow_ReturnsFalse()
    {
        var config = new ViewModelLocatorConfiguration();
        var sp = new SimpleServiceProvider();
        var manager = new WindowManager(new ViewModelLocator(config, sp));

        var result = manager.Activate(new TestScreen());
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task ShowDialog_NullOwnerViewModel_Throws()
    {
        var config = new ViewModelLocatorConfiguration();
        var sp = new SimpleServiceProvider();
        var manager = new WindowManager(new ViewModelLocator(config, sp));

        await Assert.That(() => manager.ShowDialog(new TestScreen(), null!, null)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShowDialog_NullViewModel_Throws()
    {
        var config = new ViewModelLocatorConfiguration();
        var sp = new SimpleServiceProvider();
        var manager = new WindowManager(new ViewModelLocator(config, sp));

        await Assert.That(() => manager.ShowDialog(null!, new TestScreen(), null)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShowOpenFilePickerAsync_NullOptions_Throws()
    {
        var config = new ViewModelLocatorConfiguration();
        var sp = new SimpleServiceProvider();
        var manager = new WindowManager(new ViewModelLocator(config, sp));

        Func<Task> act = () => manager.ShowOpenFilePickerAsync(null!, new TestScreen());
        await Assert.That(act).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShowSaveFilePickerAsync_NullOptions_Throws()
    {
        var config = new ViewModelLocatorConfiguration();
        var sp = new SimpleServiceProvider();
        var manager = new WindowManager(new ViewModelLocator(config, sp));

        Func<Task> act = () => manager.ShowSaveFilePickerAsync(null!, new TestScreen());
        await Assert.That(act).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShowOpenFolderPickerAsync_NullOptions_Throws()
    {
        var config = new ViewModelLocatorConfiguration();
        var sp = new SimpleServiceProvider();
        var manager = new WindowManager(new ViewModelLocator(config, sp));

        Func<Task> act = () => manager.ShowOpenFolderPickerAsync(null!, new TestScreen());
        await Assert.That(act).Throws<ArgumentNullException>();
    }
}

/// <summary>
/// Exposes protected EnsureWindow and GetWindow for testing.
/// </summary>
internal sealed class TestableWindowManager : WindowManager
{
    public TestableWindowManager(IViewModelLocator locator) : base(locator)
    {
    }

    public Window CallEnsureWindow(object viewModel, Control view)
    {
        return EnsureWindow(viewModel, view);
    }

    public static Window? TestGetWindow(object? viewModel)
    {
        return GetWindow(viewModel);
    }
}
