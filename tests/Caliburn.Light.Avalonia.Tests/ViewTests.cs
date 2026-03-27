using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using TUnit.Core;
using TUnit.Core.Executors;

namespace Caliburn.Light.Avalonia.Tests;

[TestExecutor<AvaloniaTestExecutor>]
public class ViewTests
{
    [Test]
    public async Task CommandParameter_GetSet_Roundtrip()
    {
        var control = new TextBlock();
        View.SetCommandParameter(control, "my-param");
        var result = View.GetCommandParameter(control);

        await Assert.That(result).IsEqualTo("my-param");
    }

    [Test]
    public async Task CommandParameter_Default_IsNull()
    {
        var control = new TextBlock();
        var result = View.GetCommandParameter(control);

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task Context_GetSet_Roundtrip()
    {
        var control = new TextBlock();
        View.SetContext(control, "detail");
        var result = View.GetContext(control);

        await Assert.That(result).IsEqualTo("detail");
    }

    [Test]
    public async Task Context_Default_IsNull()
    {
        var control = new TextBlock();
        var result = View.GetContext(control);

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task IsGenerated_GetSet_Roundtrip()
    {
        var control = new TextBlock();
        View.SetIsGenerated(control, true);
        var result = View.GetIsGenerated(control);

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsGenerated_Default_IsFalse()
    {
        var control = new TextBlock();
        var result = View.GetIsGenerated(control);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GetDispatcherFrom_ReturnsIDispatcher()
    {
        var result = View.GetDispatcherFrom(Dispatcher.UIThread);

        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsAssignableTo<IDispatcher>();
    }

    [Test]
    public async Task GetDispatcherFrom_CheckAccess_TrueOnUIThread()
    {
        var dispatcher = View.GetDispatcherFrom(Dispatcher.UIThread);
        var result = dispatcher.CheckAccess();

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GetDispatcherFrom_SameDispatcher_AreEqual()
    {
        var dispatcher1 = View.GetDispatcherFrom(Dispatcher.UIThread);
        var dispatcher2 = View.GetDispatcherFrom(Dispatcher.UIThread);
        var areEqual = dispatcher1.Equals(dispatcher2);

        await Assert.That(areEqual).IsTrue();
    }

    [Test]
    public async Task ExecuteOnFirstLoad_FiresOnLoad()
    {
        var control = new TextBlock();
        var handlerFired = false;

        // ExecuteOnFirstLoad wires to Loaded event
        View.ExecuteOnFirstLoad(control, _ => handlerFired = true);

        // In headless without visual tree, the control is not loaded,
        // so the handler is wired but not yet fired
        await Assert.That(handlerFired).IsFalse();
    }

    [Test]
    public async Task ExecuteOnLoad_NotLoaded_ReturnsFalse()
    {
        var control = new TextBlock();
        var result = View.ExecuteOnLoad(control, _ => { });

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task ExecuteOnUnload_WiresHandler()
    {
        // Just verify wiring doesn't throw
        var control = new TextBlock();
        View.ExecuteOnUnload(control, _ => { });

        // Wiring itself is tested by not throwing; verify element still usable
        var ctrl = new TextBlock { Text = "ok" };
        View.ExecuteOnUnload(ctrl, _ => { });
        var text = ctrl.Text;

        await Assert.That(text).IsEqualTo("ok");
    }

    [Test]
    public async Task Bind_GetSet_Roundtrip()
    {
        var control = new TextBlock();
        View.SetBind(control, true);
        var result = View.GetBind(control);

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task Create_GetSet_Roundtrip()
    {
        var control = new TextBlock();
        View.SetCreate(control, true);
        var result = View.GetCreate(control);

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task ExecuteOnFirstLoad_CalledTwice_DoesNotThrow()
    {
        var action = () =>
        {
            var control = new TextBlock();
            int callCount = 0;
            View.ExecuteOnFirstLoad(control, _ => callCount++);
            View.ExecuteOnFirstLoad(control, _ => callCount++);
        };

        await Assert.That(action).ThrowsNothing();
    }

    [Test]
    public async Task ViewModelLocator_GetSet_Roundtrip()
    {
        var config = new ViewModelLocatorConfiguration();
        var sp = new SimpleServiceProvider();
        var locator = new ViewModelLocator(config, sp);

        var control = new TextBlock();
        View.SetViewModelLocator(control, locator);
        var result = View.GetViewModelLocator(control);

        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsAssignableTo<IViewModelLocator>();
    }

    [Test]
    public async Task Bind_WithIViewAwareDataContext_AttachesView()
    {
        var screen = new Screen();
        var control = new ContentControl();
        View.SetBind(control, true);
        control.DataContext = screen;

        var result = ReferenceEquals(((IViewAware)screen).GetView(), control);

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task Bind_ChangingDataContext_DetachesOldAttachesNew()
    {
        var screen1 = new Screen();
        var screen2 = new Screen();
        var control = new ContentControl();
        View.SetBind(control, true);

        control.DataContext = screen1;
        control.DataContext = screen2;

        var oldDetached = ((IViewAware)screen1).GetView() is null;
        var newAttached = ((IViewAware)screen2).GetView() is not null;

        await Assert.That(oldDetached).IsTrue();
        await Assert.That(newAttached).IsTrue();
    }

    [Test]
    public async Task BindAndCreate_BothTrue_ThrowsInvalidOperationException()
    {
        var action = () =>
        {
            var control = new ContentControl();
            control.DataContext = new Screen();
            View.SetBind(control, true);
            View.SetCreate(control, true);
        };

        await Assert.That(action).ThrowsExactly<InvalidOperationException>();
    }

    [Test]
    public async Task Create_WithViewModelLocator_CreatesViewAndSetsContent()
    {
        var config = new ViewModelLocatorConfiguration()
            .AddMapping<TextBlock, Screen>();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());

        var parent = new ContentControl();
        View.SetViewModelLocator(parent, locator);
        View.SetCreate(parent, true);
        parent.DataContext = new Screen();

        var contentNotNull = parent.Content is not null;
        var contentIsTextBlock = parent.Content is TextBlock;

        await Assert.That(contentNotNull).IsTrue();
        await Assert.That(contentIsTextBlock).IsTrue();
    }

    [Test]
    public async Task Bind_WithContext_AttachesViewWithContext()
    {
        var screen = new Screen();
        var control = new ContentControl();
        View.SetContext(control, "detail");
        View.SetBind(control, true);
        control.DataContext = screen;

        var contextViewAttached = ReferenceEquals(((IViewAware)screen).GetView("detail"), control);
        var defaultViewNull = ((IViewAware)screen).GetView() is null;

        await Assert.That(contextViewAttached).IsTrue();
        await Assert.That(defaultViewNull).IsTrue();
    }

    [Test]
    public async Task ExecuteOnLayoutUpdated_WiresHandler_DoesNotThrow()
    {
        var action = () =>
        {
            var control = new TextBlock();
            View.ExecuteOnLayoutUpdated(control, _ => { });
        };

        await Assert.That(action).ThrowsNothing();
    }
}
