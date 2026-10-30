using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

[Obsolete("Tests for the obsolete DispatcherHelper API.")]
public class DispatcherHelperTests
{
    [Test]
    public async Task SwitchTo_ReturnsDispatcherAwaitable()
    {
        IDispatcher dispatcher = CurrentThreadDispatcher.Instance;

        var awaitable = DispatcherHelper.SwitchTo(dispatcher);

        await Assert.That(awaitable).IsAssignableTo<DispatcherHelper.DispatcherAwaitable>();
    }

    [Test]
    public async Task GetAwaiter_ReturnsSelf()
    {
        var awaitable = DispatcherHelper.SwitchTo(CurrentThreadDispatcher.Instance);

        var awaiter = awaitable.GetAwaiter();

        await Assert.That(awaiter).IsEqualTo(awaitable);
    }

    [Test]
    public async Task IsCompleted_WhenHasAccess_ReturnsTrue()
    {
        var awaitable = DispatcherHelper.SwitchTo(CurrentThreadDispatcher.Instance);

        var result = awaitable.IsCompleted;

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsCompleted_WhenNoAccess_ReturnsFalse()
    {
        var dispatcher = new NoAccessDispatcher();
        var awaitable = DispatcherHelper.SwitchTo(dispatcher);

        var result = awaitable.IsCompleted;

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GetResult_DoesNotThrow()
    {
        var awaitable = DispatcherHelper.SwitchTo(CurrentThreadDispatcher.Instance);
        var action = () => awaitable.GetResult();
        await Assert.That(action).ThrowsNothing();
    }

    [Test]
    public async Task OnCompleted_DispatchesAction()
    {
        var dispatched = false;
        var awaitable = DispatcherHelper.SwitchTo(CurrentThreadDispatcher.Instance);

        awaitable.OnCompleted(() => dispatched = true);

        await Assert.That(dispatched).IsTrue();
    }

    [Test]
    public async Task CanBeAwaited()
    {
        var dispatcher = CurrentThreadDispatcher.Instance;

        await DispatcherHelper.SwitchTo(dispatcher);
    }

    private sealed class NoAccessDispatcher : IDispatcher
    {
        public bool CheckAccess() => false;

        public void BeginInvoke(Action action) => action();
    }
}
