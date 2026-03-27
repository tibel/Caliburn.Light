using Avalonia.Controls;
using TUnit.Core;
using TUnit.Core.Executors;

namespace Caliburn.Light.Avalonia.Tests;

/// <summary>
/// Tests the internal ViewAdapter through the public ViewHelper static API.
/// The ViewAdapter is registered via [ModuleInitializer] when the assembly loads.
/// </summary>
[TestExecutor<AvaloniaTestExecutor>]
public class ViewAdapterTests
{
    [Test]
    public async Task ViewHelper_IsInitialized_AfterSetup_ReturnsTrue()
    {
        // The executor starts the Avalonia app; the [ModuleInitializer] on
        // ViewAdapter registers the adapter with ViewHelper.
        // Under coverage instrumentation, module init order may vary,
        // so we explicitly re-initialize if needed.
        if (!ViewHelper.IsInitialized)
            InvokeViewAdapterInitialize();

        await Assert.That(ViewHelper.IsInitialized).IsTrue();
    }

    [Test]
    public async Task GetFirstNonGeneratedView_NotGenerated_ReturnsSameView()
    {
        var control = new TextBlock();
        var result = ViewHelper.GetFirstNonGeneratedView(control);
        var areSame = ReferenceEquals(control, result);

        await Assert.That(areSame).IsTrue();
    }

    [Test]
    public async Task GetFirstNonGeneratedView_GeneratedContentControl_ReturnsContent()
    {
        var inner = new TextBlock { Text = "inner" };
        var outer = new ContentControl { Content = inner };
        View.SetIsGenerated(outer, true);

        var result = ViewHelper.GetFirstNonGeneratedView(outer);
        var areSame = ReferenceEquals(inner, result);

        await Assert.That(areSame).IsTrue();
    }

    [Test]
    public async Task GetCommandParameter_ReadsAttachedProperty()
    {
        var control = new TextBlock();
        View.SetCommandParameter(control, "test-param");
        var result = ViewHelper.GetCommandParameter(control);

        await Assert.That(result).IsEqualTo("test-param");
    }

    [Test]
    public async Task GetCommandParameter_NoParam_ReturnsNull()
    {
        var control = new TextBlock();
        var result = ViewHelper.GetCommandParameter(control);

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task GetDispatcher_ReturnsIDispatcher()
    {
        var control = new TextBlock();
        var result = ViewHelper.GetDispatcher(control);

        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsAssignableTo<IDispatcher>();
    }

    [Test]
    public async Task GetCommandParameter_ReturnsButtonCommandParameter_WhenAttachedNotSet()
    {
        var button = new Button { CommandParameter = "native-param" };
        var result = ViewHelper.GetCommandParameter(button);

        await Assert.That(result).IsEqualTo("native-param");
    }

    [Test]
    public async Task GetDispatcher_ReturnsSameDispatcher_ForSameThread()
    {
        var control1 = new TextBlock();
        var control2 = new TextBlock();
        var d1 = ViewHelper.GetDispatcher(control1);
        var d2 = ViewHelper.GetDispatcher(control2);
        var areEqual = d1.Equals(d2);

        await Assert.That(areEqual).IsTrue();
    }

    [Test]
    public async Task TryCloseAsync_Window_ReturnsTrue()
    {
        var window = new Window();
        window.Show();
        var result = await ViewHelper.TryCloseAsync(window);

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task TryCloseAsync_NonWindowControl_ReturnsFalse()
    {
        // ViewHelper.TryCloseAsync for a non-Window, non-Popup AvaloniaObject
        // The adapter returns FalseTask
        var control = new TextBlock();
        var result = await ViewHelper.TryCloseAsync(control);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task ModuleInitializer_RegistersAvaloniaAdapter()
    {
        await Assert.That(ViewHelper.IsInitialized).IsTrue();

        var button = new Button();
        var dispatcher = ViewHelper.GetDispatcher(button);
        var canHandle = dispatcher is not null && dispatcher.CheckAccess();

        await Assert.That(canHandle).IsTrue();
    }

    [Test]
    public async Task Initialize_CalledTwice_DoesNotCorruptState()
    {
        InvokeViewAdapterInitialize();

        await Assert.That(ViewHelper.IsInitialized).IsTrue();

        var button = new Button();
        var dispatcher = ViewHelper.GetDispatcher(button);
        var canHandle = dispatcher is not null && dispatcher.CheckAccess();

        await Assert.That(canHandle).IsTrue();
    }

    [Test, NotInParallel]
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
            var canHandle = dispatcher is not null && dispatcher.CheckAccess();

            await Assert.That(canHandle).IsTrue();
        }
        finally
        {
            if (!ViewHelper.IsInitialized)
                InvokeViewAdapterInitialize();
        }
    }

    [Test]
    public async Task ExecuteOnLayoutUpdated_WiresHandler_DoesNotThrow()
    {
        var action = () =>
        {
            var tb = new TextBlock();
            ViewHelper.ExecuteOnLayoutUpdated(tb, _ => { });
        };

        await Assert.That(action).ThrowsNothing();
    }

    private static void InvokeViewAdapterInitialize()
    {
        var viewAdapterType = typeof(View).Assembly.GetType("Caliburn.Light.Avalonia.ViewAdapter")!;
        viewAdapterType.GetMethod("Initialize", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!
            .Invoke(null, null);
    }
}
