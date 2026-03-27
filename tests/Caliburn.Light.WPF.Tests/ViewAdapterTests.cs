using System.Windows;
using System.Windows.Controls;
using TUnit.Core;
using TUnit.Core.Executors;

namespace Caliburn.Light.WPF.Tests;

[TestExecutor<WpfTestExecutor>]
[NotInParallel("ViewHelper")]
public class ViewAdapterTests
{
    [Test]
    public async Task ViewHelper_IsInitialized_ReturnsTrue()
    {
        // ViewAdapter.Initialize is called via [ModuleInitializer]
        await Assert.That(ViewHelper.IsInitialized).IsTrue();
    }

    [Test]
    public async Task IsInDesignTool_ReturnsFalse_InTestContext()
    {
        await Assert.That(ViewHelper.IsInDesignTool).IsFalse();
    }

    [Test]
    public async Task GetFirstNonGeneratedView_ReturnsContent_WhenContentControlIsGenerated()
    {
        var inner = new TextBlock { Text = "Inner" };
        var cc = new ContentControl { Content = inner };
        View.SetIsGenerated(cc, true);

        var result = ViewHelper.GetFirstNonGeneratedView(cc);
        await Assert.That(ReferenceEquals(result, inner)).IsTrue();
    }

    [Test]
    public async Task GetFirstNonGeneratedView_ReturnsSelf_WhenNotGenerated()
    {
        var cc = new ContentControl();
        var result = ViewHelper.GetFirstNonGeneratedView(cc);
        await Assert.That(ReferenceEquals(result, cc)).IsTrue();
    }

    [Test]
    public async Task GetCommandParameter_ReturnsAttachedPropertyValue()
    {
        var button = new Button();
        View.SetCommandParameter(button, "test-param");

        var result = ViewHelper.GetCommandParameter(button);
        await Assert.That(result).IsEqualTo("test-param");
    }

    [Test]
    public async Task GetCommandParameter_ReturnsNull_WhenNotSet()
    {
        var button = new Button();
        var result = ViewHelper.GetCommandParameter(button);
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task GetCommandParameter_ReturnsButtonCommandParameter_WhenAttachedNotSet()
    {
        var button = new Button { CommandParameter = "native-param" };
        var result = ViewHelper.GetCommandParameter(button);
        await Assert.That(result).IsEqualTo("native-param");
    }

    [Test]
    public async Task GetDispatcher_ReturnsIDispatcher_WithCheckAccess()
    {
        var button = new Button();
        var dispatcher = ViewHelper.GetDispatcher(button);

        await Assert.That(dispatcher).IsNotNull();
        await Assert.That(dispatcher.CheckAccess()).IsTrue();
    }

    [Test]
    public async Task GetDispatcher_ReturnsSameDispatcher_ForSameThread()
    {
        var button1 = new Button();
        var button2 = new Button();

        var d1 = ViewHelper.GetDispatcher(button1);
        var d2 = ViewHelper.GetDispatcher(button2);

        await Assert.That(d1.Equals(d2)).IsTrue();
    }

    [Test]
    public async Task TryCloseAsync_Window_ReturnsTrue()
    {
        var window = new Window();
        var result = await ViewHelper.TryCloseAsync(window);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task TryCloseAsync_NonWindowElement_ReturnsFalse()
    {
        var button = new Button();
        var result = await ViewHelper.TryCloseAsync(button);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task ModuleInitializer_RegistersWpfAdapter()
    {
        // ViewHelper.IsInitialized proves the [ModuleInitializer] ran
        await Assert.That(ViewHelper.IsInitialized).IsTrue();

        // Verify the adapter handles WPF types correctly
        var button = new Button();
        var dispatcher = ViewHelper.GetDispatcher(button);
        await Assert.That(dispatcher).IsNotNull();
        await Assert.That(dispatcher.CheckAccess()).IsTrue();
    }

    [Test]
    public async Task Initialize_CalledTwice_DoesNotCorruptState()
    {
        InvokeViewAdapterInitialize();

        await Assert.That(ViewHelper.IsInitialized).IsTrue();

        var button = new Button();
        var dispatcher = ViewHelper.GetDispatcher(button);
        await Assert.That(dispatcher).IsNotNull();
        await Assert.That(dispatcher.CheckAccess()).IsTrue();
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
            await Assert.That(dispatcher).IsNotNull();
        }
        finally
        {
            if (!ViewHelper.IsInitialized)
                InvokeViewAdapterInitialize();
        }
    }

    private static void InvokeViewAdapterInitialize()
    {
        var viewAdapterType = typeof(View).Assembly.GetType("Caliburn.Light.WPF.ViewAdapter")!;
        viewAdapterType.GetMethod("Initialize", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!
            .Invoke(null, null);
    }
}
