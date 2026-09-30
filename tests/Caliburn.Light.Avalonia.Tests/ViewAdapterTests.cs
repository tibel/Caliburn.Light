using Avalonia.Controls;
using TUnit.Core;
using TUnit.Core.Executors;

namespace Caliburn.Light.Avalonia.Tests;

/// <summary>
/// Tests the internal ViewAdapter directly, reachable via InternalsVisibleTo.
/// Each test constructs its own adapter, so nothing here touches the
/// ViewHelper static registry.
/// </summary>
[TestExecutor<AvaloniaTestExecutor>]
public class ViewAdapterTests
{
    private readonly ViewAdapter _adapter = new();

    [Test]
    public async Task IsInDesignTool_ReturnsFalse_InTestContext()
    {
        await Assert.That(_adapter.IsInDesignTool).IsFalse();
    }

    [Test]
    public async Task CanHandle_AvaloniaObject_ReturnsTrue()
    {
        await Assert.That(_adapter.CanHandle(new TextBlock())).IsTrue();
    }

    [Test]
    public async Task GetFirstNonGeneratedView_NotGenerated_ReturnsSameView()
    {
        var control = new TextBlock();
        var result = _adapter.GetFirstNonGeneratedView(control);
        var areSame = ReferenceEquals(control, result);

        await Assert.That(areSame).IsTrue();
    }

    [Test]
    public async Task GetFirstNonGeneratedView_GeneratedContentControl_ReturnsContent()
    {
        var inner = new TextBlock { Text = "inner" };
        var outer = new ContentControl { Content = inner };
        View.SetIsGenerated(outer, true);

        var result = _adapter.GetFirstNonGeneratedView(outer);
        var areSame = ReferenceEquals(inner, result);

        await Assert.That(areSame).IsTrue();
    }

    [Test]
    public async Task GetCommandParameter_ReadsAttachedProperty()
    {
        var control = new TextBlock();
        View.SetCommandParameter(control, "test-param");
        var result = _adapter.GetCommandParameter(control);

        await Assert.That(result).IsEqualTo("test-param");
    }

    [Test]
    public async Task GetCommandParameter_NoParam_ReturnsNull()
    {
        var control = new TextBlock();
        var result = _adapter.GetCommandParameter(control);

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
    public async Task GetDispatcher_ReturnsIDispatcher()
    {
        var control = new TextBlock();
        var result = _adapter.GetDispatcher(control);

        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsAssignableTo<IDispatcher>();
    }

    [Test]
    public async Task GetDispatcher_ReturnsSameDispatcher_ForSameThread()
    {
        var control1 = new TextBlock();
        var control2 = new TextBlock();
        var d1 = _adapter.GetDispatcher(control1);
        var d2 = _adapter.GetDispatcher(control2);
        var areEqual = d1.Equals(d2);

        await Assert.That(areEqual).IsTrue();
    }

    [Test]
    public async Task TryCloseAsync_Window_ReturnsTrue()
    {
        var window = new Window();
        window.Show();
        var result = await _adapter.TryCloseAsync(window);
        window.Close();

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task TryCloseAsync_NonWindowControl_ReturnsFalse()
    {
        var control = new TextBlock();
        var result = await _adapter.TryCloseAsync(control);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task ExecuteOnLayoutUpdated_WiresHandler_DoesNotThrow()
    {
        var action = () =>
        {
            var tb = new TextBlock();
            _adapter.ExecuteOnLayoutUpdated(tb, _ => { });
        };

        await Assert.That(action).ThrowsNothing();
    }
}
