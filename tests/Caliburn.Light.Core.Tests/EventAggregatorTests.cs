using System.Runtime.CompilerServices;
using Caliburn.Light;
using TUnit.Core;

namespace Caliburn.Light.Core.Tests;

public record MessageA(string Value);
public record MessageB(int Value);

public class TestTarget
{
    public List<object> ReceivedMessages { get; } = [];
}

[NotInParallel("StaticExecutingEvent")]
public class EventAggregatorTests
{
    [Test]
    public async Task Subscribe_WithValidArgs_ReturnsHandler()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();

        var handler = ea.Subscribe<TestTarget, MessageA>(target, (t, m) => t.ReceivedMessages.Add(m));

        await Assert.That(handler).IsNotNull();
        await Assert.That(handler!.IsDead).IsFalse();
    }

    [Test]
    public async Task Subscribe_NullTarget_ThrowsArgumentNullException()
    {
        var ea = new EventAggregator();

        var action = () => ea.Subscribe<TestTarget, MessageA>(null!, (t, m) => { });

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    [Test]
    public async Task Subscribe_NullHandler_ThrowsArgumentNullException()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();

        var action = () => ea.Subscribe<TestTarget, MessageA>(target, (Action<TestTarget, MessageA>)null!);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    [Test]
    public async Task Subscribe_NullAsyncHandler_ThrowsArgumentNullException()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();

        var action = () => ea.Subscribe<TestTarget, MessageA>(target, (Func<TestTarget, MessageA, Task>)null!);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    [Test]
    public async Task Publish_MatchingMessageType_DeliversToSubscriber()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();
        ea.Subscribe<TestTarget, MessageA>(target, (t, m) => t.ReceivedMessages.Add(m));
        var msg = new MessageA("hello");

        ea.Publish(msg);

        await Assert.That(target.ReceivedMessages.Count).IsEqualTo(1);
        await Assert.That(target.ReceivedMessages[0]).IsEqualTo(msg);
    }

    [Test]
    public async Task Publish_NonMatchingMessageType_DoesNotDeliver()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();
        ea.Subscribe<TestTarget, MessageA>(target, (t, m) => t.ReceivedMessages.Add(m));

        ea.Publish(new MessageB(42));

        await Assert.That(target.ReceivedMessages.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Publish_MultipleSubscribers_AllReceiveMessage()
    {
        var ea = new EventAggregator();
        var target1 = new TestTarget();
        var target2 = new TestTarget();
        ea.Subscribe<TestTarget, MessageA>(target1, (t, m) => t.ReceivedMessages.Add(m));
        ea.Subscribe<TestTarget, MessageA>(target2, (t, m) => t.ReceivedMessages.Add(m));
        var msg = new MessageA("broadcast");

        ea.Publish(msg);

        await Assert.That(target1.ReceivedMessages.Count).IsEqualTo(1);
        await Assert.That(target2.ReceivedMessages.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Unsubscribe_StopsDelivery()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();
        var handler = ea.Subscribe<TestTarget, MessageA>(target, (t, m) => t.ReceivedMessages.Add(m));

        ea.Unsubscribe(handler);
        ea.Publish(new MessageA("after-unsub"));

        await Assert.That(target.ReceivedMessages.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Unsubscribe_SameHandlerTwice_DoesNotThrow()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();
        var handler = ea.Subscribe<TestTarget, MessageA>(target, (t, m) => t.ReceivedMessages.Add(m));

        ea.Unsubscribe(handler);
        ea.Unsubscribe(handler);

        await Assert.That(target.ReceivedMessages.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Unsubscribe_NullHandler_ThrowsArgumentNullException()
    {
        var ea = new EventAggregator();

        var action = () => ea.Unsubscribe(null!);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    [Test]
    public async Task Publish_NoSubscribers_DoesNotThrow()
    {
        var ea = new EventAggregator();

        var action = () => ea.Publish(new MessageA("lonely"));

        await Assert.That(action).ThrowsNothing();
    }

    [Test]
    public async Task Publish_NullMessage_ThrowsArgumentNullException()
    {
        var ea = new EventAggregator();

        var action = () => ea.Publish(null!);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    [Test]
    public async Task Subscribe_AsyncHandler_DeliversMessage()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();
        ea.Subscribe<TestTarget, MessageA>(target, (t, m) =>
        {
            t.ReceivedMessages.Add(m);
            return Task.CompletedTask;
        });
        var msg = new MessageA("async");

        ea.Publish(msg);

        await Assert.That(target.ReceivedMessages.Count).IsEqualTo(1);
        await Assert.That(target.ReceivedMessages[0]).IsEqualTo(msg);
    }

    [Test]
    public async Task Publish_AsyncHandler_RaisesExecutingEvent()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();
        var tcs = new TaskCompletionSource();
        bool executingRaised = false;

        void onExecuting(object? sender, TaskEventArgs e) => executingRaised = true;
        EventAggregator.Executing += onExecuting;

        try
        {
            ea.Subscribe<TestTarget, MessageA>(target, (t, m) =>
            {
                t.ReceivedMessages.Add(m);
                return tcs.Task;
            });

            ea.Publish(new MessageA("pending"));

            await Assert.That(executingRaised).IsTrue();

            tcs.SetResult();
        }
        finally
        {
            EventAggregator.Executing -= onExecuting;
        }
    }

    [Test]
    public async Task Publish_SyncHandler_DoesNotRaiseExecutingEvent()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();
        bool executingRaised = false;

        void onExecuting(object? sender, TaskEventArgs e) => executingRaised = true;
        EventAggregator.Executing += onExecuting;

        try
        {
            ea.Subscribe<TestTarget, MessageA>(target, (t, m) => t.ReceivedMessages.Add(m));

            ea.Publish(new MessageA("sync"));

            await Assert.That(executingRaised).IsFalse();
        }
        finally
        {
            EventAggregator.Executing -= onExecuting;
        }
    }

    [Test]
    public async Task Handler_CanHandle_MatchingType_ReturnsTrue()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();
        var handler = ea.Subscribe<TestTarget, MessageA>(target, (t, m) => t.ReceivedMessages.Add(m));

        var result = handler.CanHandle(new MessageA("test"));

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task Handler_CanHandle_NonMatchingType_ReturnsFalse()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();
        var handler = ea.Subscribe<TestTarget, MessageA>(target, (t, m) => t.ReceivedMessages.Add(m));

        var result = handler.CanHandle(new MessageB(1));

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task Handler_IsDead_AfterTargetGC_ReturnsTrue()
    {
        var ea = new EventAggregator();
        var handler = SubscribeWeakTarget(ea);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        await Assert.That(handler.IsDead).IsTrue();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static IEventAggregatorHandler SubscribeWeakTarget(EventAggregator ea)
    {
        var target = new TestTarget();
        return ea.Subscribe<TestTarget, MessageA>(target, (t, m) => t.ReceivedMessages.Add(m));
    }

    [Test]
    public async Task Publish_DeadHandler_DoesNotThrow()
    {
        var ea = new EventAggregator();
        SubscribeWeakTarget(ea);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var action = () => ea.Publish(new MessageA("to-dead"));

        await Assert.That(action).ThrowsNothing();
    }

    [Test]
    public async Task Subscribe_WithoutDispatcher_UsesCurrentThreadDispatcher()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();

        var handler = ea.Subscribe<TestTarget, MessageA>(target, (t, m) => t.ReceivedMessages.Add(m));

        await Assert.That(handler.Dispatcher).IsEqualTo(CurrentThreadDispatcher.Instance);
    }

    [Test]
    public async Task Subscribe_WithCustomDispatcher_UsesProvidedDispatcher()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();
        var customDispatcher = new TestDispatcher();

        var handler = ea.Subscribe<TestTarget, MessageA>(target, (t, m) => t.ReceivedMessages.Add(m), customDispatcher);

        await Assert.That(handler.Dispatcher).IsEqualTo(customDispatcher);
    }

    [Test]
    public async Task Subscribe_AsyncWithoutDispatcher_UsesCurrentThreadDispatcher()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();

        var handler = ea.Subscribe<TestTarget, MessageA>(target, (t, m) =>
        {
            t.ReceivedMessages.Add(m);
            return Task.CompletedTask;
        });

        await Assert.That(handler.Dispatcher).IsEqualTo(CurrentThreadDispatcher.Instance);
    }

    [Test]
    public async Task Publish_MultipleMessageTypes_OnlyMatchingHandlersReceive()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();
        ea.Subscribe<TestTarget, MessageA>(target, (t, m) => t.ReceivedMessages.Add(m));
        ea.Subscribe<TestTarget, MessageB>(target, (t, m) => t.ReceivedMessages.Add(m));

        ea.Publish(new MessageA("only-a"));

        await Assert.That(target.ReceivedMessages.Count).IsEqualTo(1);
        await Assert.That(target.ReceivedMessages[0]).IsTypeOf<MessageA>();
    }

    [Test]
    public async Task Publish_MultipleMessageTypes_BothTypesDelivered()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();
        ea.Subscribe<TestTarget, MessageA>(target, (t, m) => t.ReceivedMessages.Add(m));
        ea.Subscribe<TestTarget, MessageB>(target, (t, m) => t.ReceivedMessages.Add(m));

        ea.Publish(new MessageA("a"));
        ea.Publish(new MessageB(99));

        await Assert.That(target.ReceivedMessages.Count).IsEqualTo(2);
        await Assert.That(target.ReceivedMessages[0]).IsTypeOf<MessageA>();
        await Assert.That(target.ReceivedMessages[1]).IsTypeOf<MessageB>();
    }

    [Test]
    public async Task Handler_HandleAsync_DeadTarget_ReturnsCompletedTask()
    {
        var ea = new EventAggregator();
        var handler = SubscribeWeakTarget(ea);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var task = handler.HandleAsync(new MessageA("dead"));

        await Assert.That(task.IsCompleted).IsTrue();
    }

    [Test]
    public async Task Publish_CustomDispatcherWithCheckAccess_PublishesViaBeginInvoke()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();
        var dispatcher = new NonCurrentThreadDispatcher();
        ea.Subscribe<TestTarget, MessageA>(target, (t, m) => t.ReceivedMessages.Add(m), dispatcher);

        ea.Publish(new MessageA("dispatched"));

        await Assert.That(dispatcher.QueuedActions.Count).IsEqualTo(1);

        dispatcher.QueuedActions[0]();

        await Assert.That(target.ReceivedMessages.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Unsubscribe_OneOfMultiple_OtherStillReceives()
    {
        var ea = new EventAggregator();
        var target1 = new TestTarget();
        var target2 = new TestTarget();
        var handler1 = ea.Subscribe<TestTarget, MessageA>(target1, (t, m) => t.ReceivedMessages.Add(m));
        ea.Subscribe<TestTarget, MessageA>(target2, (t, m) => t.ReceivedMessages.Add(m));

        ea.Unsubscribe(handler1);
        ea.Publish(new MessageA("after-partial-unsub"));

        await Assert.That(target1.ReceivedMessages.Count).IsEqualTo(0);
        await Assert.That(target2.ReceivedMessages.Count).IsEqualTo(1);
    }
}

public class TestDispatcher : IDispatcher
{
    public bool CheckAccess() => true;
    public void BeginInvoke(Action action) => action();
}

public class NonCurrentThreadDispatcher : IDispatcher
{
    public List<Action> QueuedActions { get; } = [];
    public bool CheckAccess() => false;
    public void BeginInvoke(Action action) => QueuedActions.Add(action);
}
