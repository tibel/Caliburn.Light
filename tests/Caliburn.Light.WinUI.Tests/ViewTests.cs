using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using TUnit.Core.Executors;

namespace Caliburn.Light.WinUI.Tests;

[TestExecutor<WinUITestExecutor>]
public class ViewTests
{
    [Test]
    public async Task CommandParameter_SetAndGet_RoundTrips()
    {
        var tb = new TextBlock();
        View.SetCommandParameter(tb, "test");
        await Assert.That(View.GetCommandParameter(tb)).IsEqualTo("test");
    }

    [Test]
    public async Task CommandParameter_DefaultValue_IsNull()
    {
        var tb = new TextBlock();
        await Assert.That(View.GetCommandParameter(tb)).IsNull();
    }

    [Test]
    public async Task Context_SetAndGet_RoundTrips()
    {
        var tb = new TextBlock();
        View.SetContext(tb, "detail");
        await Assert.That(View.GetContext(tb)).IsEqualTo("detail");
    }

    [Test]
    public async Task Context_DefaultValue_IsNull()
    {
        var tb = new TextBlock();
        await Assert.That(View.GetContext(tb)).IsNull();
    }

    [Test]
    public async Task IsGenerated_SetAndGet_RoundTrips()
    {
        var tb = new TextBlock();
        View.SetIsGenerated(tb, true);
        var val1 = View.GetIsGenerated(tb);
        View.SetIsGenerated(tb, false);
        var val2 = View.GetIsGenerated(tb);

        await Assert.That(val1).IsTrue();
        await Assert.That(val2).IsFalse();
    }

    [Test]
    public async Task IsGenerated_DefaultValue_IsFalse()
    {
        var tb = new TextBlock();
        await Assert.That(View.GetIsGenerated(tb)).IsFalse();
    }

    [Test]
    public async Task Bind_SetAndGet_RoundTrips()
    {
        var tb = new TextBlock();
        View.SetBind(tb, true);
        await Assert.That(View.GetBind(tb)).IsTrue();
    }

    [Test]
    public async Task Create_SetAndGet_RoundTrips()
    {
        // Use a fresh ContentControl without DataContext to avoid OnCreateChanged side effects
        var cc = new ContentControl();
        View.SetCreate(cc, true);
        await Assert.That(View.GetCreate(cc)).IsTrue();
    }

    [Test]
    public async Task ViewModelLocator_SetAndGet_RoundTrips()
    {
        var tb = new TextBlock();
        var locator = new StubViewModelLocator();
        View.SetViewModelLocator(tb, locator);
        await Assert.That(ReferenceEquals(locator, View.GetViewModelLocator(tb))).IsTrue();
    }

    [Test]
    public async Task GetDispatcherFrom_ReturnsIDispatcher()
    {
        var dq = DispatcherQueue.GetForCurrentThread();
        await Assert.That(View.GetDispatcherFrom(dq)).IsNotNull();
    }

    [Test]
    public async Task GetDispatcherFrom_CheckAccess_TrueOnUIThread()
    {
        var dq = DispatcherQueue.GetForCurrentThread();
        var dispatcher = View.GetDispatcherFrom(dq);
        await Assert.That(dispatcher.CheckAccess()).IsTrue();
    }

    [Test]
    public async Task GetDispatcherFrom_SameDispatcher_AreEqual()
    {
        var dq = DispatcherQueue.GetForCurrentThread();
        var d1 = View.GetDispatcherFrom(dq);
        var d2 = View.GetDispatcherFrom(dq);
        await Assert.That(d1.Equals(d2)).IsTrue();
    }

    [Test]
    public async Task ExecuteOnFirstLoad_NotLoaded_DoesNotFireImmediately()
    {
        var fired = false;
        var tb = new TextBlock();
        View.ExecuteOnFirstLoad(tb, _ => fired = true);
        await Assert.That(fired).IsFalse();
    }

    [Test]
    public async Task ExecuteOnFirstLoad_CalledTwice_SecondIsNoop()
    {
        var count = 0;
        var tb = new TextBlock();
        View.ExecuteOnFirstLoad(tb, _ => count++);
        View.ExecuteOnFirstLoad(tb, _ => count++);
        // Neither handler fires because the element is not loaded,
        // but only the first call wires a handler (count stays 0).
        await Assert.That(count).IsEqualTo(0);
    }

    [Test]
    public async Task ExecuteOnLoad_ReturnsFalse_WhenNotLoaded()
    {
        var tb = new TextBlock();
        await Assert.That(View.ExecuteOnLoad(tb, _ => { })).IsFalse();
    }

    [Test]
    public async Task ExecuteOnUnload_WiresHandler_DoesNotThrow()
    {
        var action = () =>
        {
            var tb = new TextBlock();
            View.ExecuteOnUnload(tb, _ => { });
        };

        await Assert.That(action).ThrowsNothing();
    }

    [Test]
    public async Task Bind_WithIViewAwareDataContext_AttachesView()
    {
        var screen = new Screen();
        var control = new ContentControl();
        View.SetBind(control, true);
        control.DataContext = screen;

        await Assert.That(ReferenceEquals(((IViewAware)screen).GetView(), control)).IsTrue();
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

        await Assert.That(((IViewAware)screen1).GetView() is null).IsTrue();
        await Assert.That(((IViewAware)screen2).GetView() is not null).IsTrue();
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

        await Assert.That(parent.Content is not null).IsTrue();
        await Assert.That(parent.Content is TextBlock).IsTrue();
    }

    [Test]
    public async Task Bind_WithContext_AttachesViewWithContext()
    {
        var screen = new Screen();
        var control = new ContentControl();
        View.SetContext(control, "detail");
        View.SetBind(control, true);
        control.DataContext = screen;

        await Assert.That(ReferenceEquals(((IViewAware)screen).GetView("detail"), control)).IsTrue();
        await Assert.That(((IViewAware)screen).GetView() is null).IsTrue();
    }

    private sealed class StubViewModelLocator : IViewModelLocator
    {
        public Microsoft.UI.Xaml.UIElement LocateForModel(object model, string? context) => new TextBlock();
        public object? LocateForView(Microsoft.UI.Xaml.UIElement view) => null;
    }
}
