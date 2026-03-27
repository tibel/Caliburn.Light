using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

public class DefaultCloseStrategyTests
{
    [Test]
    public async Task ExecuteAsync_EmptyList_ReturnsCanClose()
    {
        var strategy = new DefaultCloseStrategy<Screen>();

        var result = await strategy.ExecuteAsync(Array.Empty<Screen>());

        await Assert.That(result.CanClose).IsTrue();
    }

    [Test]
    public async Task ExecuteAsync_NonCloseGuardItems_ReturnsCanClose()
    {
        var strategy = new DefaultCloseStrategy<object>();

        var result = await strategy.ExecuteAsync(new object[] { new object(), new object() });

        await Assert.That(result.CanClose).IsTrue();
    }

    [Test]
    public async Task ExecuteAsync_AllCanClose_ReturnsCanClose()
    {
        var strategy = new DefaultCloseStrategy<Screen>();
        var items = new Screen[]
        {
            new TestScreen { CanCloseResult = true },
            new TestScreen { CanCloseResult = true },
        };

        var result = await strategy.ExecuteAsync(items);

        await Assert.That(result.CanClose).IsTrue();
    }

    [Test]
    public async Task ExecuteAsync_SomeCannotClose_ReturnsCannotClose()
    {
        var strategy = new DefaultCloseStrategy<Screen>();
        var items = new Screen[]
        {
            new TestScreen { CanCloseResult = true },
            new TestScreen { CanCloseResult = false },
        };

        var result = await strategy.ExecuteAsync(items);

        await Assert.That(result.CanClose).IsFalse();
    }

    [Test]
    public async Task ExecuteAsync_AllCannotClose_ReturnsCannotClose()
    {
        var strategy = new DefaultCloseStrategy<Screen>();
        var items = new Screen[]
        {
            new TestScreen { CanCloseResult = false },
            new TestScreen { CanCloseResult = false },
        };

        var result = await strategy.ExecuteAsync(items);

        await Assert.That(result.CanClose).IsFalse();
    }

    [Test]
    public async Task ExecuteAsync_Default_ReturnsEmptyCloseables()
    {
        var strategy = new DefaultCloseStrategy<Screen>();
        var items = new Screen[]
        {
            new TestScreen { CanCloseResult = true },
            new TestScreen { CanCloseResult = false },
        };

        var result = await strategy.ExecuteAsync(items);

        await Assert.That(result.Closeables.Count).IsEqualTo(0);
    }

    [Test]
    public async Task ExecuteAsync_WithClosables_ReturnsCloseableItems()
    {
        var strategy = new DefaultCloseStrategy<Screen>(closeConductedItemsWhenConductorCannotClose: true);
        var closeable = new TestScreen { CanCloseResult = true };
        var nonCloseable = new TestScreen { CanCloseResult = false };
        var items = new Screen[] { closeable, nonCloseable };

        var result = await strategy.ExecuteAsync(items);

        await Assert.That(result.CanClose).IsFalse();
        await Assert.That(result.Closeables.Count).IsEqualTo(1);
        await Assert.That(result.Closeables[0]).IsEqualTo(closeable);
    }

    [Test]
    public async Task ExecuteAsync_WithClosables_AllCanClose_ReturnsAllAsCloseables()
    {
        var strategy = new DefaultCloseStrategy<Screen>(closeConductedItemsWhenConductorCannotClose: true);
        var item1 = new TestScreen { CanCloseResult = true };
        var item2 = new TestScreen { CanCloseResult = true };
        var items = new Screen[] { item1, item2 };

        var result = await strategy.ExecuteAsync(items);

        await Assert.That(result.CanClose).IsTrue();
        await Assert.That(result.Closeables.Count).IsEqualTo(2);
    }

    [Test]
    public async Task ExecuteAsync_WithClosables_NonCloseGuardItems_ReturnsAllAsCloseables()
    {
        var strategy = new DefaultCloseStrategy<object>(closeConductedItemsWhenConductorCannotClose: true);
        var items = new object[] { new object(), new object() };

        var result = await strategy.ExecuteAsync(items);

        await Assert.That(result.CanClose).IsTrue();
        await Assert.That(result.Closeables.Count).IsEqualTo(2);
    }

    [Test]
    public async Task ExecuteAsync_WithClosables_NoneCanClose_ReturnsEmptyCloseables()
    {
        var strategy = new DefaultCloseStrategy<Screen>(closeConductedItemsWhenConductorCannotClose: true);
        var items = new Screen[]
        {
            new TestScreen { CanCloseResult = false },
            new TestScreen { CanCloseResult = false },
        };

        var result = await strategy.ExecuteAsync(items);

        await Assert.That(result.CanClose).IsFalse();
        await Assert.That(result.Closeables.Count).IsEqualTo(0);
    }

    [Test]
    public async Task ExecuteAsync_SingleItem_CanClose_ReturnsTrue()
    {
        var strategy = new DefaultCloseStrategy<Screen>();
        var items = new Screen[] { new TestScreen { CanCloseResult = true } };

        var result = await strategy.ExecuteAsync(items);

        await Assert.That(result.CanClose).IsTrue();
    }

    [Test]
    public async Task ExecuteAsync_SingleItem_CannotClose_ReturnsFalse()
    {
        var strategy = new DefaultCloseStrategy<Screen>();
        var items = new Screen[] { new TestScreen { CanCloseResult = false } };

        var result = await strategy.ExecuteAsync(items);

        await Assert.That(result.CanClose).IsFalse();
    }
}
