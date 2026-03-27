using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

public class ConductorOneActiveTests
{
    private static Task ActivateAsync(object obj) => ((IActivatable)obj).ActivateAsync();
    private static Task DeactivateAsync(object obj, bool close) => ((IActivatable)obj).DeactivateAsync(close);

    [Test]
    public async Task ActivateItemAsync_SetsActiveAndAddsToItems()
    {
        var conductor = new Conductor<Screen>.Collection.OneActive();
        await ActivateAsync(conductor);
        var item = new Screen();

        await conductor.ActivateItemAsync(item);

        await Assert.That(conductor.ActiveItem).IsEqualTo(item);
        await Assert.That(conductor.Items.Count).IsEqualTo(1);
        await Assert.That(item.IsActive).IsTrue();
    }

    [Test]
    public async Task ActivateItemAsync_SwitchesActiveItem()
    {
        var conductor = new Conductor<Screen>.Collection.OneActive();
        await ActivateAsync(conductor);
        var item1 = new Screen();
        var item2 = new Screen();

        await conductor.ActivateItemAsync(item1);
        await conductor.ActivateItemAsync(item2);

        await Assert.That(conductor.ActiveItem).IsEqualTo(item2);
        await Assert.That(item2.IsActive).IsTrue();
        await Assert.That(item1.IsActive).IsFalse();
        await Assert.That(conductor.Items.Count).IsEqualTo(2);
    }

    [Test]
    public async Task ActivateItemAsync_SameItem_Reactivates()
    {
        var conductor = new Conductor<Screen>.Collection.OneActive();
        await ActivateAsync(conductor);
        var item = new Screen();
        await conductor.ActivateItemAsync(item);
        await DeactivateAsync(item, false);
        await Assert.That(item.IsActive).IsFalse();

        await conductor.ActivateItemAsync(item);

        await Assert.That(item.IsActive).IsTrue();
        await Assert.That(conductor.ActiveItem).IsEqualTo(item);
    }

    [Test]
    public async Task DeactivateItemAsync_CloseActiveItem_SelectsNext()
    {
        var conductor = new Conductor<Screen>.Collection.OneActive();
        await ActivateAsync(conductor);
        var item1 = new TestScreen();
        var item2 = new TestScreen();
        await conductor.ActivateItemAsync(item1);
        await conductor.ActivateItemAsync(item2);
        // Items: [item1, item2], Active: item2

        await conductor.DeactivateItemAsync(item2, true);

        await Assert.That(conductor.ActiveItem).IsEqualTo(item1);
        await Assert.That(conductor.Items.Count).IsEqualTo(1);
    }

    [Test]
    public async Task DetermineNextItemToActivate_ClosingFirst_SelectsSecond()
    {
        var conductor = new Conductor<Screen>.Collection.OneActive();
        await ActivateAsync(conductor);
        var item1 = new TestScreen();
        var item2 = new TestScreen();
        var item3 = new TestScreen();
        await conductor.ActivateItemAsync(item1);
        await conductor.ActivateItemAsync(item2);
        await conductor.ActivateItemAsync(item3);
        // Items: [item1, item2, item3], Active: item3
        await conductor.ActivateItemAsync(item1);
        // Active: item1

        await conductor.DeactivateItemAsync(item1, true);

        await Assert.That(conductor.ActiveItem).IsEqualTo(item2);
    }

    [Test]
    public async Task DetermineNextItemToActivate_ClosingMiddle_SelectsPrevious()
    {
        var conductor = new Conductor<Screen>.Collection.OneActive();
        await ActivateAsync(conductor);
        var item1 = new TestScreen();
        var item2 = new TestScreen();
        var item3 = new TestScreen();
        await conductor.ActivateItemAsync(item1);
        await conductor.ActivateItemAsync(item2);
        await conductor.ActivateItemAsync(item3);
        // Items: [item1, item2, item3], Active: item3
        await conductor.ActivateItemAsync(item2);
        // Active: item2

        await conductor.DeactivateItemAsync(item2, true);

        await Assert.That(conductor.ActiveItem).IsEqualTo(item1);
    }

    [Test]
    public async Task DetermineNextItemToActivate_ClosingLast_SelectsPrevious()
    {
        var conductor = new Conductor<Screen>.Collection.OneActive();
        await ActivateAsync(conductor);
        var item1 = new TestScreen();
        var item2 = new TestScreen();
        var item3 = new TestScreen();
        await conductor.ActivateItemAsync(item1);
        await conductor.ActivateItemAsync(item2);
        await conductor.ActivateItemAsync(item3);
        // Items: [item1, item2, item3], Active: item3

        await conductor.DeactivateItemAsync(item3, true);

        await Assert.That(conductor.ActiveItem).IsEqualTo(item2);
    }

    [Test]
    public async Task DetermineNextItemToActivate_ClosingOnlyItem_ActiveBecomesNull()
    {
        var conductor = new Conductor<Screen>.Collection.OneActive();
        await ActivateAsync(conductor);
        var item = new TestScreen();
        await conductor.ActivateItemAsync(item);

        await conductor.DeactivateItemAsync(item, true);

        await Assert.That(conductor.ActiveItem).IsNull();
        await Assert.That(conductor.Items.Count).IsEqualTo(0);
    }

    [Test]
    public async Task DeactivateItemAsync_WithoutClose_ItemRemainsInCollection()
    {
        var conductor = new Conductor<Screen>.Collection.OneActive();
        await ActivateAsync(conductor);
        var item1 = new TestScreen();
        var item2 = new TestScreen();
        await conductor.ActivateItemAsync(item1);
        await conductor.ActivateItemAsync(item2);
        // Items: [item1, item2], Active: item2

        await conductor.DeactivateItemAsync(item2, false);

        await Assert.That(conductor.Items.Count).IsEqualTo(2);
        await Assert.That(item2.IsActive).IsFalse();
    }

    [Test]
    public async Task DeactivateItemAsync_NonActiveItem_WithClose_RemovesFromCollection()
    {
        var conductor = new Conductor<Screen>.Collection.OneActive();
        await ActivateAsync(conductor);
        var item1 = new TestScreen();
        var item2 = new TestScreen();
        await conductor.ActivateItemAsync(item1);
        await conductor.ActivateItemAsync(item2);
        // Items: [item1, item2], Active: item2

        await conductor.DeactivateItemAsync(item1, true);

        await Assert.That(conductor.ActiveItem).IsEqualTo(item2);
        await Assert.That(conductor.Items.Count).IsEqualTo(1);
    }

    [Test]
    public async Task DeactivateItemAsync_NonActiveItem_WithoutClose_KeepsInCollection()
    {
        var conductor = new Conductor<Screen>.Collection.OneActive();
        await ActivateAsync(conductor);
        var item1 = new TestScreen();
        var item2 = new TestScreen();
        await conductor.ActivateItemAsync(item1);
        await conductor.ActivateItemAsync(item2);
        // Items: [item1, item2], Active: item2

        await conductor.DeactivateItemAsync(item1, false);

        await Assert.That(conductor.ActiveItem).IsEqualTo(item2);
        await Assert.That(conductor.Items.Count).IsEqualTo(2);
    }

    [Test]
    public async Task GetChildren_ReturnsAllItems()
    {
        var conductor = new Conductor<Screen>.Collection.OneActive();
        await ActivateAsync(conductor);
        var item1 = new Screen();
        var item2 = new Screen();
        await conductor.ActivateItemAsync(item1);
        await conductor.ActivateItemAsync(item2);

        var children = conductor.GetChildren();

        await Assert.That(children.Count).IsEqualTo(2);
    }

    [Test]
    public async Task CanCloseAsync_AllCanClose_ReturnsTrue()
    {
        var conductor = new Conductor<Screen>.Collection.OneActive();
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
        var conductor = new Conductor<Screen>.Collection.OneActive();
        await ActivateAsync(conductor);
        var item1 = new TestScreen { CanCloseResult = true };
        var item2 = new TestScreen { CanCloseResult = false };
        await conductor.ActivateItemAsync(item1);
        await conductor.ActivateItemAsync(item2);

        var result = await conductor.CanCloseAsync();

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task ConductorActivation_ActivatesActiveItem()
    {
        var conductor = new Conductor<Screen>.Collection.OneActive();
        var item = new Screen();
        conductor.Items.Add(item);
        // Manually set active by activating then deactivating conductor
        await ActivateAsync(conductor);
        await conductor.ActivateItemAsync(item);
        await DeactivateAsync(conductor, false);
        await Assert.That(item.IsActive).IsFalse();

        await ActivateAsync(conductor);

        await Assert.That(item.IsActive).IsTrue();
    }

    [Test]
    public async Task ConductorDeactivation_WithClose_ClearsItems()
    {
        var conductor = new Conductor<Screen>.Collection.OneActive();
        await ActivateAsync(conductor);
        var item1 = new Screen();
        var item2 = new Screen();
        await conductor.ActivateItemAsync(item1);
        await conductor.ActivateItemAsync(item2);

        await DeactivateAsync(conductor, true);

        await Assert.That(conductor.Items.Count).IsEqualTo(0);
    }

    [Test]
    public async Task EnsureItem_SetsParentOnIChild()
    {
        var conductor = new Conductor<Screen>.Collection.OneActive();
        await ActivateAsync(conductor);
        var item = new TestScreen();

        await conductor.ActivateItemAsync(item);

        await Assert.That(item.Parent).IsEqualTo(conductor);
    }

    [Test]
    public async Task CloseActiveItem_SelectsNextItem()
    {
        var conductor = new Conductor<Screen>.Collection.OneActive();
        await ActivateAsync(conductor);

        var s1 = new Screen();
        var s2 = new Screen();
        var s3 = new Screen();

        await conductor.ActivateItemAsync(s1);
        await conductor.ActivateItemAsync(s2);
        await conductor.ActivateItemAsync(s3);

        // Close s3 (active), should select s2 or another
        await conductor.DeactivateItemAsync(s3, true);
        await Assert.That(conductor.ActiveItem).IsNotNull();
        await Assert.That(conductor.ActiveItem!.IsActive).IsTrue();
    }
}
