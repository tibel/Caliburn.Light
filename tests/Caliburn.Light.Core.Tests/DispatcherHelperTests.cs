using Caliburn.Light;
using static Caliburn.Light.DispatcherHelper;

namespace Caliburn.Light.Core.Tests;

public class DispatcherHelperTests
{
    [Test]
    public async Task SwitchTo_ReturnsDispatcherAwaitable()
    {
        IDispatcher dispatcher = CurrentThreadDispatcher.Instance;

        var awaitable = dispatcher.SwitchTo();

        await Assert.That(awaitable).IsAssignableTo<DispatcherAwaitable>();
    }

    [Test]
    public async Task GetAwaiter_ReturnsSelf()
    {
        var awaitable = CurrentThreadDispatcher.Instance.SwitchTo();

        var awaiter = awaitable.GetAwaiter();

        await Assert.That(awaiter).IsEqualTo(awaitable);
    }

    [Test]
    public async Task IsCompleted_WhenHasAccess_ReturnsTrue()
    {
        var awaitable = CurrentThreadDispatcher.Instance.SwitchTo();

        var result = awaitable.IsCompleted;

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsCompleted_WhenNoAccess_ReturnsFalse()
    {
        var dispatcher = new NoAccessDispatcher();
        var awaitable = dispatcher.SwitchTo();

        var result = awaitable.IsCompleted;

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GetResult_DoesNotThrow()
    {
        var awaitable = CurrentThreadDispatcher.Instance.SwitchTo();
        var action = () => awaitable.GetResult();
        await Assert.That(action).ThrowsNothing();
    }

    [Test]
    public async Task OnCompleted_DispatchesAction()
    {
        var dispatched = false;
        var awaitable = CurrentThreadDispatcher.Instance.SwitchTo();

        awaitable.OnCompleted(() => dispatched = true);

        await Assert.That(dispatched).IsTrue();
    }

    [Test]
    public async Task CanBeAwaited()
    {
        var dispatcher = CurrentThreadDispatcher.Instance;

        await dispatcher.SwitchTo();
    }

    private sealed class NoAccessDispatcher : IDispatcher
    {
        public bool CheckAccess() => false;

        public void BeginInvoke(Action action) => action();
    }
}
