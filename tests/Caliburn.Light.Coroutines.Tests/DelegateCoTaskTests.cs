using Caliburn.Light;

namespace Caliburn.Light.Coroutines.Tests;

public class DelegateCoTaskTests
{
    [Test]
    public async Task ActionAdapter_ExecutesAction()
    {
        var executed = false;

        await new Action(() => executed = true).AsCoTask().ExecuteAsync();

        await Assert.That(executed).IsTrue();
    }

    [Test]
    public async Task ActionAdapter_PropagatesActionException()
    {
        var error = new InvalidOperationException();

        await Assert.That(async () => await new Action(() => throw error).AsCoTask().ExecuteAsync())
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task FuncAdapter_PropagatesFunctionException()
    {
        var error = new InvalidOperationException();

        await Assert.That(async () => await new Func<int>(() => throw error).AsCoTask().ExecuteAsync())
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ActionAdapter_NullAction_ThrowsArgumentNullException()
    {
        await Assert.That(() => ((Action)null!).AsCoTask()).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task FuncAdapter_NullFunction_ThrowsArgumentNullException()
    {
        await Assert.That(() => ((Func<int>)null!).AsCoTask()).Throws<ArgumentNullException>();
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
