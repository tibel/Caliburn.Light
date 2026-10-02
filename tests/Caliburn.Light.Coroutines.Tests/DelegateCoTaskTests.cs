using Caliburn.Light;

namespace Caliburn.Light.Coroutines.Tests;

public class DelegateCoTaskTests
{
    [Test]
    public async Task ActionAdapter_ExecutesAction()
    {
        var executed = false;

        await Coroutine.From(() => executed = true).ExecuteAsync();

        await Assert.That(executed).IsTrue();
    }

    [Test]
    public async Task ActionAdapter_PropagatesActionException()
    {
        var error = new InvalidOperationException();

        await Assert.That(async () => await Coroutine.From(() => throw error).ExecuteAsync())
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task FuncAdapter_PropagatesFunctionException()
    {
        var error = new InvalidOperationException();

        await Assert.That(async () => await Coroutine.From(() => throw error).ExecuteAsync())
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ActionAdapter_NullAction_ThrowsArgumentNullException()
    {
        await Assert.That(() => Coroutine.From((Action)null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task FuncAdapter_NullFunction_ThrowsArgumentNullException()
    {
        await Assert.That(() => Coroutine.From((Func<int>)null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task SimpleCoTask_BeginExecute_RaisesExpectedCompletion()
    {
        CoTaskCompletedEventArgs? completed = null;
        var coTask = SimpleCoTask.Failed(new InvalidOperationException());
        coTask.Completed += (_, args) => completed = args;

        coTask.BeginExecute(new CommandExecutionContext());

        await Assert.That(completed).IsNotNull();
        await Assert.That(completed!.Error).IsTypeOf<InvalidOperationException>();
        await Assert.That(completed.WasCancelled).IsFalse();
    }

    [Test]
    public async Task SimpleCoTask_Cancelled_RaisesCancellation()
    {
        CoTaskCompletedEventArgs? completed = null;
        var coTask = SimpleCoTask.Cancelled();
        coTask.Completed += (_, args) => completed = args;

        coTask.BeginExecute(new CommandExecutionContext());

        await Assert.That(completed).IsNotNull();
        await Assert.That(completed!.Error).IsNull();
        await Assert.That(completed.WasCancelled).IsTrue();
    }
}
