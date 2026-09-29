using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

public class WeakEventHandlerTests
{
    [Test]
    public async Task RegisterPropertyChangedWeak_ReceivesEvents()
    {
        var source = new TestBindableObject();
        var subscriber = new PropertyChangedSubscriber();
        using var reg = source.RegisterPropertyChangedWeak(subscriber,
            static (s, sender, e) => s.OnPropertyChanged(sender, e));

        source.Name = "Alice";

        await Assert.That(subscriber.CallCount).IsEqualTo(1);
        await Assert.That(subscriber.LastPropertyName).IsEqualTo("Name");
    }

    [Test]
    public async Task RegisterPropertyChangedWeak_Dispose_StopsEvents()
    {
        var source = new TestBindableObject();
        var subscriber = new PropertyChangedSubscriber();
        var reg = source.RegisterPropertyChangedWeak(subscriber,
            static (s, sender, e) => s.OnPropertyChanged(sender, e));

        reg.Dispose();
        source.Name = "Alice";

        await Assert.That(subscriber.CallCount).IsEqualTo(0);
    }

    [Test]
    public async Task RegisterPropertyChangedWeak_MultipleEvents_AllReceived()
    {
        var source = new TestBindableObject();
        var subscriber = new PropertyChangedSubscriber();
        using var reg = source.RegisterPropertyChangedWeak(subscriber,
            static (s, sender, e) => s.OnPropertyChanged(sender, e));

        source.Name = "Alice";
        source.Age = 30;

        await Assert.That(subscriber.CallCount).IsEqualTo(2);
    }

    [Test]
    public async Task RegisterPropertyChangingWeak_ReceivesEvents()
    {
        var source = new TestBindableObject();
        var subscriber = new PropertyChangingSubscriber();
        using var reg = source.RegisterPropertyChangingWeak(subscriber,
            static (s, sender, e) => s.OnPropertyChanging(sender, e));

        source.Name = "Alice";

        await Assert.That(subscriber.CallCount).IsEqualTo(1);
        await Assert.That(subscriber.LastPropertyName).IsEqualTo("Name");
    }

    [Test]
    public async Task RegisterPropertyChangingWeak_Dispose_StopsEvents()
    {
        var source = new TestBindableObject();
        var subscriber = new PropertyChangingSubscriber();
        var reg = source.RegisterPropertyChangingWeak(subscriber,
            static (s, sender, e) => s.OnPropertyChanging(sender, e));

        reg.Dispose();
        source.Name = "Alice";

        await Assert.That(subscriber.CallCount).IsEqualTo(0);
    }

    [Test]
    public async Task RegisterCollectionChangedWeak_ReceivesEvents()
    {
        var source = new BindableCollection<string>();
        var subscriber = new CollectionChangedSubscriber();
        using var reg = source.RegisterCollectionChangedWeak(subscriber,
            static (s, sender, e) => s.OnCollectionChanged(sender, e));

        source.Add("item");

        await Assert.That(subscriber.CallCount).IsEqualTo(1);
        await Assert.That(subscriber.LastAction).IsEqualTo(NotifyCollectionChangedAction.Add);
    }

    [Test]
    public async Task RegisterCollectionChangedWeak_Dispose_StopsEvents()
    {
        var source = new BindableCollection<string>();
        var subscriber = new CollectionChangedSubscriber();
        var reg = source.RegisterCollectionChangedWeak(subscriber,
            static (s, sender, e) => s.OnCollectionChanged(sender, e));

        reg.Dispose();
        source.Add("item");

        await Assert.That(subscriber.CallCount).IsEqualTo(0);
    }

    [Test]
    public async Task RegisterCanExecuteChangedWeak_ReceivesEvents()
    {
        var command = new DelegateCommand(() => { });
        var subscriber = new CanExecuteChangedSubscriber();
        using var reg = command.RegisterCanExecuteChangedWeak(subscriber,
            static (s, sender, e) => s.OnCanExecuteChanged(sender, e));

        command.RaiseCanExecuteChanged();

        await Assert.That(subscriber.CallCount).IsEqualTo(1);
    }

    [Test]
    public async Task RegisterCanExecuteChangedWeak_Dispose_StopsEvents()
    {
        var command = new DelegateCommand(() => { });
        var subscriber = new CanExecuteChangedSubscriber();
        var reg = command.RegisterCanExecuteChangedWeak(subscriber,
            static (s, sender, e) => s.OnCanExecuteChanged(sender, e));

        reg.Dispose();
        command.RaiseCanExecuteChanged();

        await Assert.That(subscriber.CallCount).IsEqualTo(0);
    }

    [Test]
    public async Task RegisterPropertyChangedWeak_SubscriberCollected_AutoRemoves()
    {
        var source = new TestBindableObject();
        var weakRef = RegisterAndAbandonPropertyChangedSubscriber(source);

        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
        GC.WaitForPendingFinalizers();

        await Assert.That(weakRef.IsAlive).IsFalse();

        // Trigger event — dead handler should auto-remove without throwing
        source.Name = "test";

        // Verify a new live subscriber receives events (proving dead handler is gone)
        var liveSubscriber = new PropertyChangedSubscriber();
        source.RegisterPropertyChangedWeak(liveSubscriber,
            static (s, sender, e) => s.OnPropertyChanged(sender, e));

        source.Name = "test2";
        await Assert.That(liveSubscriber.CallCount).IsEqualTo(1);
    }

    [Test]
    public async Task RegisterActivatedWeak_ReceivesEvents()
    {
        var source = new TestScreen();
        var subscriber = new ActivatedSubscriber();
        using var reg = source.RegisterActivatedWeak(subscriber,
            static (s, sender, e) => s.OnActivated(sender, e));

        await ((IActivatable)source).ActivateAsync();

        await Assert.That(subscriber.CallCount).IsEqualTo(1);
        await Assert.That(subscriber.WasInitialized).IsTrue();
    }

    [Test]
    public async Task RegisterActivatedWeak_Dispose_StopsEvents()
    {
        var source = new TestScreen();
        var subscriber = new ActivatedSubscriber();
        var reg = source.RegisterActivatedWeak(subscriber,
            static (s, sender, e) => s.OnActivated(sender, e));

        reg.Dispose();
        await ((IActivatable)source).ActivateAsync();

        await Assert.That(subscriber.CallCount).IsEqualTo(0);
    }

    [Test]
    public async Task RegisterDeactivatingWeak_ReceivesEvents()
    {
        var source = new TestScreen();
        var subscriber = new DeactivatingSubscriber();
        using var reg = source.RegisterDeactivatingWeak(subscriber,
            static (s, sender, e) => s.OnDeactivating(sender, e));

        await ((IActivatable)source).ActivateAsync();
        await ((IActivatable)source).DeactivateAsync(true);

        await Assert.That(subscriber.CallCount).IsEqualTo(1);
        await Assert.That(subscriber.WasClosed).IsTrue();
    }

    [Test]
    public async Task RegisterDeactivatingWeak_Dispose_StopsEvents()
    {
        var source = new TestScreen();
        var subscriber = new DeactivatingSubscriber();
        var reg = source.RegisterDeactivatingWeak(subscriber,
            static (s, sender, e) => s.OnDeactivating(sender, e));

        reg.Dispose();
        await ((IActivatable)source).ActivateAsync();
        await ((IActivatable)source).DeactivateAsync(true);

        await Assert.That(subscriber.CallCount).IsEqualTo(0);
    }

    [Test]
    public async Task RegisterDeactivatedWeak_ReceivesEvents()
    {
        var source = new TestScreen();
        var subscriber = new DeactivatedSubscriber();
        using var reg = source.RegisterDeactivatedWeak(subscriber,
            static (s, sender, e) => s.OnDeactivated(sender, e));

        await ((IActivatable)source).ActivateAsync();
        await ((IActivatable)source).DeactivateAsync(false);

        await Assert.That(subscriber.CallCount).IsEqualTo(1);
        await Assert.That(subscriber.WasClosed).IsFalse();
    }

    [Test]
    public async Task RegisterDeactivatedWeak_Dispose_StopsEvents()
    {
        var source = new TestScreen();
        var subscriber = new DeactivatedSubscriber();
        var reg = source.RegisterDeactivatedWeak(subscriber,
            static (s, sender, e) => s.OnDeactivated(sender, e));

        reg.Dispose();
        await ((IActivatable)source).ActivateAsync();
        await ((IActivatable)source).DeactivateAsync(false);

        await Assert.That(subscriber.CallCount).IsEqualTo(0);
    }

    [Test]
    public async Task RegisterActivationProcessedWeak_ReceivesEvents()
    {
        var conductor = new Conductor<TestScreen>();
        var subscriber = new ActivationProcessedSubscriber();
        var item = new TestScreen();
        using var reg = conductor.RegisterActivationProcessedWeak(subscriber,
            static (s, sender, e) => s.OnActivationProcessed(sender, e));

        await conductor.ActivateItemAsync(item);

        await Assert.That(subscriber.CallCount).IsEqualTo(1);
        await Assert.That(subscriber.Item).IsSameReferenceAs(item);
        await Assert.That(subscriber.Success).IsTrue();
    }

    [Test]
    public async Task RegisterActivationProcessedWeak_CloseGuardDenied_ReceivesFailure()
    {
        var conductor = new Conductor<TestScreen>();
        var subscriber = new ActivationProcessedSubscriber();
        using var reg = conductor.RegisterActivationProcessedWeak(subscriber,
            static (s, sender, e) => s.OnActivationProcessed(sender, e));

        await conductor.ActivateItemAsync(new TestScreen { CanCloseResult = false });
        await conductor.ActivateItemAsync(new TestScreen());

        await Assert.That(subscriber.CallCount).IsEqualTo(2);
        await Assert.That(subscriber.Success).IsFalse();
    }

    [Test]
    public async Task RegisterActivationProcessedWeak_Dispose_StopsEvents()
    {
        var conductor = new Conductor<TestScreen>();
        var subscriber = new ActivationProcessedSubscriber();
        var reg = conductor.RegisterActivationProcessedWeak(subscriber,
            static (s, sender, e) => s.OnActivationProcessed(sender, e));

        reg.Dispose();
        await conductor.ActivateItemAsync(new TestScreen());

        await Assert.That(subscriber.CallCount).IsEqualTo(0);
    }

    [Test]
    public async Task RegisterActivatedWeak_SubscriberCollected_AutoRemoves()
    {
        var source = new TestScreen();
        var weakRef = RegisterAndAbandonActivatedSubscriber(source);

        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
        GC.WaitForPendingFinalizers();

        await Assert.That(weakRef.IsAlive).IsFalse();

        // Trigger the event — the dead handler should auto-remove without throwing
        await ((IActivatable)source).ActivateAsync();

        var liveSubscriber = new ActivatedSubscriber();
        source.RegisterActivatedWeak(liveSubscriber,
            static (s, sender, e) => s.OnActivated(sender, e));

        await ((IActivatable)source).DeactivateAsync(false);
        await ((IActivatable)source).ActivateAsync();

        await Assert.That(liveSubscriber.CallCount).IsEqualTo(1);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference RegisterAndAbandonActivatedSubscriber(TestScreen source)
    {
        var subscriber = new ActivatedSubscriber();
        source.RegisterActivatedWeak(subscriber,
            static (s, sender, e) => s.OnActivated(sender, e));
        return new WeakReference(subscriber);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference RegisterAndAbandonPropertyChangedSubscriber(TestBindableObject source)
    {
        var subscriber = new PropertyChangedSubscriber();
        source.RegisterPropertyChangedWeak(subscriber,
            static (s, sender, e) => s.OnPropertyChanged(sender, e));
        return new WeakReference(subscriber);
    }

    #region Subscriber classes

    private class PropertyChangedSubscriber
    {
        public int CallCount;
        public string? LastPropertyName;

        public void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            CallCount++;
            LastPropertyName = e.PropertyName;
        }
    }

    private class PropertyChangingSubscriber
    {
        public int CallCount;
        public string? LastPropertyName;

        public void OnPropertyChanging(object? sender, PropertyChangingEventArgs e)
        {
            CallCount++;
            LastPropertyName = e.PropertyName;
        }
    }

    private class CollectionChangedSubscriber
    {
        public int CallCount;
        public NotifyCollectionChangedAction? LastAction;

        public void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            CallCount++;
            LastAction = e.Action;
        }
    }

    private class CanExecuteChangedSubscriber
    {
        public int CallCount;

        public void OnCanExecuteChanged(object? sender, EventArgs e)
        {
            CallCount++;
        }
    }

    private class ActivatedSubscriber
    {
        public int CallCount;
        public bool? WasInitialized;

        public void OnActivated(object? sender, ActivationEventArgs e)
        {
            CallCount++;
            WasInitialized = e.WasInitialized;
        }
    }

    private class DeactivatingSubscriber
    {
        public int CallCount;
        public bool? WasClosed;

        public void OnDeactivating(object? sender, DeactivationEventArgs e)
        {
            CallCount++;
            WasClosed = e.WasClosed;
        }
    }

    private class DeactivatedSubscriber
    {
        public int CallCount;
        public bool? WasClosed;

        public void OnDeactivated(object? sender, DeactivationEventArgs e)
        {
            CallCount++;
            WasClosed = e.WasClosed;
        }
    }

    private class ActivationProcessedSubscriber
    {
        public int CallCount;
        public object? Item;
        public bool? Success;

        public void OnActivationProcessed(object? sender, ActivationProcessedEventArgs e)
        {
            CallCount++;
            Item = e.Item;
            Success = e.Success;
        }
    }

    #endregion
}
