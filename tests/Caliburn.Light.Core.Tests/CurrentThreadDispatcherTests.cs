using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

public class CurrentThreadDispatcherTests
{
    [Test]
    public async Task Instance_CalledMultipleTimes_ReturnsSameSingleton()
    {
        var instance1 = CurrentThreadDispatcher.Instance;
        var instance2 = CurrentThreadDispatcher.Instance;

        await Assert.That(instance1).IsSameReferenceAs(instance2);
    }

    [Test]
    public async Task Instance_IsNotNull()
    {
        await Assert.That(CurrentThreadDispatcher.Instance).IsNotNull();
    }

    [Test]
    public async Task CheckAccess_Always_ReturnsTrue()
    {
        var dispatcher = CurrentThreadDispatcher.Instance;

        var result = dispatcher.CheckAccess();

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task BeginInvoke_WithAction_ExecutesImmediately()
    {
        var dispatcher = CurrentThreadDispatcher.Instance;
        var executed = false;

        dispatcher.BeginInvoke(() => executed = true);

        await Assert.That(executed).IsTrue();
    }

    [Test]
    public async Task BeginInvoke_WithAction_ExecutesSynchronously()
    {
        var dispatcher = CurrentThreadDispatcher.Instance;
        var order = new List<int>();

        dispatcher.BeginInvoke(() => order.Add(1));
        order.Add(2);

        await Assert.That(order[0]).IsEqualTo(1);
        await Assert.That(order[1]).IsEqualTo(2);
    }

    [Test]
    public async Task Instance_ImplementsIDispatcher()
    {
        await Assert.That(CurrentThreadDispatcher.Instance).IsAssignableTo<IDispatcher>();
    }
}
