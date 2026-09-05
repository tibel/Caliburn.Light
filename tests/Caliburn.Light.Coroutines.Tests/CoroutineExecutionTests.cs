using Caliburn.Light;

namespace Caliburn.Light.Coroutines.Tests;

public class CoroutineExecutionTests
{
    [Test]
    public async Task ExecuteAsync_SucceededCoTask_Completes()
    {
        await SimpleCoTask.Succeeded().ExecuteAsync();
    }

    [Test]
    public async Task ExecuteAsync_FailedCoTask_ThrowsError()
    {
        var error = new InvalidOperationException("failure");

        await Assert.That(async () => await SimpleCoTask.Failed(error).ExecuteAsync())
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ExecuteAsync_CancelledCoTask_IsCanceled()
    {
        var task = SimpleCoTask.Cancelled().ExecuteAsync();

        await Assert.That(task.IsCanceled).IsTrue();
        await Assert.That(async () => await task).Throws<TaskCanceledException>();
    }

    [Test]
    public async Task ExecuteAsync_ResultCoTask_ReturnsResult()
    {
        var result = await new Func<int>(() => 42).AsCoTask().ExecuteAsync();

        await Assert.That(result).IsEqualTo(42);
    }

    [Test]
    public async Task ExecuteAsync_TaskAdapter_PropagatesResult()
    {
        var result = await Task.FromResult("result").AsCoTask().ExecuteAsync();

        await Assert.That(result).IsEqualTo("result");
    }
}
