using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

/// <summary>
/// A Screen subclass that implements IChild and allows controlling CanCloseAsync.
/// Shared across conductor test files.
/// </summary>
internal class TestScreen : Screen, IChild
{
    public bool CanCloseResult { get; set; } = true;

    public override Task<bool> CanCloseAsync() => Task.FromResult(CanCloseResult);

    private object? _parent;

    public object? Parent
    {
        get => _parent;
        set => _parent = value;
    }
}

public class ConductorTests
{
    private static Task ActivateAsync(object obj) => ((IActivatable)obj).ActivateAsync();
    private static Task DeactivateAsync(object obj, bool close) => ((IActivatable)obj).DeactivateAsync(close);

    [Test]
    public async Task ActiveItem_Initially_IsNull()
    {
        var conductor = new Conductor<Screen>();

        await Assert.That(conductor.ActiveItem).IsNull();
    }

    [Test]
    public async Task GetChildren_NoActiveItem_ReturnsEmpty()
    {
        var conductor = new Conductor<Screen>();

        var children = conductor.GetChildren();

        await Assert.That(children.Count).IsEqualTo(0);
    }

    [Test]
    public async Task ActivateItemAsync_WithItem_SetsActiveItem()
    {
        var conductor = new Conductor<Screen>();
        await ActivateAsync(conductor);
        var item = new Screen();

        await conductor.ActivateItemAsync(item);

        await Assert.That(conductor.ActiveItem).IsEqualTo(item);
    }

    [Test]
    public async Task ActivateItemAsync_WithItem_ActivatesItem()
    {
        var conductor = new Conductor<Screen>();
        await ActivateAsync(conductor);
        var item = new Screen();

        await conductor.ActivateItemAsync(item);

        await Assert.That(item.IsActive).IsTrue();
    }

    [Test]
    public async Task ActivateItemAsync_SameItem_ReactivatesItem()
    {
        var conductor = new Conductor<Screen>();
        await ActivateAsync(conductor);
        var item = new Screen();
        await conductor.ActivateItemAsync(item);
        await conductor.DeactivateItemAsync(item, false);
        await Assert.That(item.IsActive).IsFalse();

        await conductor.ActivateItemAsync(item);

        await Assert.That(item.IsActive).IsTrue();
    }

    [Test]
    public async Task ActivateItemAsync_NewItem_DeactivatesOldItem()
    {
        var conductor = new Conductor<Screen>();
        await ActivateAsync(conductor);
        var item1 = new TestScreen();
        var item2 = new TestScreen();

        await conductor.ActivateItemAsync(item1);
        await conductor.ActivateItemAsync(item2);

        await Assert.That(item1.IsActive).IsFalse();
        await Assert.That(item2.IsActive).IsTrue();
        await Assert.That(conductor.ActiveItem).IsEqualTo(item2);
    }

    [Test]
    public async Task ActivateItemAsync_NewItem_OldCannotClose_DoesNotSwitch()
    {
        var conductor = new Conductor<Screen>();
        await ActivateAsync(conductor);
        var item1 = new TestScreen { CanCloseResult = false };
        var item2 = new TestScreen();

        await conductor.ActivateItemAsync(item1);
        await conductor.ActivateItemAsync(item2);

        await Assert.That(conductor.ActiveItem).IsEqualTo(item1);
        await Assert.That(item1.IsActive).IsTrue();
    }

    [Test]
    public async Task ActivationProcessed_OnActivation_FiresWithSuccess()
    {
        var conductor = new Conductor<Screen>();
        await ActivateAsync(conductor);
        var item = new Screen();
        ActivationProcessedEventArgs? eventArgs = null;
        conductor.ActivationProcessed += (_, e) => eventArgs = e;

        await conductor.ActivateItemAsync(item);

        await Assert.That(eventArgs).IsNotNull();
        await Assert.That(eventArgs!.Item).IsEqualTo(item);
        await Assert.That(eventArgs.Success).IsTrue();
    }

    [Test]
    public async Task ActivationProcessed_FailedActivation_FiresWithFailure()
    {
        var conductor = new Conductor<Screen>();
        await ActivateAsync(conductor);
        var item1 = new TestScreen { CanCloseResult = false };
        await conductor.ActivateItemAsync(item1);
        ActivationProcessedEventArgs? eventArgs = null;
        conductor.ActivationProcessed += (_, e) => eventArgs = e;
        var item2 = new TestScreen();

        await conductor.ActivateItemAsync(item2);

        await Assert.That(eventArgs).IsNotNull();
        await Assert.That(eventArgs!.Item).IsEqualTo(item2);
        await Assert.That(eventArgs.Success).IsFalse();
    }

    [Test]
    public async Task DeactivateItemAsync_WithClose_RemovesActiveItem()
    {
        var conductor = new Conductor<Screen>();
        await ActivateAsync(conductor);
        var item = new TestScreen();
        await conductor.ActivateItemAsync(item);

        await conductor.DeactivateItemAsync(item, true);

        await Assert.That(conductor.ActiveItem).IsNull();
        await Assert.That(item.IsActive).IsFalse();
    }

    [Test]
    public async Task DeactivateItemAsync_WithoutClose_DeactivatesButKeepsActiveItem()
    {
        var conductor = new Conductor<Screen>();
        await ActivateAsync(conductor);
        var item = new Screen();
        await conductor.ActivateItemAsync(item);

        await conductor.DeactivateItemAsync(item, false);

        await Assert.That(conductor.ActiveItem).IsEqualTo(item);
        await Assert.That(item.IsActive).IsFalse();
    }

    [Test]
    public async Task DeactivateItemAsync_WithClose_ItemCannotClose_KeepsActiveItem()
    {
        var conductor = new Conductor<Screen>();
        await ActivateAsync(conductor);
        var item = new TestScreen { CanCloseResult = false };
        await conductor.ActivateItemAsync(item);

        await conductor.DeactivateItemAsync(item, true);

        await Assert.That(conductor.ActiveItem).IsEqualTo(item);
        await Assert.That(item.IsActive).IsTrue();
    }

    [Test]
    public async Task DeactivateItemAsync_NonActiveItem_DoesNothing()
    {
        var conductor = new Conductor<Screen>();
        await ActivateAsync(conductor);
        var item1 = new TestScreen();
        var item2 = new TestScreen();
        await conductor.ActivateItemAsync(item1);

        await conductor.DeactivateItemAsync(item2, true);

        await Assert.That(conductor.ActiveItem).IsEqualTo(item1);
    }

    [Test]
    public async Task GetChildren_WithActiveItem_ReturnsItem()
    {
        var conductor = new Conductor<Screen>();
        await ActivateAsync(conductor);
        var item = new Screen();
        await conductor.ActivateItemAsync(item);

        var children = conductor.GetChildren();

        await Assert.That(children.Count).IsEqualTo(1);
        await Assert.That(children[0]).IsEqualTo(item);
    }

    [Test]
    public async Task CanCloseAsync_NoActiveItem_ReturnsTrue()
    {
        var conductor = new Conductor<Screen>();

        var result = await conductor.CanCloseAsync();

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task CanCloseAsync_ItemCanClose_ReturnsTrue()
    {
        var conductor = new Conductor<Screen>();
        await ActivateAsync(conductor);
        var item = new TestScreen { CanCloseResult = true };
        await conductor.ActivateItemAsync(item);

        var result = await conductor.CanCloseAsync();

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task CanCloseAsync_ItemCannotClose_ReturnsFalse()
    {
        var conductor = new Conductor<Screen>();
        await ActivateAsync(conductor);
        var item = new TestScreen { CanCloseResult = false };
        await conductor.ActivateItemAsync(item);

        var result = await conductor.CanCloseAsync();

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task EnsureItem_SetsParentOnIChild()
    {
        var conductor = new Conductor<Screen>();
        await ActivateAsync(conductor);
        var item = new TestScreen();

        await conductor.ActivateItemAsync(item);

        await Assert.That(item.Parent).IsEqualTo(conductor);
    }

    [Test]
    public async Task ConductorDeactivation_WithClose_ClosesActiveItem()
    {
        var conductor = new Conductor<Screen>();
        await ActivateAsync(conductor);
        var item = new Screen();
        await conductor.ActivateItemAsync(item);

        await DeactivateAsync(conductor, true);

        await Assert.That(conductor.ActiveItem).IsNull();
    }

    [Test]
    public async Task ConductorDeactivation_WithoutClose_DeactivatesActiveItem()
    {
        var conductor = new Conductor<Screen>();
        await ActivateAsync(conductor);
        var item = new Screen();
        await conductor.ActivateItemAsync(item);

        await DeactivateAsync(conductor, false);

        await Assert.That(item.IsActive).IsFalse();
        await Assert.That(conductor.ActiveItem).IsEqualTo(item);
    }

    [Test]
    public async Task ActivateItemAsync_RapidSequential_LastItemWins()
    {
        var conductor = new Conductor<Screen>();
        await ActivateAsync(conductor);

        var screen1 = new Screen();
        var screen2 = new Screen();
        var screen3 = new Screen();

        await conductor.ActivateItemAsync(screen1);
        await conductor.ActivateItemAsync(screen2);
        await conductor.ActivateItemAsync(screen3);

        await Assert.That(conductor.ActiveItem).IsSameReferenceAs(screen3);
        await Assert.That(screen3.IsActive).IsTrue();
        await Assert.That(screen1.IsActive).IsFalse();
        await Assert.That(screen2.IsActive).IsFalse();
    }

    [Test]
    public async Task ActivateItemAsync_SameItemTwice_IsIdempotent()
    {
        var conductor = new Conductor<Screen>();
        await ActivateAsync(conductor);

        var screen = new Screen();
        await conductor.ActivateItemAsync(screen);
        await conductor.ActivateItemAsync(screen);

        await Assert.That(conductor.ActiveItem).IsSameReferenceAs(screen);
        await Assert.That(screen.IsActive).IsTrue();
    }

    [Test]
    public async Task ActivateItemAsync_Null_ClearsActiveItem()
    {
        var conductor = new Conductor<Screen>();
        await ActivateAsync(conductor);

        var screen = new Screen();
        await conductor.ActivateItemAsync(screen);
        await Assert.That(conductor.ActiveItem).IsNotNull();

        await conductor.ActivateItemAsync(null);
        await Assert.That(conductor.ActiveItem).IsNull();
    }

    [Test]
    public async Task ActivateItemAsync_WhileCloseGuardBlocks_DoesNotActivate()
    {
        var conductor = new Conductor<Screen>();
        await ActivateAsync(conductor);

        var blocking = new CloseGuardScreen { AllowClose = false };
        var newScreen = new Screen();

        await conductor.ActivateItemAsync(blocking);
        await Assert.That(conductor.ActiveItem).IsSameReferenceAs(blocking);

        // Try to activate new screen - blocked by close guard
        await conductor.ActivateItemAsync(newScreen);
        await Assert.That(conductor.ActiveItem).IsSameReferenceAs(blocking);
        await Assert.That(newScreen.IsActive).IsFalse();

        // Now allow close and try again
        blocking.AllowClose = true;
        await conductor.ActivateItemAsync(newScreen);
        await Assert.That(conductor.ActiveItem).IsSameReferenceAs(newScreen);
        await Assert.That(newScreen.IsActive).IsTrue();
    }

    [Test]
    public async Task DeactivateItemAsync_ActiveItem_WithClose_ClearsActiveItem()
    {
        var conductor = new Conductor<Screen>();
        await ActivateAsync(conductor);

        var screen = new Screen();
        await conductor.ActivateItemAsync(screen);

        await conductor.DeactivateItemAsync(screen, true);
        await Assert.That(conductor.ActiveItem).IsNull();
        await Assert.That(screen.IsActive).IsFalse();
    }

    [Test]
    public async Task DeactivateItemAsync_NonActiveItem_IsNoOp()
    {
        var conductor = new Conductor<Screen>();
        await ActivateAsync(conductor);

        var active = new Screen();
        var other = new Screen();
        await conductor.ActivateItemAsync(active);

        await conductor.DeactivateItemAsync(other, true);
        await Assert.That(conductor.ActiveItem).IsSameReferenceAs(active);
    }

    [Test]
    public async Task ActivateItemAsync_ConcurrentCalls_NoCorruption()
    {
        var conductor = new Conductor<Screen>();
        await ActivateAsync(conductor);

        var screens = Enumerable.Range(0, 10).Select(_ => new Screen()).ToArray();

        // Fire multiple activations concurrently
        var tasks = screens.Select(s => conductor.ActivateItemAsync(s)).ToArray();
        await Task.WhenAll(tasks);

        // Should have exactly one active item, and it should be active
        await Assert.That(conductor.ActiveItem).IsNotNull();
        await Assert.That(conductor.ActiveItem!.IsActive).IsTrue();

        // All others should not be active
        var inactiveCount = screens.Count(s => !s.IsActive);
        await Assert.That(inactiveCount).IsEqualTo(9);
    }

    private class CloseGuardScreen : Screen
    {
        public bool AllowClose { get; set; } = true;
        public override Task<bool> CanCloseAsync() => Task.FromResult(AllowClose);
    }
}
