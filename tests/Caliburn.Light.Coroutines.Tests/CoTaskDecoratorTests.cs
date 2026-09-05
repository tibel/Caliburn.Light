using Caliburn.Light;

namespace Caliburn.Light.Coroutines.Tests;

public class CoTaskDecoratorTests
{
    [Test]
    public async Task WhenCancelled_ExecutesFallbackAndPreservesCancellation()
    {
        var fallbackExecuted = false;

        var task = SimpleCoTask.Cancelled()
            .WhenCancelled(() =>
            {
                fallbackExecuted = true;
                return SimpleCoTask.Succeeded();
            })
            .ExecuteAsync();

        await Assert.That(task.IsCanceled).IsTrue();
        await Assert.That(fallbackExecuted).IsTrue();
    }

    [Test]
    public async Task WhenCancelled_NonCancelledInner_DoesNotExecuteFallback()
    {
        var fallbackExecuted = false;

        await SimpleCoTask.Succeeded()
            .WhenCancelled(() =>
            {
                fallbackExecuted = true;
                return SimpleCoTask.Succeeded();
            })
            .ExecuteAsync();

        await Assert.That(fallbackExecuted).IsFalse();
    }

    [Test]
    public async Task WhenCancelled_FallbackException_PropagatesException()
    {
        var error = new InvalidOperationException();

        await Assert.That(async () => await SimpleCoTask.Cancelled()
                .WhenCancelled(() => SimpleCoTask.Failed(error))
                .ExecuteAsync())
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task WhenCancelled_FallbackDelegateThrows_PropagatesException()
    {
        var error = new InvalidOperationException();

        await Assert.That(async () => await SimpleCoTask.Cancelled()
                .WhenCancelled(() => throw error)
                .ExecuteAsync())
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task WhenCancelled_FallbackReturnsNull_ThrowsInvalidOperationException()
    {
        await Assert.That(async () => await SimpleCoTask.Cancelled()
                .WhenCancelled(() => null!)
                .ExecuteAsync())
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task OverrideCancel_ReturnsConfiguredResult()
    {
        var result = await new ResultCoTask<int>(SimpleCoTask.Cancelled(), 7)
            .OverrideCancel(42)
            .ExecuteAsync();

        await Assert.That(result).IsEqualTo(42);
    }

    [Test]
    public async Task OverrideCancel_NonGeneric_ClearsCancellation()
    {
        var task = SimpleCoTask.Cancelled().OverrideCancel().ExecuteAsync();

        await task;
        await Assert.That(task.IsCanceled).IsFalse();
    }

    [Test]
    public async Task OverrideCancel_ResultSuccess_PreservesResult()
    {
        var result = await new ResultCoTask<int>(SimpleCoTask.Succeeded(), 7)
            .OverrideCancel(42)
            .ExecuteAsync();

        await Assert.That(result).IsEqualTo(7);
    }

    [Test]
    public async Task Rescue_MatchingException_ExecutesRecovery()
    {
        var recovered = false;

        var task = SimpleCoTask.Failed(new InvalidOperationException())
            .Rescue<InvalidOperationException>(_ =>
            {
                recovered = true;
                return SimpleCoTask.Succeeded();
            }, cancelCoTask: false)
            .ExecuteAsync();

        await Assert.That(task.IsCompletedSuccessfully).IsTrue();
        await Assert.That(recovered).IsTrue();
    }

    [Test]
    public async Task Rescue_DefaultCancelOption_CancelsAfterRecovery()
    {
        var task = SimpleCoTask.Failed(new InvalidOperationException())
            .Rescue<InvalidOperationException>(_ => SimpleCoTask.Succeeded())
            .ExecuteAsync();

        await Assert.That(task.IsCanceled).IsTrue();
    }

    [Test]
    public async Task Rescue_NonMatchingException_PropagatesOriginalException()
    {
        await Assert.That(async () => await SimpleCoTask.Failed(new ArgumentException())
                .Rescue<InvalidOperationException>(_ => SimpleCoTask.Succeeded())
                .ExecuteAsync())
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task Rescue_RecoveryException_PropagatesException()
    {
        var error = new InvalidOperationException();

        await Assert.That(async () => await SimpleCoTask.Failed(new ArgumentException())
                .Rescue<ArgumentException>(_ => SimpleCoTask.Failed(error), cancelCoTask: false)
                .ExecuteAsync())
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Rescue_RecoveryDelegateThrows_PropagatesException()
    {
        var error = new InvalidOperationException();

        await Assert.That(async () => await SimpleCoTask.Failed(new ArgumentException())
                .Rescue<ArgumentException>(_ => throw error, cancelCoTask: false)
                .ExecuteAsync())
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Rescue_RecoveryReturnsNull_ThrowsInvalidOperationException()
    {
        await Assert.That(async () => await SimpleCoTask.Failed(new ArgumentException())
                .Rescue<ArgumentException>(_ => null!, cancelCoTask: false)
                .ExecuteAsync())
            .Throws<InvalidOperationException>();
    }

    private sealed class ResultCoTask<TResult> : ICoTask<TResult>
    {
        private readonly ICoTask _inner;

        public ResultCoTask(ICoTask inner, TResult result)
        {
            _inner = inner;
            Result = result;
        }

        public TResult Result { get; }

        public event EventHandler<CoTaskCompletedEventArgs>? Completed;

        public void BeginExecute(CommandExecutionContext context)
        {
            _inner.Completed += OnCompleted;
            _inner.BeginExecute(context);
        }

        private void OnCompleted(object? sender, CoTaskCompletedEventArgs args)
        {
            _inner.Completed -= OnCompleted;
            Completed?.Invoke(this, args);
        }
    }
}
