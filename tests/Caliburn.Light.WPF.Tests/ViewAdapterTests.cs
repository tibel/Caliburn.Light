using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TUnit.Core;
using TUnit.Core.Executors;

namespace Caliburn.Light.WPF.Tests;

/// <summary>
/// Tests the internal ViewAdapter directly, reachable via InternalsVisibleTo.
/// Each test constructs its own adapter, so nothing here touches the
/// ViewHelper static registry.
/// </summary>
[TestExecutor<WpfTestExecutor>]
public class ViewAdapterTests
{
    private readonly ViewAdapter _adapter = new();

    [Test]
    public async Task IsInDesignTool_ReturnsFalse_InTestContext()
    {
        await Assert.That(_adapter.IsInDesignTool).IsFalse();
    }

    [Test]
    public async Task CanHandle_WpfElement_ReturnsTrue()
    {
        await Assert.That(_adapter.CanHandle(new Button())).IsTrue();
    }

    [Test]
    public async Task GetFirstNonGeneratedView_ReturnsContent_WhenContentControlIsGenerated()
    {
        var inner = new TextBlock { Text = "Inner" };
        var cc = new ContentControl { Content = inner };
        View.SetIsGenerated(cc, true);

        var result = _adapter.GetFirstNonGeneratedView(cc);
        await Assert.That(ReferenceEquals(result, inner)).IsTrue();
    }

    [Test]
    public async Task GetFirstNonGeneratedView_ReturnsSelf_WhenNotGenerated()
    {
        var cc = new ContentControl();
        var result = _adapter.GetFirstNonGeneratedView(cc);
        await Assert.That(ReferenceEquals(result, cc)).IsTrue();
    }

    [Test]
    public async Task GetCommandParameter_ReturnsAttachedPropertyValue()
    {
        var button = new Button();
        View.SetCommandParameter(button, "test-param");

        var result = _adapter.GetCommandParameter(button);
        await Assert.That(result).IsEqualTo("test-param");
    }

    [Test]
    public async Task GetCommandParameter_ReturnsNull_WhenNotSet()
    {
        var button = new Button();
        var result = _adapter.GetCommandParameter(button);
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task GetCommandParameter_ReturnsButtonCommandParameter_WhenAttachedNotSet()
    {
        var button = new Button { CommandParameter = "native-param" };
        var result = _adapter.GetCommandParameter(button);
        await Assert.That(result).IsEqualTo("native-param");
    }

    [Test]
    public async Task CheckAccess_ReturnsTrue_OnDispatcherThread()
    {
        await Assert.That(_adapter.CheckAccess(new Button())).IsTrue();
    }

    [Test]
    public async Task BeginInvoke_ExecutesOnDispatcherThread()
    {
        var view = new Button();
        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _adapter.BeginInvoke(view, () => completion.SetResult(_adapter.CheckAccess(view)));

        await Assert.That(await completion.Task.WaitAsync(TimeSpan.FromSeconds(5))).IsTrue();
    }

    [Test]
    public async Task TryCloseAsync_Window_ReturnsTrue()
    {
        var window = new Window();
        var result = await _adapter.TryCloseAsync(window);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task TryCloseAsync_NonWindowElement_ReturnsFalse()
    {
        var button = new Button();
        var result = await _adapter.TryCloseAsync(button);
        await Assert.That(result).IsFalse();
    }
}
