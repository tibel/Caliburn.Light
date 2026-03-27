using System.Collections.Specialized;
using System.ComponentModel;
using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

public class BindableCollectionTests
{
    // --- Constructor tests ---

    [Test]
    public async Task Constructor_Default_CreatesEmptyCollection()
    {
        var collection = new BindableCollection<string>();

        await Assert.That(collection.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Constructor_WithCollection_CopiesElements()
    {
        var source = new[] { "a", "b", "c" };

        var collection = new BindableCollection<string>(source);

        await Assert.That(collection.Count).IsEqualTo(3);
        await Assert.That(collection[0]).IsEqualTo("a");
        await Assert.That(collection[1]).IsEqualTo("b");
        await Assert.That(collection[2]).IsEqualTo("c");
    }

    // --- Add tests ---

    [Test]
    public async Task Add_RaisesCollectionChanged()
    {
        var collection = new BindableCollection<string>();
        NotifyCollectionChangedEventArgs? args = null;
        collection.CollectionChanged += (_, e) => args = e;

        collection.Add("item");

        await Assert.That(args).IsNotNull();
        await Assert.That(args!.Action).IsEqualTo(NotifyCollectionChangedAction.Add);
        await Assert.That(args.NewItems![0]).IsEqualTo("item");
    }

    [Test]
    public async Task Add_RaisesPropertyChanged_Count()
    {
        var collection = new BindableCollection<string>();
        var properties = new List<string?>();
        ((INotifyPropertyChanged)collection).PropertyChanged += (_, e) => properties.Add(e.PropertyName);

        collection.Add("item");

        await Assert.That(properties).Contains("Count");
    }

    // --- Remove tests ---

    [Test]
    public async Task Remove_RaisesCollectionChanged()
    {
        var collection = new BindableCollection<string>(["item"]);
        NotifyCollectionChangedEventArgs? args = null;
        collection.CollectionChanged += (_, e) => args = e;

        collection.Remove("item");

        await Assert.That(args).IsNotNull();
        await Assert.That(args!.Action).IsEqualTo(NotifyCollectionChangedAction.Remove);
    }

    // --- Clear tests ---

    [Test]
    public async Task Clear_RaisesCollectionChanged_Reset()
    {
        var collection = new BindableCollection<string>(["a", "b"]);
        NotifyCollectionChangedEventArgs? args = null;
        collection.CollectionChanged += (_, e) => args = e;

        collection.Clear();

        await Assert.That(args).IsNotNull();
        await Assert.That(args!.Action).IsEqualTo(NotifyCollectionChangedAction.Reset);
    }

    [Test]
    public async Task Clear_RemovesAllItems()
    {
        var collection = new BindableCollection<string>(["a", "b"]);

        collection.Clear();

        await Assert.That(collection.Count).IsEqualTo(0);
    }

    // --- AddRange tests ---

    [Test]
    public async Task AddRange_AddsAllItems()
    {
        var collection = new BindableCollection<string>();

        collection.AddRange(["a", "b", "c"]);

        await Assert.That(collection.Count).IsEqualTo(3);
    }

    [Test]
    public async Task AddRange_RaisesSingleCollectionChangedReset()
    {
        var collection = new BindableCollection<string>();
        var collectionChangedCount = 0;
        collection.CollectionChanged += (_, e) => collectionChangedCount++;

        collection.AddRange(["a", "b", "c"]);

        await Assert.That(collectionChangedCount).IsEqualTo(1);
    }

    [Test]
    public async Task AddRange_NullItems_ThrowsArgumentNullException()
    {
        var collection = new BindableCollection<string>();

        await Assert.That(() => collection.AddRange(null!)).Throws<ArgumentNullException>();
    }

    // --- RemoveRange tests ---

    [Test]
    public async Task RemoveRange_RemovesSpecifiedItems()
    {
        var collection = new BindableCollection<string>(["a", "b", "c", "d"]);

        collection.RemoveRange(["b", "d"]);

        await Assert.That(collection.Count).IsEqualTo(2);
        await Assert.That(collection[0]).IsEqualTo("a");
        await Assert.That(collection[1]).IsEqualTo("c");
    }

    [Test]
    public async Task RemoveRange_RaisesSingleCollectionChangedReset()
    {
        var collection = new BindableCollection<string>(["a", "b", "c"]);
        var collectionChangedCount = 0;
        collection.CollectionChanged += (_, e) => collectionChangedCount++;

        collection.RemoveRange(["a", "c"]);

        await Assert.That(collectionChangedCount).IsEqualTo(1);
    }

    [Test]
    public async Task RemoveRange_NullItems_ThrowsArgumentNullException()
    {
        var collection = new BindableCollection<string>();

        await Assert.That(() => collection.RemoveRange(null!)).Throws<ArgumentNullException>();
    }

    // --- Move tests ---

    [Test]
    public async Task Move_MovesItem()
    {
        var collection = new BindableCollection<string>(["a", "b", "c"]);

        collection.Move(0, 2);

        await Assert.That(collection[0]).IsEqualTo("b");
        await Assert.That(collection[1]).IsEqualTo("c");
        await Assert.That(collection[2]).IsEqualTo("a");
    }

    [Test]
    public async Task Move_RaisesCollectionChanged_Move()
    {
        var collection = new BindableCollection<string>(["a", "b", "c"]);
        NotifyCollectionChangedEventArgs? args = null;
        collection.CollectionChanged += (_, e) => args = e;

        collection.Move(0, 2);

        await Assert.That(args).IsNotNull();
        await Assert.That(args!.Action).IsEqualTo(NotifyCollectionChangedAction.Move);
    }

    // --- SuspendNotifications tests ---

    [Test]
    public async Task SuspendNotifications_SuppressesCollectionChanged()
    {
        var collection = new BindableCollection<string>();
        var raised = false;
        collection.CollectionChanged += (_, _) => raised = true;

        using (collection.SuspendNotifications())
        {
            collection.Add("item");
        }

        await Assert.That(raised).IsFalse();
    }

    [Test]
    public async Task SuspendNotifications_SuppressesPropertyChanged()
    {
        var collection = new BindableCollection<string>();
        var raised = false;
        ((INotifyPropertyChanged)collection).PropertyChanged += (_, _) => raised = true;

        using (collection.SuspendNotifications())
        {
            collection.Add("item");
        }

        await Assert.That(raised).IsFalse();
    }

    [Test]
    public async Task SuspendNotifications_AfterDispose_NotificationsResume()
    {
        var collection = new BindableCollection<string>();
        var raised = false;
        collection.CollectionChanged += (_, _) => raised = true;

        using (collection.SuspendNotifications()) { }

        collection.Add("item");

        await Assert.That(raised).IsTrue();
    }

    [Test]
    public async Task SuspendNotifications_Nested_StaysSuppressed()
    {
        var collection = new BindableCollection<string>();
        var raised = false;
        collection.CollectionChanged += (_, _) => raised = true;

        using (collection.SuspendNotifications())
        {
            using (collection.SuspendNotifications())
            {
                collection.Add("a");
            }

            collection.Add("b");
        }

        await Assert.That(raised).IsFalse();
    }

    // --- Refresh tests ---

    [Test]
    public async Task Refresh_RaisesCollectionChanged_Reset()
    {
        var collection = new BindableCollection<string>(["a"]);
        NotifyCollectionChangedEventArgs? args = null;
        collection.CollectionChanged += (_, e) => args = e;

        collection.Refresh();

        await Assert.That(args).IsNotNull();
        await Assert.That(args!.Action).IsEqualTo(NotifyCollectionChangedAction.Reset);
    }

    [Test]
    public async Task Refresh_RaisesPropertyChanged_Count()
    {
        var collection = new BindableCollection<string>(["a"]);
        var properties = new List<string?>();
        ((INotifyPropertyChanged)collection).PropertyChanged += (_, e) => properties.Add(e.PropertyName);

        collection.Refresh();

        await Assert.That(properties).Contains("Count");
    }

    [Test]
    public async Task Refresh_RaisesPropertyChanged_ItemIndexer()
    {
        var collection = new BindableCollection<string>(["a"]);
        var properties = new List<string?>();
        ((INotifyPropertyChanged)collection).PropertyChanged += (_, e) => properties.Add(e.PropertyName);

        collection.Refresh();

        await Assert.That(properties).Contains("Item[]");
    }

    [Test]
    public async Task Refresh_WhenSuspended_DoesNotRaise()
    {
        var collection = new BindableCollection<string>(["a"]);
        var raised = false;
        collection.CollectionChanged += (_, _) => raised = true;

        using (collection.SuspendNotifications())
        {
            collection.Refresh();
        }

        await Assert.That(raised).IsFalse();
    }

    // --- Null item tests ---

    [Test]
    public async Task Add_NullItem_Succeeds()
    {
        var collection = new BindableCollection<string?>();

        collection.Add(null);

        await Assert.That(collection.Count).IsEqualTo(1);
        await Assert.That(collection[0]).IsNull();
    }

    [Test]
    public async Task AddRange_WithNullItems_Succeeds()
    {
        var collection = new BindableCollection<string?>();

        collection.AddRange([null, "a", null]);

        await Assert.That(collection.Count).IsEqualTo(3);
        await Assert.That(collection[0]).IsNull();
        await Assert.That(collection[1]).IsEqualTo("a");
    }

    // --- Replace / Set tests ---

    [Test]
    public async Task Indexer_Set_RaisesCollectionChanged_Replace()
    {
        var collection = new BindableCollection<string>(["a", "b"]);
        NotifyCollectionChangedEventArgs? args = null;
        collection.CollectionChanged += (_, e) => args = e;

        collection[0] = "x";

        await Assert.That(args).IsNotNull();
        await Assert.That(args!.Action).IsEqualTo(NotifyCollectionChangedAction.Replace);
    }
}
