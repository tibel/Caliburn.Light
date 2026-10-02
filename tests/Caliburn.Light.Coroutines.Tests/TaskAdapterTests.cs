using System.Threading;
using Caliburn.Light;

namespace Caliburn.Light.Coroutines.Tests;

public class TaskAdapterTests
{
    [Test]
    public async Task TaskAdapter_CompletesSuccessfulTask()
    {
        await Coroutine.From(() => Task.CompletedTask).ExecuteAsync();
    }

    [Test]
    public async Task TaskAdapter_PropagatesTaskException()
    {
        await Assert.That(async () => await Coroutine.From(() => Task.FromException(new InvalidOperationException())).ExecuteAsync())
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task TaskAdapter_PropagatesCancellation()
    {
        var task = Coroutine.From(() => Task.FromCanceled(new CancellationToken(true))).ExecuteAsync();

        await Assert.That(task.IsCanceled).IsTrue();
    }

    [Test]
    public async Task GenericTaskAdapter_PropagatesTaskException()
    {
        await Assert.That(async () => await Coroutine.From(() => Task.FromException<int>(new InvalidOperationException())).ExecuteAsync())
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task GenericTaskAdapter_PropagatesCancellation()
    {
        var task = Coroutine.From(() => Task.FromCanceled<int>(new CancellationToken(true))).ExecuteAsync();

        await Assert.That(task.IsCanceled).IsTrue();
    }

    [Test]
    public async Task TaskAdapter_NullTask_ThrowsArgumentNullException()
    {
        await Assert.That(() => Coroutine.From((Func<Task>)null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task GenericTaskAdapter_NullTask_ThrowsArgumentNullException()
    {
        await Assert.That(() => Coroutine.From((Func<Task<int>>)null!)).Throws<ArgumentNullException>();
    }
}
