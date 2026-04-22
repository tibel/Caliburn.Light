using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TUnit.Core;
using TUnit.Core.Executors;

namespace Caliburn.Light.WinUI.Tests;

[TestExecutor<WinUITestExecutor>]
[NotInParallel("ViewHelper")]
public class ViewAdapterTests
{
    [Test]
    public async Task ViewHelper_IsInitialized_ReturnsTrue()
    {
        // ViewAdapter registers itself via [ModuleInitializer]
        await Assert.That(ViewHelper.IsInitialized).IsTrue();
    }

    [Test]
    public async Task IsInDesignTool_ReturnsFalse_InTestContext()
    {
        await Assert.That(ViewHelper.IsInDesignTool).IsFalse();
    }

    [Test]
    public async Task GetFirstNonGeneratedView_ReturnsSelf_WhenNotGenerated()
    {
        var cc = new ContentControl();
        var result = ViewHelper.GetFirstNonGeneratedView(cc);
        await Assert.That(ReferenceEquals(cc, result)).IsTrue();
    }

    [Test]
    public async Task GetFirstNonGeneratedView_ReturnsContent_WhenGenerated()
    {
        var inner = new TextBlock { Text = "inner" };
        var cc = new ContentControl { Content = inner };
        View.SetIsGenerated(cc, true);
        var result = ViewHelper.GetFirstNonGeneratedView(cc);
        await Assert.That(ReferenceEquals(inner, result)).IsTrue();
    }

    [Test]
    public async Task GetCommandParameter_ReturnsAttachedPropertyValue()
    {
        var tb = new TextBlock();
        View.SetCommandParameter(tb, "attached-value");
        await Assert.That(ViewHelper.GetCommandParameter(tb)).IsEqualTo("attached-value");
    }

    [Test]
    public async Task GetCommandParameter_ReturnsNull_WhenNotSet()
    {
        var tb = new TextBlock();
        await Assert.That(ViewHelper.GetCommandParameter(tb)).IsNull();
    }

    [Test]
    public async Task GetCommandParameter_ReturnsButtonCommandParameter_WhenAttachedNotSet()
    {
        var btn = new Button();
        btn.CommandParameter = "button-param";
        await Assert.That(ViewHelper.GetCommandParameter(btn)).IsEqualTo("button-param");
    }

    [Test]
    public async Task GetDispatcher_ReturnsIDispatcher()
    {
        var tb = new TextBlock();
        await Assert.That(ViewHelper.GetDispatcher(tb)).IsNotNull();
    }

    [Test]
    public async Task GetDispatcher_ReturnsSameDispatcher_ForSameThread()
    {
        var tb1 = new TextBlock();
        var tb2 = new TextBlock();
        var d1 = ViewHelper.GetDispatcher(tb1);
        var d2 = ViewHelper.GetDispatcher(tb2);
        await Assert.That(d1.Equals(d2)).IsTrue();
    }

    [Test]
    public async Task TryCloseAsync_NonWindowControl_ReturnsFalse()
    {
        var tb = new TextBlock();
        var result = ViewHelper.TryCloseAsync(tb).GetAwaiter().GetResult();
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task TryCloseAsync_Popup_ReturnsTrue()
    {
        var popup = new Microsoft.UI.Xaml.Controls.Primitives.Popup { IsOpen = true };
        var result = ViewHelper.TryCloseAsync(popup).GetAwaiter().GetResult();
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task ModuleInitializer_RegistersWinUIAdapter()
    {
        await Assert.That(ViewHelper.IsInitialized).IsTrue();

        var button = new Button();
        var dispatcher = ViewHelper.GetDispatcher(button);
        await Assert.That(dispatcher is not null && dispatcher.CheckAccess()).IsTrue();
    }

    [Test]
    public async Task Initialize_CalledTwice_DoesNotCorruptState()
    {
        InvokeViewAdapterInitialize();

        await Assert.That(ViewHelper.IsInitialized).IsTrue();

        var button = new Button();
        var dispatcher = ViewHelper.GetDispatcher(button);
        await Assert.That(dispatcher is not null && dispatcher.CheckAccess()).IsTrue();
    }

    [Test]
    public async Task Reset_ThenReinitialize_WorksCorrectly()
    {
        ViewHelper.Reset();
        try
        {
            await Assert.That(ViewHelper.IsInitialized).IsFalse();

            InvokeViewAdapterInitialize();
            await Assert.That(ViewHelper.IsInitialized).IsTrue();

            var button = new Button();
            var dispatcher = ViewHelper.GetDispatcher(button);
            await Assert.That(dispatcher is not null && dispatcher.CheckAccess()).IsTrue();
        }
        finally
        {
            if (!ViewHelper.IsInitialized)
                InvokeViewAdapterInitialize();
        }
    }

    private static void InvokeViewAdapterInitialize()
    {
        var viewAdapterType = typeof(View).Assembly.GetType("Caliburn.Light.WinUI.ViewAdapter")!;
        viewAdapterType.GetMethod("Initialize", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!
            .Invoke(null, null);
    }
}
