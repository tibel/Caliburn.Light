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
    public async Task GetDispatcher_ReturnsIDispatcher()
    {
        var tb = new TextBlock();
        await Assert.That(_adapter.GetDispatcher(tb)).IsNotNull();
    }

    [Test]
    public async Task GetDispatcher_ReturnsSameDispatcher_ForSameThread()
    {
        var tb1 = new TextBlock();
        var tb2 = new TextBlock();
        var d1 = _adapter.GetDispatcher(tb1);
        var d2 = _adapter.GetDispatcher(tb2);
        await Assert.That(d1.Equals(d2)).IsTrue();
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
