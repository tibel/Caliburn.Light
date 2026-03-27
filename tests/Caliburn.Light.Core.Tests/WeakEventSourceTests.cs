using System.Runtime.CompilerServices;
using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

public class WeakEventSourceTests
{
    #region WeakEventSource (non-generic)

    [Test]
    public async Task Add_Raise_HandlerIsInvoked()
    {
        var source = new WeakEventSource();
        var callCount = 0;
        source.Add((_, _) => callCount++);

        source.Raise(null, EventArgs.Empty);

        await Assert.That(callCount).IsEqualTo(1);
    }

    [Test]
    public async Task Remove_AfterAdd_HandlerNotInvoked()
    {
        var source = new WeakEventSource();
        var callCount = 0;
        EventHandler handler = (_, _) => callCount++;
        source.Add(handler);
        source.Remove(handler);

        source.Raise(null, EventArgs.Empty);

        await Assert.That(callCount).IsEqualTo(0);
    }

    [Test]
    public async Task Raise_MultipleHandlers_AllInvoked()
    {
        var source = new WeakEventSource();
        var callCount1 = 0;
        var callCount2 = 0;
        source.Add((_, _) => callCount1++);
        source.Add((_, _) => callCount2++);

        source.Raise(null, EventArgs.Empty);

        await Assert.That(callCount1).IsEqualTo(1);
        await Assert.That(callCount2).IsEqualTo(1);
    }

    [Test]
    public void Add_NullHandler_DoesNotThrow()
    {
        var source = new WeakEventSource();

        source.Add(null!);
        source.Raise(null, EventArgs.Empty);
    }

    [Test]
    public void Remove_NullHandler_DoesNotThrow()
    {
        var source = new WeakEventSource();

        source.Remove(null!);
    }

    [Test]
    public void Raise_NoHandlers_DoesNotThrow()
    {
        var source = new WeakEventSource();

        source.Raise(null, EventArgs.Empty);
    }

    [Test]
    public async Task Raise_CorrectSenderAndArgs()
    {
        var source = new WeakEventSource();
        object? capturedSender = null;
        EventArgs? capturedArgs = null;
        source.Add((sender, e) =>
        {
            capturedSender = sender;
            capturedArgs = e;
        });

        var expectedSender = new object();
        source.Raise(expectedSender, EventArgs.Empty);

        await Assert.That(capturedSender).IsEqualTo(expectedSender);
        await Assert.That(capturedArgs).IsEqualTo(EventArgs.Empty);
    }

    [Test]
    public async Task Raise_AfterSubscriberCollected_DoesNotInvoke()
    {
        var source = new WeakEventSource();
        var totalCallCount = 0;
        var liveCallCount = 0;
        source.Add((_, _) => { totalCallCount++; liveCallCount++; });

        var weakRef = AddHandlerFromTemporarySubscriber(source);

        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
        GC.WaitForPendingFinalizers();

        source.Raise(null, EventArgs.Empty);

        await Assert.That(weakRef.IsAlive).IsFalse();
        await Assert.That(liveCallCount).IsEqualTo(1);
        // totalCallCount also 1 means the dead handler was NOT invoked
        await Assert.That(totalCallCount).IsEqualTo(1);
    }

    [Test]
    public async Task Add_AfterSubscriberCollected_CleansDeadReferences()
    {
        var source = new WeakEventSource();
        var totalCallCount = 0;

        var weakRef = AddHandlerFromTemporarySubscriber(source);

        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
        GC.WaitForPendingFinalizers();

        await Assert.That(weakRef.IsAlive).IsFalse();

        // Adding a new handler after GC should trigger cleanup of dead references
        source.Add((_, _) => totalCallCount++);

        source.Raise(null, EventArgs.Empty);

        // Only the newly added handler should fire (dead reference cleaned up)
        await Assert.That(totalCallCount).IsEqualTo(1);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference AddHandlerFromTemporarySubscriber(WeakEventSource source)
    {
        var subscriber = new NonGenericEventSubscriber();
        source.Add(subscriber.Handle);
        return new WeakReference(subscriber);
    }

    [Test]
    public async Task Remove_OneOfMultiple_OtherStillInvoked()
    {
        var source = new WeakEventSource();
        var callCount1 = 0;
        var callCount2 = 0;
        EventHandler handler1 = (_, _) => callCount1++;
        EventHandler handler2 = (_, _) => callCount2++;
        source.Add(handler1);
        source.Add(handler2);

        source.Remove(handler1);
        source.Raise(null, EventArgs.Empty);

        await Assert.That(callCount1).IsEqualTo(0);
        await Assert.That(callCount2).IsEqualTo(1);
    }

    [Test]
    public async Task Add_StaticHandler_IsInvoked()
    {
        Interlocked.Exchange(ref _staticCallCount, 0);
        var source = new WeakEventSource();
        source.Add(StaticHandler);

        source.Raise(null, EventArgs.Empty);

        await Assert.That(Volatile.Read(ref _staticCallCount)).IsEqualTo(1);
    }

    private static int _staticCallCount;
    private static void StaticHandler(object? sender, EventArgs e) => Interlocked.Increment(ref _staticCallCount);

    [Test]
    public async Task Raise_HandlerRemovesSelf_DoesNotThrow()
    {
        var source = new WeakEventSource();
        var callCount = 0;
        EventHandler? selfRemovingHandler = null;
        selfRemovingHandler = (_, _) =>
        {
            callCount++;
            source.Remove(selfRemovingHandler!);
        };
        source.Add(selfRemovingHandler);

        source.Raise(null, EventArgs.Empty);
        await Assert.That(callCount).IsEqualTo(1);

        source.Raise(null, EventArgs.Empty);
        await Assert.That(callCount).IsEqualTo(1);
    }

    #endregion

    #region WeakEventSource<TEventArgs>

    [Test]
    public async Task Typed_Add_Raise_HandlerIsInvoked()
    {
        var source = new WeakEventSource<TestEventArgs>();
        var callCount = 0;
        source.Add((_, _) => callCount++);

        source.Raise(null, new TestEventArgs("test"));

        await Assert.That(callCount).IsEqualTo(1);
    }

    [Test]
    public async Task Typed_Remove_AfterAdd_HandlerNotInvoked()
    {
        var source = new WeakEventSource<TestEventArgs>();
        var callCount = 0;
        EventHandler<TestEventArgs> handler = (_, _) => callCount++;
        source.Add(handler);
        source.Remove(handler);

        source.Raise(null, new TestEventArgs("test"));

        await Assert.That(callCount).IsEqualTo(0);
    }

    [Test]
    public async Task Typed_Raise_CorrectSenderAndArgs()
    {
        var source = new WeakEventSource<TestEventArgs>();
        object? capturedSender = null;
        TestEventArgs? capturedArgs = null;
        source.Add((sender, e) =>
        {
            capturedSender = sender;
            capturedArgs = e;
        });

        var expectedSender = new object();
        var expectedArgs = new TestEventArgs("hello");
        source.Raise(expectedSender, expectedArgs);

        await Assert.That(capturedSender).IsEqualTo(expectedSender);
        await Assert.That(capturedArgs).IsNotNull();
        await Assert.That(capturedArgs!.Message).IsEqualTo("hello");
    }

    [Test]
    public async Task Typed_Raise_MultipleHandlers_AllInvoked()
    {
        var source = new WeakEventSource<TestEventArgs>();
        var callCount1 = 0;
        var callCount2 = 0;
        source.Add((_, _) => callCount1++);
        source.Add((_, _) => callCount2++);

        source.Raise(null, new TestEventArgs("test"));

        await Assert.That(callCount1).IsEqualTo(1);
        await Assert.That(callCount2).IsEqualTo(1);
    }

    [Test]
    public async Task Typed_Raise_AfterSubscriberCollected_DoesNotInvoke()
    {
        var source = new WeakEventSource<TestEventArgs>();
        var totalCallCount = 0;
        var liveCallCount = 0;
        source.Add((_, _) => { totalCallCount++; liveCallCount++; });

        var weakRef = AddTypedHandlerFromTemporarySubscriber(source);

        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
        GC.WaitForPendingFinalizers();

        source.Raise(null, new TestEventArgs("test"));

        await Assert.That(weakRef.IsAlive).IsFalse();
        await Assert.That(liveCallCount).IsEqualTo(1);
        // totalCallCount also 1 means the dead handler was NOT invoked
        await Assert.That(totalCallCount).IsEqualTo(1);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference AddTypedHandlerFromTemporarySubscriber(WeakEventSource<TestEventArgs> source)
    {
        var subscriber = new TypedEventSubscriber();
        source.Add(subscriber.Handle);
        return new WeakReference(subscriber);
    }

    #endregion

    #region Helpers

    private class NonGenericEventSubscriber
    {
        public int CallCount;
        public void Handle(object? sender, EventArgs e) => CallCount++;
    }

    private class TypedEventSubscriber
    {
        public int CallCount;
        public void Handle(object? sender, TestEventArgs e) => CallCount++;
    }

    private sealed class TestEventArgs : EventArgs
    {
        public string Message { get; }
        public TestEventArgs(string message) => Message = message;
    }

    #endregion
}
