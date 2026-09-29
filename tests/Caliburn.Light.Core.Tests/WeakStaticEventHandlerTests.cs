using System.ComponentModel;
using System.Runtime.CompilerServices;
using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

[NotInParallel("StaticExecutingEvent")]
public class WeakStaticEventHandlerTests
{
    [Test]
    public async Task RegisterAsyncCommandExecutingWeak_ReceivesEvents()
    {
        var tcs = new TaskCompletionSource();
        var subscriber = new ExecutingSubscriber();
        using var reg = WeakEventHandler.RegisterAsyncCommandExecutingWeak(subscriber,
            static (s, sender, e) => s.OnExecuting(sender, e));

        var command = new AsyncDelegateCommand(() => tcs.Task);
        command.Execute(null);

        await Assert.That(subscriber.CallCount).IsEqualTo(1);
        await Assert.That(subscriber.LastTask).IsNotNull();

        tcs.SetResult();
        await WaitForIsExecutingFalseAsync(command);
    }

    [Test]
    public async Task RegisterAsyncCommandExecutingWeak_Dispose_StopsEvents()
    {
        var tcs = new TaskCompletionSource();
        var subscriber = new ExecutingSubscriber();
        var reg = WeakEventHandler.RegisterAsyncCommandExecutingWeak(subscriber,
            static (s, sender, e) => s.OnExecuting(sender, e));

        reg.Dispose();

        var command = new AsyncDelegateCommand(() => tcs.Task);
        command.Execute(null);

        await Assert.That(subscriber.CallCount).IsEqualTo(0);

        tcs.SetResult();
        await WaitForIsExecutingFalseAsync(command);
    }

    [Test]
    public async Task RegisterAsyncCommandExecutingWeak_SubscriberCollected_DoesNotReceiveEvents()
    {
        var tcs = new TaskCompletionSource();
        var command = new AsyncDelegateCommand(() => tcs.Task);
        var weakRef = RegisterAndAbandonAsyncCommandSubscriber();

        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
        GC.WaitForPendingFinalizers();

        await Assert.That(weakRef.IsAlive).IsFalse();

        // Trigger the event — the dead handler must detach itself without throwing
        command.Execute(null);

        // Verify a new live subscriber still receives events
        var liveSubscriber = new ExecutingSubscriber();
        using var reg = WeakEventHandler.RegisterAsyncCommandExecutingWeak(liveSubscriber,
            static (s, sender, e) => s.OnExecuting(sender, e));

        var second = new TaskCompletionSource();
        var secondCommand = new AsyncDelegateCommand(() => second.Task);
        secondCommand.Execute(null);

        await Assert.That(liveSubscriber.CallCount).IsEqualTo(1);

        tcs.SetResult();
        second.SetResult();
        await WaitForIsExecutingFalseAsync(command);
        await WaitForIsExecutingFalseAsync(secondCommand);
    }

    [Test]
    public async Task RegisterEventAggregatorExecutingWeak_ReceivesEvents()
    {
        var subscriber = new ExecutingSubscriber();
        using var reg = WeakEventHandler.RegisterEventAggregatorExecutingWeak(subscriber,
            static (s, sender, e) => s.OnExecuting(sender, e));

        var ea = new EventAggregator();
        var target = new TestTarget();
        var tcs = new TaskCompletionSource();
        ea.Subscribe<TestTarget, MessageA>(target, (t, m) => { t.ReceivedMessages.Add(m); return tcs.Task; });

        ea.Publish(new MessageA("pending"));

        await Assert.That(subscriber.CallCount).IsEqualTo(1);
        await Assert.That(subscriber.LastTask).IsNotNull();

        tcs.SetResult();
    }

    [Test]
    public async Task RegisterEventAggregatorExecutingWeak_Dispose_StopsEvents()
    {
        var subscriber = new ExecutingSubscriber();
        var reg = WeakEventHandler.RegisterEventAggregatorExecutingWeak(subscriber,
            static (s, sender, e) => s.OnExecuting(sender, e));

        reg.Dispose();

        var ea = new EventAggregator();
        var target = new TestTarget();
        var tcs = new TaskCompletionSource();
        ea.Subscribe<TestTarget, MessageA>(target, (t, m) => { t.ReceivedMessages.Add(m); return tcs.Task; });

        ea.Publish(new MessageA("pending"));

        await Assert.That(subscriber.CallCount).IsEqualTo(0);

        tcs.SetResult();
    }

    [Test]
    public async Task RegisterEventAggregatorExecutingWeak_SubscriberCollected_DoesNotReceiveEvents()
    {
        var weakRef = RegisterAndAbandonEventAggregatorSubscriber();

        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
        GC.WaitForPendingFinalizers();

        await Assert.That(weakRef.IsAlive).IsFalse();

        // Trigger the event — the dead handler must detach itself without throwing
        PublishPending();

        // Verify a new live subscriber still receives events
        var liveSubscriber = new ExecutingSubscriber();
        using var reg = WeakEventHandler.RegisterEventAggregatorExecutingWeak(liveSubscriber,
            static (s, sender, e) => s.OnExecuting(sender, e));

        PublishPending();

        await Assert.That(liveSubscriber.CallCount).IsEqualTo(1);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference RegisterAndAbandonAsyncCommandSubscriber()
    {
        var subscriber = new ExecutingSubscriber();
        WeakEventHandler.RegisterAsyncCommandExecutingWeak(subscriber,
            static (s, sender, e) => s.OnExecuting(sender, e));
        return new WeakReference(subscriber);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference RegisterAndAbandonEventAggregatorSubscriber()
    {
        var subscriber = new ExecutingSubscriber();
        WeakEventHandler.RegisterEventAggregatorExecutingWeak(subscriber,
            static (s, sender, e) => s.OnExecuting(sender, e));
        return new WeakReference(subscriber);
    }

    private static void PublishPending()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();
        var tcs = new TaskCompletionSource();
        ea.Subscribe<TestTarget, MessageA>(target, (t, m) => { t.ReceivedMessages.Add(m); return tcs.Task; });
        ea.Publish(new MessageA("pending"));
        tcs.SetResult();
    }

    private static async Task WaitForIsExecutingFalseAsync(AsyncCommand command, int timeoutMs = 5000)
    {
        if (!command.IsExecuting) return;

        var tcs = new TaskCompletionSource();
        PropertyChangedEventHandler handler = null!;
        handler = (s, e) =>
        {
            if (e.PropertyName == "IsExecuting" && !command.IsExecuting)
            {
                command.PropertyChanged -= handler;
                tcs.TrySetResult();
            }
        };

        command.PropertyChanged += handler;

        if (!command.IsExecuting) tcs.TrySetResult();

        await tcs.Task.WaitAsync(TimeSpan.FromMilliseconds(timeoutMs));
    }

    private class ExecutingSubscriber
    {
        public int CallCount;
        public Task? LastTask;

        public void OnExecuting(object? sender, TaskEventArgs e)
        {
            CallCount++;
            LastTask = e.Task;
        }
    }
}
