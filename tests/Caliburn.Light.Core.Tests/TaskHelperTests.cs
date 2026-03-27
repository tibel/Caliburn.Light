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

public class TaskEventArgsTests
{
    [Test]
    public async Task Constructor_NullTask_ThrowsArgumentNullException()
    {
        await Assert.That(() => new TaskEventArgs(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_ValidTask_StoresTask()
    {
        var task = Task.CompletedTask;
        var args = new TaskEventArgs(task);
        await Assert.That(args.Task).IsEqualTo(task);
    }

    [Test]
    public async Task Task_Property_ReturnsSameInstance()
    {
        var task = Task.FromResult(42);
        var args = new TaskEventArgs(task);
        await Assert.That(ReferenceEquals(args.Task, task)).IsTrue();
    }

    [Test]
    public async Task InheritsFromEventArgs()
    {
        var args = new TaskEventArgs(Task.CompletedTask);
        await Assert.That(args is EventArgs).IsTrue();
    }
}
