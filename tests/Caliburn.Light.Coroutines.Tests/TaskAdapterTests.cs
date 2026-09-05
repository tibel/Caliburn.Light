using System.Threading;
using Caliburn.Light;

namespace Caliburn.Light.Coroutines.Tests;

public class TaskAdapterTests
{
    [Test]
    public async Task TaskAdapter_CompletesSuccessfulTask()
    {
        await Task.CompletedTask.AsCoTask().ExecuteAsync();
    }

    [Test]
    public async Task TaskAdapter_PropagatesTaskException()
    {
        await Assert.That(async () => await Task.FromException(new InvalidOperationException()).AsCoTask().ExecuteAsync())
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task TaskAdapter_PropagatesCancellation()
    {
        var task = Task.FromCanceled(new CancellationToken(true)).AsCoTask().ExecuteAsync();

        await Assert.That(task.IsCanceled).IsTrue();
    }

    [Test]
    public async Task GenericTaskAdapter_PropagatesTaskException()
    {
        await Assert.That(async () => await Task.FromException<int>(new InvalidOperationException()).AsCoTask().ExecuteAsync())
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task GenericTaskAdapter_PropagatesCancellation()
    {
        var task = Task.FromCanceled<int>(new CancellationToken(true)).AsCoTask().ExecuteAsync();

        await Assert.That(task.IsCanceled).IsTrue();
    }

    [Test]
    public async Task TaskAdapter_NullTask_ThrowsArgumentNullException()
    {
        await Assert.That(() => ((Task)null!).AsCoTask()).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task GenericTaskAdapter_NullTask_ThrowsArgumentNullException()
    {
        await Assert.That(() => ((Task<int>)null!).AsCoTask()).Throws<ArgumentNullException>();
    }
}
