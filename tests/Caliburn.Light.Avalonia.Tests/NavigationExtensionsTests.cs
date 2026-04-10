using Avalonia.Controls;
using TUnit.Core.Executors;

namespace Caliburn.Light.Avalonia.Tests;

[TestExecutor<AvaloniaTestExecutor>]
public sealed class NavigationExtensionsTests
{
    [Test]
    public async Task PushAsync_Generic_CreatesAndPushesPage()
    {
        var navigationPage = new NavigationPage();
        using var lifecycle = new PageLifecycle(navigationPage, null, new StubLocator());

        await navigationPage.PushAsync<ContentPage>();

        var stack = navigationPage.NavigationStack;
        await Assert.That(stack).Count().IsEqualTo(1);
        await Assert.That(stack[0]).IsTypeOf<ContentPage>();
    }

    [Test]
    public async Task PushAsync_Generic_ActivatesViewModel()
    {
        var screen = new TestScreen();
        var navigationPage = new NavigationPage();
        using var lifecycle = new PageLifecycle(navigationPage, null, new StubLocator(screen));

        await navigationPage.PushAsync<ContentPage>();

        await Assert.That(screen.IsActive).IsTrue();
    }

    [Test]
    public async Task ReplaceAsync_Generic_CreatesAndReplacesPage()
    {
        var navigationPage = new NavigationPage();
        using var lifecycle = new PageLifecycle(navigationPage, null, new StubLocator());

        var originalPage = new ContentPage();
        await navigationPage.PushAsync(originalPage);

        await navigationPage.ReplaceAsync<ContentPage>();

        var stack = navigationPage.NavigationStack;
        await Assert.That(stack).Count().IsEqualTo(1);
        await Assert.That(ReferenceEquals(stack[0], originalPage)).IsFalse();
        await Assert.That(stack[0]).IsTypeOf<ContentPage>();
    }

    [Test]
    public async Task ReplaceAsync_Generic_ActivatesNewViewModel()
    {
        var screen1 = new TestScreen();
        var screen2 = new TestScreen();
        var locator = new SequentialLocator([screen1, screen2]);
        var navigationPage = new NavigationPage();
        using var lifecycle = new PageLifecycle(navigationPage, null, locator);

        await navigationPage.PushAsync<ContentPage>();
        await Assert.That(screen1.IsActive).IsTrue();

        await navigationPage.ReplaceAsync<ContentPage>();
        await Assert.That(screen2.IsActive).IsTrue();
        await Assert.That(screen1.IsActive).IsFalse();
    }

    private sealed class TestScreen : Screen { }

    private sealed class StubLocator : IViewModelLocator
    {
        private readonly object? _viewModel;
        public StubLocator(object? viewModel = null) => _viewModel = viewModel;
        public Control LocateForModel(object model, string? context) => new TextBlock();
        public object? LocateForView(Control view) => _viewModel;
    }

    private sealed class SequentialLocator : IViewModelLocator
    {
        private readonly object?[] _viewModels;
        private int _index;
        public SequentialLocator(object?[] viewModels) => _viewModels = viewModels;
        public Control LocateForModel(object model, string? context) => new TextBlock();
        public object? LocateForView(Control view) => _index < _viewModels.Length ? _viewModels[_index++] : null;
    }
}
