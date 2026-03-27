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

        // Trigger event - the dead handler detects GC'd subscriber and auto-removes
        source.Name = "test";

        await Assert.That(weakRef.IsAlive).IsFalse();
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

    #endregion
}
