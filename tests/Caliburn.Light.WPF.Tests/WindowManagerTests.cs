using System.Windows;
using TUnit.Core.Executors;

namespace Caliburn.Light.WPF.Tests;

internal class TestableWindowManager : WindowManager
{
    public TestableWindowManager(IViewModelLocator locator) : base(locator) { }

    public static Window? TestGetWindow(object? viewModel) => GetWindow(viewModel);

    public Window CallEnsureWindow(object viewModel, System.Windows.UIElement view) => EnsureWindow(viewModel, view);
}

[TestExecutor<WpfTestExecutor>]
public class WindowManagerTests
{
    [Test]
    public async Task Constructor_WithValidLocator_Succeeds()
    {
        var config = new ViewModelLocatorConfiguration();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());

        var manager = new WindowManager(locator);
        await Assert.That(manager).IsNotNull();
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
        var screen = new Screen();
        var result = TestableWindowManager.TestGetWindow(screen);
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task GetWindow_ReturnsNull_WhenViewIsNotInWindow()
    {
        var screen = new Screen();
        // Attach a non-window view (a button not in any window)
        var button = new System.Windows.Controls.Button();
        ((IViewAware)screen).AttachView(button, null);

        var result = TestableWindowManager.TestGetWindow(screen);
        // Window.GetWindow returns null for elements not in a window's visual tree
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task GetWindow_TraversesParent_ViaIChild()
    {
        // Create a parent screen with a view and a child without views
        var parent = new Screen();
        var child = new ChildScreen { Parent = parent };

        // Parent has no views either, so GetWindow returns null
        // but it should traverse without throwing
        var result = TestableWindowManager.TestGetWindow(child);
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task Constructor_NullLocator_Throws()
    {
        await Assert.That(() => new WindowManager(null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShowWindow_NullViewModel_Throws()
    {
        var config = new ViewModelLocatorConfiguration();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());
        var manager = new WindowManager(locator);

        await Assert.That(() => manager.ShowWindow(null!, null))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task EnsureWindow_NonWindowView_WrapsInWindow()
    {
        var config = new ViewModelLocatorConfiguration();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());
        var manager = new TestableWindowManager(locator);

        var button = new System.Windows.Controls.Button();
        var window = manager.CallEnsureWindow(new TestViewModel1(), button);

        await Assert.That(window is Window).IsTrue();
        await Assert.That(window.Content).IsEqualTo(button);
        await Assert.That(View.GetIsGenerated(window)).IsTrue();
    }

    [Test]
    public async Task EnsureWindow_WindowView_ReturnsSameWindow()
    {
        var config = new ViewModelLocatorConfiguration();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());
        var manager = new TestableWindowManager(locator);

        var original = new Window();
        var result = manager.CallEnsureWindow(new TestViewModel1(), original);

        await Assert.That(ReferenceEquals(result, original)).IsTrue();
    }

    [Test]
    public async Task EnsureWindow_NonWindow_SetsIsGenerated()
    {
        var config = new ViewModelLocatorConfiguration();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());
        var manager = new TestableWindowManager(locator);

        var button = new System.Windows.Controls.Button();
        var window = manager.CallEnsureWindow(new TestViewModel1(), button);

        await Assert.That(View.GetIsGenerated(window)).IsTrue();
    }

    [Test]
    public async Task EnsureWindow_Window_IsGeneratedFalse()
    {
        var config = new ViewModelLocatorConfiguration();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());
        var manager = new TestableWindowManager(locator);

        var original = new Window();
        var result = manager.CallEnsureWindow(new TestViewModel1(), original);

        await Assert.That(View.GetIsGenerated(result)).IsFalse();
    }

    // NOTE: Testing ShowWindow/ShowDialog requires showing actual windows, which is
    // not feasible in a headless CI test environment. Those would need integration tests.
}

internal class ChildScreen : Screen, IChild
{
    public object? Parent { get; set; }
}
