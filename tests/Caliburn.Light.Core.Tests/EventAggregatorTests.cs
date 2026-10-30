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

[NotInParallel("SharedStaticEventAndViewHelper")]
public class EventAggregatorTests
{
    [Before(Test)]
    public void Setup()
    {
        ViewHelper.Reset();
    }

    [After(Test)]
    public void Cleanup()
    {
        ViewHelper.Reset();
    }

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
    public async Task Unsubscribe_UnknownHandler_DoesNotAffectSubscribers()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();
        ea.Subscribe<TestTarget, MessageA>(target, (t, m) => t.ReceivedMessages.Add(m));
        var otherAggregator = new EventAggregator();
        var unknownHandler = otherAggregator.Subscribe<TestTarget, MessageA>(target, (t, m) => t.ReceivedMessages.Add(m));

        ea.Unsubscribe(unknownHandler);
        ea.Publish(new MessageA("still-subscribed"));

        await Assert.That(target.ReceivedMessages.Count).IsEqualTo(1);
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
    public async Task Subscribe_WithoutThreadOption_UsesPublisher()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();

        var handler = ea.Subscribe<TestTarget, MessageA>(target, (t, m) => t.ReceivedMessages.Add(m));

        await Assert.That(handler.ThreadOption).IsEqualTo(ThreadOption.PublisherThread);
    }

    [Test]
    public async Task Subscribe_AsyncWithoutThreadOption_UsesPublisher()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();

        var handler = ea.Subscribe<TestTarget, MessageA>(target, (t, m) =>
        {
            t.ReceivedMessages.Add(m);
            return Task.CompletedTask;
        });

        await Assert.That(handler.ThreadOption).IsEqualTo(ThreadOption.PublisherThread);
    }

    [Test]
    public async Task Subscribe_InvalidThreadOption_Throws()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();

        await Assert.That(() => ea.Subscribe<TestTarget, MessageA>(
                target,
                (t, m) => t.ReceivedMessages.Add(m),
                (ThreadOption)3))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task Subscribe_AsyncInvalidThreadOption_Throws()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();

        await Assert.That(() => ea.Subscribe<TestTarget, MessageA>(
                target,
                (t, m) =>
                {
                    t.ReceivedMessages.Add(m);
                    return Task.CompletedTask;
                },
                (ThreadOption)(-1)))
            .Throws<ArgumentOutOfRangeException>();
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
    public async Task Publish_UIThread_WhenPublisherHasViewAccess_RunsInline()
    {
        var context = new TestViewContext { IsCurrent = true };
        var view = new TestView(context);
        var target = new TestViewAwareTarget();
        ((IViewAware)target).AttachView(view, null);
        ViewHelper.Initialize(new TestViewAdapter());
        var ea = new EventAggregator();
        ea.Subscribe<TestViewAwareTarget, MessageA>(target, (t, m) => t.ReceivedMessages.Add(m), ThreadOption.UIThread);

        ea.Publish(new MessageA("inline"));

        await Assert.That(target.ReceivedMessages.Count).IsEqualTo(1);
        await Assert.That(context.QueuedActions.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Publish_UIThread_ViewAwareWithMultipleViews_UsesFirstAttachedView()
    {
        ViewHelper.Initialize(new TestViewAdapter());
        var firstContext = new TestViewContext();
        var secondContext = new TestViewContext();
        var target = new TestViewAwareTarget();
        var firstView = new TestView(firstContext);
        var secondView = new TestView(secondContext);
        ((IViewAware)target).AttachView(firstView, "first");
        ((IViewAware)target).AttachView(secondView, "second");
        var ea = new EventAggregator();
        ea.Subscribe<TestViewAwareTarget, MessageA>(target, (t, m) => t.ReceivedMessages.Add(m), ThreadOption.UIThread);

        ea.Publish(new MessageA("first-view"));

        await Assert.That(firstContext.QueuedActions.Count).IsEqualTo(1);
        await Assert.That(secondContext.QueuedActions.Count).IsEqualTo(0);
        firstContext.RunNext();
        await Assert.That(target.ReceivedMessages.Count).IsEqualTo(1);
        GC.KeepAlive(firstView);
        GC.KeepAlive(secondView);
    }

    [Test]
    public async Task Publish_UIThread_CustomViewAware_UsesGetViewsFallback()
    {
        ViewHelper.Initialize(new TestViewAdapter());
        var context = new TestViewContext();
        var target = new CustomViewAwareTarget(new TestView(context));
        var ea = new EventAggregator();
        ea.Subscribe<CustomViewAwareTarget, MessageA>(target, (t, m) => t.ReceivedMessages.Add(m), ThreadOption.UIThread);

        ea.Publish(new MessageA("custom-view-aware"));

        await Assert.That(context.QueuedActions.Count).IsEqualTo(1);
        context.RunNext();
        await Assert.That(target.ReceivedMessages.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Publish_UIThread_ViewsOnSameThread_SharesOneContinuation()
    {
        var context = new TestViewContext();
        var view1 = new TestView(context);
        var view2 = new TestView(context);
        var target1 = new TestViewAwareTarget();
        var target2 = new TestViewAwareTarget();
        ((IViewAware)target1).AttachView(view1, null);
        ((IViewAware)target2).AttachView(view2, null);
        ViewHelper.Initialize(new TestViewAdapter());
        var ea = new EventAggregator();
        ea.Subscribe<TestViewAwareTarget, MessageA>(target1, (t, m) => t.ReceivedMessages.Add(m), ThreadOption.UIThread);
        ea.Subscribe<TestViewAwareTarget, MessageA>(target2, (t, m) => t.ReceivedMessages.Add(m), ThreadOption.UIThread);

        ea.Publish(new MessageA("batched"));

        await Assert.That(context.QueuedActions.Count).IsEqualTo(1);
        await Assert.That(target1.ReceivedMessages.Count).IsEqualTo(0);
        context.RunNext();
        await Assert.That(target1.ReceivedMessages.Count).IsEqualTo(1);
        await Assert.That(target2.ReceivedMessages.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Publish_UIThread_InterleavedViews_BatchesEachThreadOnce()
    {
        var contextA = new TestViewContext();
        var contextB = new TestViewContext();
        var targetA1 = new TestViewAwareTarget();
        var targetB = new TestViewAwareTarget();
        var targetA2 = new TestViewAwareTarget();
        ((IViewAware)targetA1).AttachView(new TestView(contextA), null);
        ((IViewAware)targetB).AttachView(new TestView(contextB), null);
        ((IViewAware)targetA2).AttachView(new TestView(contextA), null);
        ViewHelper.Initialize(new TestViewAdapter());
        var ea = new EventAggregator();
        ea.Subscribe<TestViewAwareTarget, MessageA>(targetA1, (t, m) => t.ReceivedMessages.Add(m), ThreadOption.UIThread);
        ea.Subscribe<TestViewAwareTarget, MessageA>(targetB, (t, m) => t.ReceivedMessages.Add(m), ThreadOption.UIThread);
        ea.Subscribe<TestViewAwareTarget, MessageA>(targetA2, (t, m) => t.ReceivedMessages.Add(m), ThreadOption.UIThread);

        ea.Publish(new MessageA("interleaved"));

        await Assert.That(contextA.QueuedActions.Count).IsEqualTo(1);
        await Assert.That(contextB.QueuedActions.Count).IsEqualTo(0);
        contextA.RunNext();
        await Assert.That(targetA1.ReceivedMessages.Count).IsEqualTo(1);
        await Assert.That(targetA2.ReceivedMessages.Count).IsEqualTo(1);
        await Assert.That(contextB.QueuedActions.Count).IsEqualTo(1);
        contextB.RunNext();
        await Assert.That(targetB.ReceivedMessages.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Publish_UIThread_TargetIsView_UsesTargetView()
    {
        ViewHelper.Initialize(new TestViewAdapter());
        var context = new TestViewContext();
        var view = new TestView(context);
        var ea = new EventAggregator();
        ea.Subscribe<TestView, MessageA>(view, (target, message) => target.ReceivedMessages.Add(message), ThreadOption.UIThread);

        ea.Publish(new MessageA("target-view"));

        await Assert.That(context.QueuedActions.Count).IsEqualTo(1);
        context.RunNext();
        await Assert.That(view.ReceivedMessages.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Publish_UIThread_FindsViewThroughParentChain()
    {
        ViewHelper.Initialize(new TestViewAdapter());
        var context = new TestViewContext();
        var view = new TestView(context);
        var parent = new TestParentAwareTarget();
        ((IViewAware)parent).AttachView(view, null);
        var target = new TestParentAwareTarget();
        ((IParentAware)target).AttachParent(parent);
        var ea = new EventAggregator();
        ea.Subscribe<TestParentAwareTarget, MessageA>(target, (t, m) => t.ReceivedMessages.Add(m), ThreadOption.UIThread);

        ea.Publish(new MessageA("parent-view"));

        await Assert.That(context.QueuedActions.Count).IsEqualTo(1);
        context.RunNext();
        await Assert.That(target.ReceivedMessages.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Publish_UIThread_WithoutView_FallsBackToPublisher()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();
        ea.Subscribe<TestTarget, MessageA>(target, (t, m) => t.ReceivedMessages.Add(m), ThreadOption.UIThread);

        ea.Publish(new MessageA("fallback"));

        await Assert.That(target.ReceivedMessages.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Publish_BackgroundThread_OnThreadPool_RunsInline()
    {
        var ea = new EventAggregator();
        var target = new TestTarget();
        var publisherThreadId = 0;
        var handlerThreadId = 0;
        ea.Subscribe<TestTarget, MessageA>(target, (_, _) =>
        {
            handlerThreadId = Environment.CurrentManagedThreadId;
        }, ThreadOption.BackgroundThread);

        await Task.Run(() =>
        {
            publisherThreadId = Environment.CurrentManagedThreadId;
            ea.Publish(new MessageA("thread-pool"));
            return handlerThreadId;
        });

        await Assert.That(handlerThreadId).IsEqualTo(publisherThreadId);
    }

    [Test]
    public async Task Publish_BackgroundThread_OffThreadPool_BatchesOnThreadPool()
    {
        var ea = new EventAggregator();
        var target1 = new TestTarget();
        var target2 = new TestTarget();
        var publisherThreadId = 0;
        var handlerThreadIds = new List<int>();
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var received = 0;

        void Handle(TestTarget target, MessageA message)
        {
            lock (handlerThreadIds)
                handlerThreadIds.Add(Environment.CurrentManagedThreadId);

            target.ReceivedMessages.Add(message);
            if (Interlocked.Increment(ref received) == 2)
                completed.TrySetResult();
        }

        ea.Subscribe<TestTarget, MessageA>(target1, Handle, ThreadOption.BackgroundThread);
        ea.Subscribe<TestTarget, MessageA>(target2, Handle, ThreadOption.BackgroundThread);
        var publishReturned = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var publisherThread = new Thread(() =>
        {
            publisherThreadId = Environment.CurrentManagedThreadId;
            ea.Publish(new MessageA("queued"));
            publishReturned.TrySetResult();
        });

        publisherThread.Start();
        await publishReturned.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await completed.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await Assert.That(target1.ReceivedMessages.Count).IsEqualTo(1);
        await Assert.That(target2.ReceivedMessages.Count).IsEqualTo(1);
        await Assert.That(handlerThreadIds.Count).IsEqualTo(2);
        await Assert.That(handlerThreadIds[0]).IsNotEqualTo(publisherThreadId);
        await Assert.That(handlerThreadIds[0]).IsEqualTo(handlerThreadIds[1]);
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

    private sealed class TestViewContext
    {
        public bool IsCurrent { get; set; }
        public List<Action> QueuedActions { get; } = [];

        public void RunNext()
        {
            var action = QueuedActions[0];
            QueuedActions.RemoveAt(0);
            IsCurrent = true;
            try
            {
                action();
            }
            finally
            {
                IsCurrent = false;
            }
        }
    }

    private sealed class TestView(TestViewContext context)
    {
        public TestViewContext Context { get; } = context;
        public List<object> ReceivedMessages { get; } = [];
    }

    private sealed class TestViewAwareTarget : ViewAware
    {
        public List<object> ReceivedMessages { get; } = [];
    }

    private sealed class TestParentAwareTarget : ViewAware
    {
        public List<object> ReceivedMessages { get; } = [];
    }

    private sealed class CustomViewAwareTarget(object view) : IViewAware
    {
        public List<object> ReceivedMessages { get; } = [];

        public void AttachView(object attachedView, string? context) { }
        public bool DetachView(object attachedView, string? context) => false;
        public object? GetView(string? context = null) => view;
        public IReadOnlyList<KeyValuePair<WeakReference, string?>> GetViews() =>
            [new KeyValuePair<WeakReference, string?>(new WeakReference(view), null)];
    }

    private sealed class TestViewAdapter : IViewAdapter
    {
        public bool IsInDesignTool => false;
        public bool CanHandle(object view) => view is TestView;
        public object GetFirstNonGeneratedView(object view) => view;
        public void ExecuteOnFirstLoad(object view, Action<object> handler) { }
        public void ExecuteOnLayoutUpdated(object view, Action<object> handler) { }
        public Task<bool> TryCloseAsync(object view) => Task.FromResult(false);
        public object? GetCommandParameter(object view) => null;
        public bool CheckAccess(object view) => ((TestView)view).Context.IsCurrent;
        public void BeginInvoke(object view, Action action) => ((TestView)view).Context.QueuedActions.Add(action);
    }
}
