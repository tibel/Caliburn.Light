using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TUnit.Core;
using TUnit.Core.Executors;

namespace Caliburn.Light.WinUI.Tests;

/// <summary>
/// Tests the internal ViewAdapter directly, reachable via InternalsVisibleTo.
/// Each test constructs its own adapter, so nothing here touches the
/// ViewHelper static registry.
/// </summary>
[TestExecutor<WinUITestExecutor>]
public class ViewAdapterTests
{
    private readonly ViewAdapter _adapter = new();

    [Test]
    public async Task IsInDesignTool_ReturnsFalse_InTestContext()
    {
        await Assert.That(_adapter.IsInDesignTool).IsFalse();
    }

    [Test]
    public async Task CanHandle_WinUIElement_ReturnsTrue()
    {
        await Assert.That(_adapter.CanHandle(new TextBlock())).IsTrue();
    }

    [Test]
    public async Task GetFirstNonGeneratedView_ReturnsSelf_WhenNotGenerated()
    {
        var cc = new ContentControl();
        var result = _adapter.GetFirstNonGeneratedView(cc);
        await Assert.That(ReferenceEquals(cc, result)).IsTrue();
    }

    [Test]
    public async Task GetFirstNonGeneratedView_ReturnsContent_WhenGenerated()
    {
        var inner = new TextBlock { Text = "inner" };
        var cc = new ContentControl { Content = inner };
        View.SetIsGenerated(cc, true);
        var result = _adapter.GetFirstNonGeneratedView(cc);
        await Assert.That(ReferenceEquals(inner, result)).IsTrue();
    }

    [Test]
    public async Task GetCommandParameter_ReturnsAttachedPropertyValue()
    {
        var tb = new TextBlock();
        View.SetCommandParameter(tb, "attached-value");
        await Assert.That(_adapter.GetCommandParameter(tb)).IsEqualTo("attached-value");
    }

    [Test]
    public async Task GetCommandParameter_ReturnsNull_WhenNotSet()
    {
        var tb = new TextBlock();
        await Assert.That(_adapter.GetCommandParameter(tb)).IsNull();
    }

    [Test]
    public async Task GetCommandParameter_ReturnsButtonCommandParameter_WhenAttachedNotSet()
    {
        var btn = new Button();
        btn.CommandParameter = "button-param";
        await Assert.That(_adapter.GetCommandParameter(btn)).IsEqualTo("button-param");
    }

    [Test]
    public async Task CheckAccess_ReturnsTrue_OnDispatcherThread()
    {
        await Assert.That(_adapter.CheckAccess(new TextBlock())).IsTrue();
    }

    [Test]
    public async Task BeginInvoke_ExecutesOnDispatcherThread()
    {
        var view = new TextBlock();
        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _adapter.BeginInvoke(view, () => completion.SetResult(_adapter.CheckAccess(view)));

        await Assert.That(await completion.Task.WaitAsync(TimeSpan.FromSeconds(5))).IsTrue();
    }

    [Test]
    public async Task TryCloseAsync_NonWindowControl_ReturnsFalse()
    {
        var tb = new TextBlock();
        var result = await _adapter.TryCloseAsync(tb);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task TryCloseAsync_Popup_ReturnsTrue()
    {
        var popup = new Microsoft.UI.Xaml.Controls.Primitives.Popup { IsOpen = true };
        var result = await _adapter.TryCloseAsync(popup);
        await Assert.That(result).IsTrue();
    }
}
