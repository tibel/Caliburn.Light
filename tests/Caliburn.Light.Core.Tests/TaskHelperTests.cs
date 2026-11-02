using System.Threading.Tasks;
using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

public class TaskHelperTests
{
    [Test]
    public async Task TrueTask_ReturnsTrue()
    {
        var result = await TaskHelper.TrueTask;
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task FalseTask_ReturnsFalse()
    {
        var result = await TaskHelper.FalseTask;
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task TrueTask_IsCompleted()
    {
        await Assert.That(TaskHelper.TrueTask.IsCompleted).IsTrue();
    }

    [Test]
    public async Task FalseTask_IsCompleted()
    {
        await Assert.That(TaskHelper.FalseTask.IsCompleted).IsTrue();
    }

    [Test]
    public async Task TrueTask_IsCached_ReturnsSameInstance()
    {
        var first = TaskHelper.TrueTask;
        var second = TaskHelper.TrueTask;
        await Assert.That(ReferenceEquals(first, second)).IsTrue();
    }

    [Test]
    public async Task FalseTask_IsCached_ReturnsSameInstance()
    {
        var first = TaskHelper.FalseTask;
        var second = TaskHelper.FalseTask;
        await Assert.That(ReferenceEquals(first, second)).IsTrue();
    }

    [Test]
    public async Task Observe_CompletedTask_DoesNotThrow()
    {
        var action = () => Task.CompletedTask.Observe();
        await Assert.That(action).ThrowsNothing();
    }

    [Test]
    public async Task Observe_DelayedTask_DoesNotPreventCompletion()
    {
        var tcs = new TaskCompletionSource();
        var task = tcs.Task;
        task.Observe();

        await Assert.That(task.IsCompleted).IsFalse();

        tcs.SetResult();

        await Assert.That(task.IsCompleted).IsTrue();
    }
}
