using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using TUnit.Core.Executors;

namespace Caliburn.Light.WPF.Tests;

[TestExecutor<WpfTestExecutor>]
public class ViewTests
{
    [Test]
    public async Task CommandParameter_SetAndGet_RoundTrips()
    {
        var button = new Button();
        View.SetCommandParameter(button, "my-param");
        var result = View.GetCommandParameter(button);
        await Assert.That(result).IsEqualTo("my-param");
    }

    [Test]
    public async Task CommandParameter_DefaultValue_IsNull()
    {
        var button = new Button();
        var result = View.GetCommandParameter(button);
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task Context_SetAndGet_RoundTrips()
    {
        var cc = new ContentControl();
        View.SetContext(cc, "detail");
        var result = View.GetContext(cc);
        await Assert.That(result).IsEqualTo("detail");
    }

    [Test]
    public async Task Context_DefaultValue_IsNull()
    {
        var cc = new ContentControl();
        var result = View.GetContext(cc);
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task IsGenerated_SetAndGet_RoundTrips()
    {
        var cc = new ContentControl();

        await Assert.That(View.GetIsGenerated(cc)).IsFalse();

        View.SetIsGenerated(cc, true);
        await Assert.That(View.GetIsGenerated(cc)).IsTrue();

        View.SetIsGenerated(cc, false);
        await Assert.That(View.GetIsGenerated(cc)).IsFalse();
    }

    [Test]
    public async Task GetDispatcherFrom_ReturnsIDispatcher()
    {
        var dispatcher = View.GetDispatcherFrom(Dispatcher.CurrentDispatcher);
        await Assert.That(dispatcher).IsNotNull();
        await Assert.That(dispatcher.CheckAccess()).IsTrue();
    }

    [Test]
    public async Task ExecuteOnFirstLoad_FiresHandler_OnFirstLoadedEvent()
    {
        var button = new Button();
        int callCount = 0;

        View.ExecuteOnFirstLoad(button, _ => callCount++);
        await Assert.That(callCount).IsEqualTo(0);

        button.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
        await Assert.That(callCount).IsEqualTo(1);
    }

    [Test]
    public async Task ExecuteOnFirstLoad_DoesNotFireAgain_OnSubsequentCalls()
    {
        var button = new Button();
        int callCount = 0;

        View.ExecuteOnFirstLoad(button, _ => callCount++);
        button.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
        await Assert.That(callCount).IsEqualTo(1);

        // Second call to ExecuteOnFirstLoad should be no-op
        View.ExecuteOnFirstLoad(button, _ => callCount++);
        button.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
        await Assert.That(callCount).IsEqualTo(1);
    }

    [Test]
    public async Task ExecuteOnLoad_ReturnsFalse_WhenNotLoaded()
    {
        var button = new Button();
        bool called = false;

        var result = View.ExecuteOnLoad(button, _ => called = true);
        await Assert.That(result).IsFalse();
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task ExecuteOnLoad_FiresHandler_WhenLoadedEventRaised()
    {
        var button = new Button();
        bool called = false;

        View.ExecuteOnLoad(button, _ => called = true);
        button.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
        await Assert.That(called).IsTrue();
    }

    [Test]
    public async Task ExecuteOnUnload_FiresHandler_WhenUnloadedEventRaised()
    {
        var button = new Button();
        bool called = false;

        View.ExecuteOnUnload(button, _ => called = true);
        await Assert.That(called).IsFalse();

        button.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
        await Assert.That(called).IsTrue();
    }

    [Test]
    public async Task ViewModelLocator_AttachedProperty_SetAndGet_RoundTrips()
    {
        var config = new ViewModelLocatorConfiguration();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());

        var cc = new ContentControl();
        View.SetViewModelLocator(cc, locator);
        var result = View.GetViewModelLocator(cc);
        await Assert.That(ReferenceEquals(result, locator)).IsTrue();
    }

    [Test]
    public async Task IsGenerated_DefaultValue_IsFalse()
    {
        var cc = new ContentControl();
        await Assert.That(View.GetIsGenerated(cc)).IsFalse();
    }

    [Test]
    public async Task GetDispatcherFrom_CheckAccess_TrueOnUIThread()
    {
        var dispatcher = View.GetDispatcherFrom(System.Windows.Threading.Dispatcher.CurrentDispatcher);
        await Assert.That(dispatcher.CheckAccess()).IsTrue();
    }

    [Test]
    public async Task GetDispatcherFrom_SameDispatcher_AreEqual()
    {
        var d1 = View.GetDispatcherFrom(System.Windows.Threading.Dispatcher.CurrentDispatcher);
        var d2 = View.GetDispatcherFrom(System.Windows.Threading.Dispatcher.CurrentDispatcher);
        await Assert.That(d1.Equals(d2)).IsTrue();
    }

    [Test]
    public async Task Bind_SetAndGet_RoundTrips()
    {
        var cc = new ContentControl();
        await Assert.That(View.GetBind(cc)).IsFalse();

        View.SetBind(cc, true);
        await Assert.That(View.GetBind(cc)).IsTrue();
    }

    [Test]
    public async Task Create_SetAndGet_RoundTrips()
    {
        var cc = new ContentControl();
        await Assert.That(View.GetCreate(cc)).IsFalse();

        View.SetCreate(cc, true);
        await Assert.That(View.GetCreate(cc)).IsTrue();
    }

    [Test]
    public async Task Bind_WithIViewAwareDataContext_AttachesView()
    {
        var screen = new Screen();
        var control = new ContentControl();
        View.SetBind(control, true);
        control.DataContext = screen;

        var attachedView = ((IViewAware)screen).GetView();
        await Assert.That(ReferenceEquals(attachedView, control)).IsTrue();
    }

    [Test]
    public async Task Bind_ChangingDataContext_DetachesOldAttachesNew()
    {
        var screen1 = new Screen();
        var screen2 = new Screen();
        var control = new ContentControl();
        View.SetBind(control, true);

        control.DataContext = screen1;
        await Assert.That(((IViewAware)screen1).GetView()).IsNotNull();

        control.DataContext = screen2;
        await Assert.That(((IViewAware)screen1).GetView()).IsNull();
        await Assert.That(((IViewAware)screen2).GetView()).IsNotNull();
    }

    [Test]
    public async Task BindAndCreate_BothTrue_ThrowsInvalidOperationException()
    {
        var control = new ContentControl();
        control.DataContext = new Screen();
        View.SetBind(control, true);

        await Assert.That(() => View.SetCreate(control, true)).ThrowsExactly<InvalidOperationException>();
    }

    [Test]
    public async Task Create_WithViewModelLocator_CreatesViewAndSetsContent()
    {
        var config = new ViewModelLocatorConfiguration()
            .AddMapping<System.Windows.Controls.TextBlock, Screen>();
        var locator = new ViewModelLocator(config, new SimpleServiceProvider());

        var parent = new ContentControl();
        View.SetViewModelLocator(parent, locator);
        View.SetCreate(parent, true);
        parent.DataContext = new Screen();

        await Assert.That(parent.Content).IsNotNull();
        await Assert.That(parent.Content is System.Windows.Controls.TextBlock).IsTrue();
    }

    [Test]
    public async Task Bind_WithContext_AttachesViewWithContext()
    {
        var screen = new Screen();
        var control = new ContentControl();
        View.SetContext(control, "detail");
        View.SetBind(control, true);
        control.DataContext = screen;

        var attachedView = ((IViewAware)screen).GetView("detail");
        await Assert.That(ReferenceEquals(attachedView, control)).IsTrue();
        await Assert.That(((IViewAware)screen).GetView()).IsNull();
    }
}
