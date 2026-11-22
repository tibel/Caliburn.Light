using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

public class DeferredEventTests
{
    [Test]
    public async Task InvokeAsync_NoDeferrals_Completes()
    {
        var eventArgs = new DeferredEventArgs();
        var invoked = false;
        EventHandler<DeferredEventArgs> handler = (_, _) => invoked = true;

        await handler.InvokeAsync(null, eventArgs);

        await Assert.That(invoked).IsTrue();
    }

    [Test]
    public async Task InvokeAsync_DeferralCompletedDuringHandler_CompletesImmediately()
    {
        var eventArgs = new DeferredEventArgs();
        EventHandler<DeferredEventArgs> handler = (_, args) => args.GetDeferral().Complete();

        var invocation = handler.InvokeAsync(null, eventArgs);

        await Assert.That(invocation.IsCompleted).IsTrue();
        await invocation;
        await new EventHandler<DeferredEventArgs>((_, _) => { }).InvokeAsync(null, eventArgs);
    }

    [Test]
    public async Task InvokeAsync_MultipleDeferrals_WaitsForAll()
    {
        var eventArgs = new DeferredEventArgs();
        EventDeferral? first = null;
        EventDeferral? second = null;
        EventHandler<DeferredEventArgs> handler = (_, args) =>
        {
            first = args.GetDeferral();
            second = args.GetDeferral();
        };

        var invocation = handler.InvokeAsync(null, eventArgs);
        first!.Complete();

        await Assert.That(invocation.IsCompleted).IsFalse();

        second!.Complete();
        await invocation;
    }

    [Test]
    public async Task GetDeferral_OutsideInvocation_Throws()
    {
        var eventArgs = new DeferredEventArgs();

        await Assert.That(() => eventArgs.GetDeferral()).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task EventDeferral_CompleteCalledTwice_CompletesOnce()
    {
        var eventArgs = new DeferredEventArgs();
        EventDeferral? deferral = null;
        EventHandler<DeferredEventArgs> handler = (_, args) => deferral = args.GetDeferral();

        var invocation = handler.InvokeAsync(null, eventArgs);
        deferral!.Complete();
        deferral.Complete();

        await invocation;
    }

    [Test]
    public async Task EventDeferral_CompleteCalledConcurrently_CompletesOnce()
    {
        var eventArgs = new DeferredEventArgs();
        EventDeferral? deferral = null;
        EventHandler<DeferredEventArgs> handler = (_, args) => deferral = args.GetDeferral();

        var invocation = handler.InvokeAsync(null, eventArgs);
        Parallel.For(0, 100, _ => deferral!.Complete());

        await invocation;
    }

    [Test]
    public async Task EventDeferral_Disposed_CompletesInvocation()
    {
        var eventArgs = new DeferredEventArgs();
        EventDeferral? deferral = null;
        EventHandler<DeferredEventArgs> handler = (_, args) => deferral = args.GetDeferral();

        var invocation = handler.InvokeAsync(null, eventArgs);
        deferral!.Dispose();

        await invocation;
    }

    [Test]
    public async Task InvokeAsync_CancellationRequested_CancelsWait()
    {
        var eventArgs = new DeferredEventArgs();
        EventDeferral? deferral = null;
        EventHandler<DeferredEventArgs> handler = (_, args) => deferral = args.GetDeferral();
        using var cancellationTokenSource = new CancellationTokenSource();

        var invocation = handler.InvokeAsync(null, eventArgs, cancellationTokenSource.Token);
        var eventDeferral = deferral!;
        await Assert.That(eventDeferral.CancellationToken).IsEqualTo(cancellationTokenSource.Token);
        cancellationTokenSource.Cancel();

        await Assert.That(eventDeferral.CancellationToken.IsCancellationRequested).IsTrue();
        await Assert.That(invocation.IsCanceled).IsTrue();
        eventDeferral.Complete();
        await new EventHandler<DeferredEventArgs>((_, _) => { }).InvokeAsync(null, eventArgs);
    }

    [Test]
    public async Task InvokeAsync_TokenAlreadyCanceled_DoesNotInvokeHandler()
    {
        var eventArgs = new DeferredEventArgs();
        var handlerInvoked = false;
        EventHandler<DeferredEventArgs> handler = (_, _) => handlerInvoked = true;
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        var invocation = handler.InvokeAsync(null, eventArgs, cancellationTokenSource.Token);

        await Assert.That(invocation.IsCanceled).IsTrue();
        await Assert.That(handlerInvoked).IsFalse();
        await new EventHandler<DeferredEventArgs>((_, _) => { }).InvokeAsync(null, eventArgs);
    }

    [Test]
    public async Task InvokeAsync_HandlerThrows_FaultsAndRetainsPendingDeferral()
    {
        var eventArgs = new DeferredEventArgs();
        EventDeferral? deferral = null;
        EventHandler<DeferredEventArgs> handler = (_, args) =>
        {
            deferral = args.GetDeferral();
            throw new InvalidOperationException();
        };

        var invocation = handler.InvokeAsync(null, eventArgs);
        await Assert.That(async () => await invocation).Throws<InvalidOperationException>();
        await Assert.That(() => eventArgs.GetDeferral()).Throws<InvalidOperationException>();

        deferral!.Complete();
        await new EventHandler<DeferredEventArgs>((_, _) => { }).InvokeAsync(null, eventArgs);
    }
}
