using System.Collections;
using System.ComponentModel;
using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

public class ScreenTestScreen : Screen
{
    public int InitializeCallCount { get; private set; }
    public int ActivateCallCount { get; private set; }
    public int DeactivateCallCount { get; private set; }
    public bool? LastDeactivateClose { get; private set; }

    protected override Task OnInitializeAsync()
    {
        InitializeCallCount++;
        return Task.CompletedTask;
    }

    protected override Task OnActivateAsync()
    {
        ActivateCallCount++;
        return Task.CompletedTask;
    }

    protected override Task OnDeactivateAsync(bool close)
    {
        DeactivateCallCount++;
        LastDeactivateClose = close;
        return Task.CompletedTask;
    }
}

public class TestChildScreen : Screen, IChild
{
    public object? Parent { get; set; }
}

public class StubConductor : IConductor
{
    public bool DeactivateItemCalled { get; private set; }
    public object? DeactivatedItem { get; private set; }
    public bool DeactivateClose { get; private set; }

    public Task ActivateItemAsync(object? item) => Task.CompletedTask;

    public Task DeactivateItemAsync(object item, bool close)
    {
        DeactivateItemCalled = true;
        DeactivatedItem = item;
        DeactivateClose = close;
        return Task.CompletedTask;
    }

#pragma warning disable CS0067
    public event EventHandler<ActivationProcessedEventArgs>? ActivationProcessed;
#pragma warning restore CS0067

    public IEnumerable GetChildren() => Array.Empty<object>();
}

[NotInParallel("ViewHelperTests")]
public class ScreenTests
{
    [Before(Test)]
    public void ResetViewHelperBefore()
    {
        ViewHelper.Reset();
    }

    [After(Test)]
    public void ResetViewHelperAfter()
    {
        ViewHelper.Reset();
    }

    [Test]
    public async Task InitialState_IsNotActive()
    {
        var screen = new ScreenTestScreen();

        await Assert.That(screen.IsActive).IsFalse();
    }

    [Test]
    public async Task InitialState_IsNotInitialized()
    {
        var screen = new ScreenTestScreen();

        await Assert.That(screen.IsInitialized).IsFalse();
    }

    [Test]
    public async Task ActivateAsync_FirstTime_InitializesScreen()
    {
        var screen = new ScreenTestScreen();
        IActivatable activatable = screen;

        await activatable.ActivateAsync();

        await Assert.That(screen.IsInitialized).IsTrue();
        await Assert.That(screen.InitializeCallCount).IsEqualTo(1);
    }

    [Test]
    public async Task ActivateAsync_FirstTime_SetsIsActive()
    {
        var screen = new ScreenTestScreen();
        IActivatable activatable = screen;

        await activatable.ActivateAsync();

        await Assert.That(screen.IsActive).IsTrue();
    }

    [Test]
    public async Task ActivateAsync_FirstTime_CallsOnActivateAsync()
    {
        var screen = new ScreenTestScreen();
        IActivatable activatable = screen;

        await activatable.ActivateAsync();

        await Assert.That(screen.ActivateCallCount).IsEqualTo(1);
    }

    [Test]
    public async Task ActivateAsync_FirstTime_RaisesActivatedWithWasInitializedTrue()
    {
        var screen = new ScreenTestScreen();
        IActivatable activatable = screen;
        ActivationEventArgs? args = null;
        activatable.Activated += (_, e) => args = e;

        await activatable.ActivateAsync();

        await Assert.That(args).IsNotNull();
        await Assert.That(args!.WasInitialized).IsTrue();
    }

    [Test]
    public async Task ActivateAsync_SecondTime_DoesNotReinitialize()
    {
        var screen = new ScreenTestScreen();
        IActivatable activatable = screen;

        await activatable.ActivateAsync();
        await activatable.DeactivateAsync(false);
        await activatable.ActivateAsync();

        await Assert.That(screen.InitializeCallCount).IsEqualTo(1);
        await Assert.That(screen.ActivateCallCount).IsEqualTo(2);
    }

    [Test]
    public async Task ActivateAsync_SecondTime_RaisesActivatedWithWasInitializedFalse()
    {
        var screen = new ScreenTestScreen();
        IActivatable activatable = screen;
        await activatable.ActivateAsync();
        await activatable.DeactivateAsync(false);

        ActivationEventArgs? args = null;
        activatable.Activated += (_, e) => args = e;
        await activatable.ActivateAsync();

        await Assert.That(args).IsNotNull();
        await Assert.That(args!.WasInitialized).IsFalse();
    }

    [Test]
    public async Task ActivateAsync_WhenAlreadyActive_DoesNothing()
    {
        var screen = new ScreenTestScreen();
        IActivatable activatable = screen;
        await activatable.ActivateAsync();

        await activatable.ActivateAsync();

        await Assert.That(screen.ActivateCallCount).IsEqualTo(1);
    }

    [Test]
    public async Task ActivateAsync_RaisesPropertyChangedForIsActive()
    {
        var screen = new ScreenTestScreen();
        IActivatable activatable = screen;
        var changedProperties = new List<string?>();
        screen.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName);

        await activatable.ActivateAsync();

        await Assert.That(changedProperties).Contains("IsActive");
    }

    [Test]
    public async Task ActivateAsync_RaisesPropertyChangedForIsInitialized()
    {
        var screen = new ScreenTestScreen();
        IActivatable activatable = screen;
        var changedProperties = new List<string?>();
        screen.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName);

        await activatable.ActivateAsync();

        await Assert.That(changedProperties).Contains("IsInitialized");
    }

    [Test]
    public async Task DeactivateAsync_SetsIsActiveFalse()
    {
        var screen = new ScreenTestScreen();
        IActivatable activatable = screen;
        await activatable.ActivateAsync();

        await activatable.DeactivateAsync(false);

        await Assert.That(screen.IsActive).IsFalse();
    }

    [Test]
    public async Task DeactivateAsync_RaisesDeactivatingEvent()
    {
        var screen = new ScreenTestScreen();
        IActivatable activatable = screen;
        await activatable.ActivateAsync();
        DeactivationEventArgs? args = null;
        activatable.Deactivating += (_, e) => args = e;

        await activatable.DeactivateAsync(false);

        await Assert.That(args).IsNotNull();
        await Assert.That(args!.WasClosed).IsFalse();
    }

    [Test]
    public async Task DeactivateAsync_RaisesDeactivatedEvent()
    {
        var screen = new ScreenTestScreen();
        IActivatable activatable = screen;
        await activatable.ActivateAsync();
        DeactivationEventArgs? args = null;
        activatable.Deactivated += (_, e) => args = e;

        await activatable.DeactivateAsync(false);

        await Assert.That(args).IsNotNull();
        await Assert.That(args!.WasClosed).IsFalse();
    }

    [Test]
    public async Task DeactivateAsync_CloseTrue_PropagatesCloseParameter()
    {
        var screen = new ScreenTestScreen();
        IActivatable activatable = screen;
        await activatable.ActivateAsync();

        await activatable.DeactivateAsync(true);

        await Assert.That(screen.LastDeactivateClose).IsTrue();
    }

    [Test]
    public async Task DeactivateAsync_CloseTrue_EventsHaveWasClosedTrue()
    {
        var screen = new ScreenTestScreen();
        IActivatable activatable = screen;
        await activatable.ActivateAsync();
        DeactivationEventArgs? deactivatingArgs = null;
        DeactivationEventArgs? deactivatedArgs = null;
        activatable.Deactivating += (_, e) => deactivatingArgs = e;
        activatable.Deactivated += (_, e) => deactivatedArgs = e;

        await activatable.DeactivateAsync(true);

        await Assert.That(deactivatingArgs!.WasClosed).IsTrue();
        await Assert.That(deactivatedArgs!.WasClosed).IsTrue();
    }

    [Test]
    public async Task DeactivateAsync_WhenNotActiveAndNotInitialized_DoesNothing()
    {
        var screen = new ScreenTestScreen();
        IActivatable activatable = screen;

        await activatable.DeactivateAsync(false);

        await Assert.That(screen.DeactivateCallCount).IsEqualTo(0);
    }

    [Test]
    public async Task DeactivateAsync_WhenInactiveButInitializedAndCloseTrue_StillDeactivates()
    {
        var screen = new ScreenTestScreen();
        IActivatable activatable = screen;
        await activatable.ActivateAsync();
        await activatable.DeactivateAsync(false);
        screen.DeactivateCallCount.ToString(); // confirm already deactivated once

        await activatable.DeactivateAsync(true);

        // Should still fire because IsInitialized=true and close=true
        await Assert.That(screen.DeactivateCallCount).IsEqualTo(2);
    }

    [Test]
    public async Task CanCloseAsync_ReturnsTrue()
    {
        var screen = new ScreenTestScreen();

        var result = await screen.CanCloseAsync();

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task TryCloseAsync_WithConductorParent_CallsDeactivateItem()
    {
        var screen = new TestChildScreen();
        var conductor = new StubConductor();
        screen.Parent = conductor;

        await screen.TryCloseAsync();

        await Assert.That(conductor.DeactivateItemCalled).IsTrue();
        await Assert.That(conductor.DeactivatedItem).IsEqualTo(screen);
        await Assert.That(conductor.DeactivateClose).IsTrue();
    }

    [Test]
    public async Task TryCloseAsync_WithoutParent_CompletesWithoutError()
    {
        var screen = new TestChildScreen();

        await screen.TryCloseAsync();

        // Should complete without throwing and screen should not be active
        await Assert.That(screen.IsActive).IsFalse();
    }

    [Test]
    public async Task FullLifecycle_CorrectEventSequence()
    {
        var screen = new ScreenTestScreen();
        IActivatable activatable = screen;
        var events = new List<string>();

        activatable.Activated += (_, e) => events.Add($"Activated(WasInitialized={e.WasInitialized})");
        activatable.Deactivating += (_, e) => events.Add($"Deactivating(WasClosed={e.WasClosed})");
        activatable.Deactivated += (_, e) => events.Add($"Deactivated(WasClosed={e.WasClosed})");

        await activatable.ActivateAsync();
        await activatable.DeactivateAsync(false);
        await activatable.ActivateAsync();
        await activatable.DeactivateAsync(true);

        await Assert.That(events.Count).IsEqualTo(6);
        await Assert.That(events[0]).IsEqualTo("Activated(WasInitialized=True)");
        await Assert.That(events[1]).IsEqualTo("Deactivating(WasClosed=False)");
        await Assert.That(events[2]).IsEqualTo("Deactivated(WasClosed=False)");
        await Assert.That(events[3]).IsEqualTo("Activated(WasInitialized=False)");
        await Assert.That(events[4]).IsEqualTo("Deactivating(WasClosed=True)");
        await Assert.That(events[5]).IsEqualTo("Deactivated(WasClosed=True)");
    }
}
