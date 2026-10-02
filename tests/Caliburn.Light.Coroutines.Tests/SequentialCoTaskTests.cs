using System.Collections.Generic;
using Caliburn.Light;

namespace Caliburn.Light.Coroutines.Tests;

public class SequentialCoTaskTests
{
    [Test]
    public async Task SequentialCoroutine_EmptySequence_Completes()
    {
        static IEnumerable<ICoTask> Sequence()
        {
            yield break;
        }

        await Coroutine.From(Sequence().GetEnumerator()).ExecuteAsync();
    }

    [Test]
    public async Task SequentialCoroutine_ExecutesStepsInOrder()
    {
        var steps = new List<int>();
        IEnumerator<ICoTask> sequence()
        {
            yield return Coroutine.From(() => steps.Add(1));
            yield return Coroutine.From(() => steps.Add(2));
        }

        await Coroutine.From(sequence()).ExecuteAsync();

        await Assert.That(steps.Count).IsEqualTo(2);
        await Assert.That(steps[0]).IsEqualTo(1);
        await Assert.That(steps[1]).IsEqualTo(2);
    }

    [Test]
    public async Task SequentialCoroutine_FailedStep_StopsFollowingSteps()
    {
        var executed = false;
        IEnumerator<ICoTask> sequence()
        {
            yield return SimpleCoTask.Failed(new InvalidOperationException());
            yield return Coroutine.From(() => executed = true);
        }

        await Assert.That(async () => await Coroutine.From(sequence()).ExecuteAsync())
            .Throws<InvalidOperationException>();

        await Assert.That(executed).IsFalse();
    }

    [Test]
    public async Task SequentialCoroutine_CancelledStep_CancelsSequence()
    {
        static IEnumerable<ICoTask> Sequence()
        {
            yield return SimpleCoTask.Cancelled();
            throw new InvalidOperationException("The sequence should stop.");
        }

        var task = Coroutine.From(Sequence().GetEnumerator()).ExecuteAsync();

        await Assert.That(task.IsCanceled).IsTrue();
    }

    [Test]
    public async Task SequentialCoroutine_NullStep_SkipsToFollowingStep()
    {
        var executed = false;
        static IEnumerable<ICoTask> Sequence(Action action)
        {
            yield return null!;
            yield return Coroutine.From(action);
        }

        await Coroutine.From(Sequence(() => executed = true).GetEnumerator()).ExecuteAsync();

        await Assert.That(executed).IsTrue();
    }

    [Test]
    public async Task SequentialCoroutine_EnumeratorException_Propagates()
    {
        await Assert.That(async () => await Coroutine.From(new ThrowingEnumerator()).ExecuteAsync())
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task SequentialCoroutine_AsynchronousChild_ResumesSequence()
    {
        var first = new TestCoTask();
        var secondExecuted = false;
        static IEnumerable<ICoTask> Sequence(TestCoTask first, Action second)
        {
            yield return first;
            yield return Coroutine.From(second);
        }

        var task = Coroutine.From(Sequence(first, () => secondExecuted = true).GetEnumerator()).ExecuteAsync();
        first.Complete();

        await task;
        await Assert.That(secondExecuted).IsTrue();
    }
}
