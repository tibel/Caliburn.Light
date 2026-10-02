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
        var result = await Coroutine.From(() => 42).ExecuteAsync();

        await Assert.That(result).IsEqualTo(42);
    }

    [Test]
    public async Task ExecuteAsync_TaskAdapter_PropagatesResult()
    {
        var result = await Coroutine.From(() => Task.FromResult("result")).ExecuteAsync();

        await Assert.That(result).IsEqualTo("result");
    }

    [Test]
    public async Task FromTask_SuccessfulTask_Completes()
    {
        var executed = false;
        var coTask = Coroutine.From(() =>
        {
            executed = true;
            return Task.CompletedTask;
        });

        await coTask.ExecuteAsync();

        await Assert.That(executed).IsTrue();
    }

    [Test]
    public async Task FromTask_WithResult_ReturnsResult()
    {
        var coTask = Coroutine.From(() => Task.FromResult(42));
        var result = await coTask.ExecuteAsync();

        await Assert.That(result).IsEqualTo(42);
    }

    [Test]
    public async Task FromTask_FailedTask_Throws()
    {
        var error = new InvalidOperationException("fail");
        var coTask = Coroutine.From(() => Task.FromException(error));

        await Assert.That(async () => await coTask.ExecuteAsync())
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task FromTask_CancelledTask_IsCanceled()
    {
        var coTask = Coroutine.From(() => Task.FromCanceled(new System.Threading.CancellationToken(true)));
        var task = coTask.ExecuteAsync();

        await Assert.That(task.IsCanceled).IsTrue();
        await Assert.That(async () => await task).Throws<TaskCanceledException>();
    }

    [Test]
    public async Task FromTask_WithContext_PassesContext()
    {
        CommandExecutionContext? captured = null;
        var coTask = Coroutine.From(ctx =>
        {
            captured = ctx;
            return Task.CompletedTask;
        });

        var context = new CommandExecutionContext { Source = "src", Target = "tgt", EventArgs = "args" };
        await coTask.ExecuteAsync(context);

        await Assert.That(captured).IsNotNull();
        await Assert.That(captured!.Source).IsEqualTo("src");
        await Assert.That(captured.Target).IsEqualTo("tgt");
        await Assert.That(captured.EventArgs).IsEqualTo("args");
    }

    [Test]
    public async Task FromTask_ResultWithContext_PassesContext()
    {
        CommandExecutionContext? captured = null;
        var coTask = Coroutine.From(ctx =>
        {
            captured = ctx;
            return Task.FromResult(123);
        });

        await coTask.ExecuteAsync(new CommandExecutionContext { Target = "target" });

        await Assert.That(captured).IsNotNull();
        await Assert.That(captured!.Target).IsEqualTo("target");
    }

    [Test]
    public async Task FromTask_FactoryCalledOnlyOnBeginExecute()
    {
        var called = 0;
        var coTask = Coroutine.From(() =>
        {
            called++;
            return Task.CompletedTask;
        });

        await Assert.That(called).IsEqualTo(0);
        await coTask.ExecuteAsync();
        await Assert.That(called).IsEqualTo(1);
    }
}
