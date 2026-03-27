using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

public class ConductorAllActiveTests
{
    private static Task ActivateAsync(object obj) => ((IActivatable)obj).ActivateAsync();
    private static Task DeactivateAsync(object obj, bool close) => ((IActivatable)obj).DeactivateAsync(close);

    [Test]
    public async Task ActivateItemAsync_AddsItemToCollection()
    {
        var conductor = new Conductor<Screen>.Collection.AllActive();
        await ActivateAsync(conductor);
        var item = new Screen();

        await conductor.ActivateItemAsync(item);

        await Assert.That(conductor.Items.Count).IsEqualTo(1);
        await Assert.That(conductor.Items[0]).IsEqualTo(item);
    }

    [Test]
    public async Task ActivateItemAsync_ActivatesItem()
    {
        var conductor = new Conductor<Screen>.Collection.AllActive();
        await ActivateAsync(conductor);
        var item = new Screen();

        await conductor.ActivateItemAsync(item);

        await Assert.That(item.IsActive).IsTrue();
    }

    [Test]
    public async Task ActivateItemAsync_MultipleItems_AllActive()
    {
        var conductor = new Conductor<Screen>.Collection.AllActive();
        await ActivateAsync(conductor);
        var item1 = new Screen();
        var item2 = new Screen();
        var item3 = new Screen();

        await conductor.ActivateItemAsync(item1);
        await conductor.ActivateItemAsync(item2);
        await conductor.ActivateItemAsync(item3);

        await Assert.That(item1.IsActive).IsTrue();
        await Assert.That(item2.IsActive).IsTrue();
        await Assert.That(item3.IsActive).IsTrue();
        await Assert.That(conductor.Items.Count).IsEqualTo(3);
    }

    [Test]
    public async Task ActivateItemAsync_SameItemTwice_DoesNotDuplicate()
    {
        var conductor = new Conductor<Screen>.Collection.AllActive();
        await ActivateAsync(conductor);
        var item = new Screen();

        await conductor.ActivateItemAsync(item);
        await conductor.ActivateItemAsync(item);

        await Assert.That(conductor.Items.Count).IsEqualTo(1);
    }

    [Test]
    public async Task ActivateItemAsync_Null_DoesNothing()
    {
        var conductor = new Conductor<Screen>.Collection.AllActive();
        await ActivateAsync(conductor);

        await conductor.ActivateItemAsync(null);

        await Assert.That(conductor.Items.Count).IsEqualTo(0);
    }

    [Test]
    public async Task DeactivateItemAsync_WithClose_RemovesItem()
    {
        var conductor = new Conductor<Screen>.Collection.AllActive();
        await ActivateAsync(conductor);
        var item = new TestScreen();
        await conductor.ActivateItemAsync(item);

        await conductor.DeactivateItemAsync(item, true);

        await Assert.That(conductor.Items.Count).IsEqualTo(0);
        await Assert.That(item.IsActive).IsFalse();
    }

    [Test]
    public async Task DeactivateItemAsync_WithoutClose_DoesNothing()
    {
        var conductor = new Conductor<Screen>.Collection.AllActive();
        await ActivateAsync(conductor);
        var item = new Screen();
        await conductor.ActivateItemAsync(item);

        await conductor.DeactivateItemAsync(item, false);

        await Assert.That(conductor.Items.Count).IsEqualTo(1);
        await Assert.That(item.IsActive).IsTrue();
    }

    [Test]
    public async Task DeactivateItemAsync_WithClose_ItemCannotClose_KeepsItem()
    {
        var conductor = new Conductor<Screen>.Collection.AllActive();
        await ActivateAsync(conductor);
        var item = new TestScreen { CanCloseResult = false };
        await conductor.ActivateItemAsync(item);

        await conductor.DeactivateItemAsync(item, true);

        await Assert.That(conductor.Items.Count).IsEqualTo(1);
        await Assert.That(item.IsActive).IsTrue();
    }

    [Test]
    public async Task GetChildren_ReturnsAllItems()
    {
        var conductor = new Conductor<Screen>.Collection.AllActive();
        await ActivateAsync(conductor);
        var item1 = new Screen();
        var item2 = new Screen();
        await conductor.ActivateItemAsync(item1);
        await conductor.ActivateItemAsync(item2);

        var children = conductor.GetChildren();

        await Assert.That(children.Count).IsEqualTo(2);
        await Assert.That(children[0]).IsEqualTo(item1);
        await Assert.That(children[1]).IsEqualTo(item2);
    }

    [Test]
    public async Task ConductorActivation_ActivatesAllItems()
    {
        var conductor = new Conductor<Screen>.Collection.AllActive();
        var item1 = new Screen();
        var item2 = new Screen();
        conductor.Items.Add(item1);
        conductor.Items.Add(item2);
        await Assert.That(item1.IsActive).IsFalse();
        await Assert.That(item2.IsActive).IsFalse();

        await ActivateAsync(conductor);

        await Assert.That(item1.IsActive).IsTrue();
        await Assert.That(item2.IsActive).IsTrue();
    }

    [Test]
    public async Task ConductorDeactivation_WithoutClose_DeactivatesAllItems()
    {
        var conductor = new Conductor<Screen>.Collection.AllActive();
        await ActivateAsync(conductor);
        var item1 = new Screen();
        var item2 = new Screen();
        await conductor.ActivateItemAsync(item1);
        await conductor.ActivateItemAsync(item2);

        await DeactivateAsync(conductor, false);

        await Assert.That(item1.IsActive).IsFalse();
        await Assert.That(item2.IsActive).IsFalse();
        await Assert.That(conductor.Items.Count).IsEqualTo(2);
    }

    [Test]
    public async Task ConductorDeactivation_WithClose_ClearsItems()
    {
        var conductor = new Conductor<Screen>.Collection.AllActive();
        await ActivateAsync(conductor);
        var item1 = new Screen();
        var item2 = new Screen();
        await conductor.ActivateItemAsync(item1);
        await conductor.ActivateItemAsync(item2);

        await DeactivateAsync(conductor, true);

        await Assert.That(conductor.Items.Count).IsEqualTo(0);
    }

    [Test]
    public async Task CanCloseAsync_AllCanClose_ReturnsTrue()
    {
        var conductor = new Conductor<Screen>.Collection.AllActive();
        await ActivateAsync(conductor);
        var item1 = new TestScreen { CanCloseResult = true };
        var item2 = new TestScreen { CanCloseResult = true };
        await conductor.ActivateItemAsync(item1);
        await conductor.ActivateItemAsync(item2);

        var result = await conductor.CanCloseAsync();

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task CanCloseAsync_SomeCannotClose_ReturnsFalse()
    {
        var conductor = new Conductor<Screen>.Collection.AllActive();
        await ActivateAsync(conductor);
        var item1 = new TestScreen { CanCloseResult = true };
        var item2 = new TestScreen { CanCloseResult = false };
        await conductor.ActivateItemAsync(item1);
        await conductor.ActivateItemAsync(item2);

        var result = await conductor.CanCloseAsync();

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task CanCloseAsync_Empty_ReturnsTrue()
    {
        var conductor = new Conductor<Screen>.Collection.AllActive();
        await ActivateAsync(conductor);

        var result = await conductor.CanCloseAsync();

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task ActivateItemAsync_SetsParentOnIChild()
    {
        var conductor = new Conductor<Screen>.Collection.AllActive();
        await ActivateAsync(conductor);
        var item = new TestScreen();

        await conductor.ActivateItemAsync(item);

        await Assert.That(item.Parent).IsEqualTo(conductor);
    }

    [Test]
    public async Task ActivationProcessed_Fires_OnActivateItem()
    {
        var conductor = new Conductor<Screen>.Collection.AllActive();
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
    public async Task AllActive_MultipleItems_AllActive()
    {
        var conductor = new Conductor<Screen>.Collection.AllActive();
        await ActivateAsync(conductor);

        var screens = Enumerable.Range(0, 5).Select(_ => new Screen()).ToArray();
        foreach (var s in screens)
            await conductor.ActivateItemAsync(s);

        foreach (var s in screens)
            await Assert.That(s.IsActive).IsTrue();

        await Assert.That(conductor.GetChildren().Count).IsEqualTo(5);
    }

    [Test]
    public async Task AllActive_CloseOne_OthersStillActive()
    {
        var conductor = new Conductor<Screen>.Collection.AllActive();
        await ActivateAsync(conductor);

        var s1 = new Screen();
        var s2 = new Screen();
        await conductor.ActivateItemAsync(s1);
        await conductor.ActivateItemAsync(s2);

        await conductor.DeactivateItemAsync(s1, true);
        await Assert.That(s1.IsActive).IsFalse();
        await Assert.That(s2.IsActive).IsTrue();
    }
}
