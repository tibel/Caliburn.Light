using System.Collections;
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

        var collection = new BindableCollection<string>();
        collection.AddRange(source);

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
        var collection = new BindableCollection<string>();
        collection.AddRange(["item"]);
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
        var collection = new BindableCollection<string>();
        collection.AddRange(["a", "b"]);
        NotifyCollectionChangedEventArgs? args = null;
        collection.CollectionChanged += (_, e) => args = e;

        collection.Clear();

        await Assert.That(args).IsNotNull();
        await Assert.That(args!.Action).IsEqualTo(NotifyCollectionChangedAction.Reset);
    }

    [Test]
    public async Task Clear_RemovesAllItems()
    {
        var collection = new BindableCollection<string>();
        collection.AddRange(["a", "b"]);

        collection.Clear();

        await Assert.That(collection.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Clear_EmptyCollection_DoesNotRaiseNotifications()
    {
        var collection = new BindableCollection<string>();
        var collectionChanged = false;
        var propertyChanged = false;
        collection.CollectionChanged += (_, _) => collectionChanged = true;
        ((INotifyPropertyChanged)collection).PropertyChanged += (_, _) => propertyChanged = true;

        collection.Clear();

        await Assert.That(collectionChanged).IsFalse();
        await Assert.That(propertyChanged).IsFalse();
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
    public async Task AddRange_EmptyCollection_DoesNotRaiseNotifications()
    {
        var collection = new BindableCollection<string>();
        var collectionChanged = false;
        var propertyChanged = false;
        collection.CollectionChanged += (_, _) => collectionChanged = true;
        ((INotifyPropertyChanged)collection).PropertyChanged += (_, _) => propertyChanged = true;

        collection.AddRange([]);

        await Assert.That(collectionChanged).IsFalse();
        await Assert.That(propertyChanged).IsFalse();
    }

    [Test]
    public async Task AddRange_NullItems_ThrowsArgumentNullException()
    {
        var collection = new BindableCollection<string>();

        await Assert.That(() => collection.AddRange(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task AddRange_Self_AppendsOriginalItems()
    {
        var collection = new BindableCollection<string>();
        collection.AddRange(["a", "b"]);

        collection.AddRange(collection);

        await Assert.That(collection.Count).IsEqualTo(4);
        await Assert.That(collection[0]).IsEqualTo("a");
        await Assert.That(collection[1]).IsEqualTo("b");
        await Assert.That(collection[2]).IsEqualTo("a");
        await Assert.That(collection[3]).IsEqualTo("b");
    }

    [Test]
    public async Task InsertRange_InsertsItemsAtIndexAndRaisesReset()
    {
        var collection = new BindableCollection<string>();
        collection.AddRange(["a", "d"]);
        NotifyCollectionChangedEventArgs? args = null;
        collection.CollectionChanged += (_, e) => args = e;

        collection.InsertRange(1, ["b", "c"]);

        await Assert.That(collection.Count).IsEqualTo(4);
        await Assert.That(collection[0]).IsEqualTo("a");
        await Assert.That(collection[1]).IsEqualTo("b");
        await Assert.That(collection[2]).IsEqualTo("c");
        await Assert.That(collection[3]).IsEqualTo("d");
        await Assert.That(args).IsNotNull();
        await Assert.That(args!.Action).IsEqualTo(NotifyCollectionChangedAction.Reset);
    }

    [Test]
    public async Task InsertRange_Self_InsertsOriginalItems()
    {
        var collection = new BindableCollection<string>();
        collection.AddRange(["a", "b"]);

        collection.InsertRange(1, collection);

        await Assert.That(collection.Count).IsEqualTo(4);
        await Assert.That(collection[0]).IsEqualTo("a");
        await Assert.That(collection[1]).IsEqualTo("a");
        await Assert.That(collection[2]).IsEqualTo("b");
        await Assert.That(collection[3]).IsEqualTo("b");
    }

    [Test]
    public async Task InsertRange_InvalidIndex_ThrowsWithoutChangingCollection()
    {
        var collection = new BindableCollection<string>();
        collection.AddRange(["a"]);

        await Assert.That(() => collection.InsertRange(2, ["b"])).Throws<ArgumentOutOfRangeException>();
        await Assert.That(collection.Count).IsEqualTo(1);
        await Assert.That(collection[0]).IsEqualTo("a");
    }

    [Test]
    public async Task InsertRange_Empty_DoesNotRaiseNotifications()
    {
        var collection = new BindableCollection<string>();
        collection.AddRange(["a"]);
        var collectionChanged = false;
        var propertyChanged = false;
        collection.CollectionChanged += (_, _) => collectionChanged = true;
        ((INotifyPropertyChanged)collection).PropertyChanged += (_, _) => propertyChanged = true;

        collection.InsertRange(0, Array.Empty<string>());

        await Assert.That(collectionChanged).IsFalse();
        await Assert.That(propertyChanged).IsFalse();
    }

    [Test]
    public async Task RemoveAll_RemovesMatchesAndRaisesReset()
    {
        var collection = new BindableCollection<string>();
        collection.AddRange(["a", "b", "c", "b"]);
        NotifyCollectionChangedEventArgs? args = null;
        collection.CollectionChanged += (_, e) => args = e;

        var removedCount = collection.RemoveAll(item => item == "b");

        await Assert.That(removedCount).IsEqualTo(2);
        await Assert.That(collection.Count).IsEqualTo(2);
        await Assert.That(collection[0]).IsEqualTo("a");
        await Assert.That(collection[1]).IsEqualTo("c");
        await Assert.That(args).IsNotNull();
        await Assert.That(args!.Action).IsEqualTo(NotifyCollectionChangedAction.Reset);
    }

    [Test]
    public async Task RemoveAll_NoMatches_DoesNotRaiseNotifications()
    {
        var collection = new BindableCollection<string>();
        collection.AddRange(["a", "b"]);
        var collectionChanged = false;
        var propertyChanged = false;
        collection.CollectionChanged += (_, _) => collectionChanged = true;
        ((INotifyPropertyChanged)collection).PropertyChanged += (_, _) => propertyChanged = true;

        var removedCount = collection.RemoveAll(item => item == "missing");

        await Assert.That(removedCount).IsEqualTo(0);
        await Assert.That(collectionChanged).IsFalse();
        await Assert.That(propertyChanged).IsFalse();
    }

    [Test]
    public async Task RemoveAll_NullPredicate_ThrowsArgumentNullException()
    {
        var collection = new BindableCollection<string>();

        await Assert.That(() => collection.RemoveAll(null!)).Throws<ArgumentNullException>();
    }

    // --- RemoveRange tests ---

    [Test]
    public async Task RemoveRange_RemovesSpecifiedItems()
    {
        var collection = new BindableCollection<string>();
        collection.AddRange(["a", "b", "c", "d"]);

        var removedCount = collection.RemoveRange(["b", "d"]);

        await Assert.That(removedCount).IsEqualTo(2);
        await Assert.That(collection.Count).IsEqualTo(2);
        await Assert.That(collection[0]).IsEqualTo("a");
        await Assert.That(collection[1]).IsEqualTo("c");
    }

    [Test]
    public async Task RemoveRange_RaisesSingleCollectionChangedReset()
    {
        var collection = new BindableCollection<string>();
        collection.AddRange(["a", "b", "c"]);
        var collectionChangedCount = 0;
        collection.CollectionChanged += (_, e) => collectionChangedCount++;

        collection.RemoveRange(["a", "c"]);

        await Assert.That(collectionChangedCount).IsEqualTo(1);
    }

    [Test]
    public async Task RemoveRange_NoMatches_DoesNotRaiseNotifications()
    {
        var collection = new BindableCollection<string>();
        collection.AddRange(["a"]);
        var collectionChanged = false;
        var propertyChanged = false;
        collection.CollectionChanged += (_, _) => collectionChanged = true;
        ((INotifyPropertyChanged)collection).PropertyChanged += (_, _) => propertyChanged = true;

        var removedCount = collection.RemoveRange(["missing"]);

        await Assert.That(removedCount).IsEqualTo(0);
        await Assert.That(collectionChanged).IsFalse();
        await Assert.That(propertyChanged).IsFalse();
    }

    [Test]
    public async Task RemoveRange_NullItems_ThrowsArgumentNullException()
    {
        var collection = new BindableCollection<string>();

        await Assert.That(() => collection.RemoveRange(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task RemoveRange_Self_ClearsItems()
    {
        var collection = new BindableCollection<string>();
        collection.AddRange(["a", "b"]);
        NotifyCollectionChangedEventArgs? args = null;
        collection.CollectionChanged += (_, e) => args = e;

        var removedCount = collection.RemoveRange(collection);

        await Assert.That(removedCount).IsEqualTo(2);
        await Assert.That(collection.Count).IsEqualTo(0);
        await Assert.That(args).IsNotNull();
        await Assert.That(args!.Action).IsEqualTo(NotifyCollectionChangedAction.Reset);
    }

    [Test]
    public async Task RemoveRange_ByIndex_RemovesRequestedCount()
    {
        var collection = new BindableCollection<string>();
        collection.AddRange(["a", "b", "c"]);

        collection.RemoveRange(1, 2);

        await Assert.That(collection.Count).IsEqualTo(1);
        await Assert.That(collection[0]).IsEqualTo("a");
    }

    [Test]
    public async Task RemoveRange_ByIndex_ZeroCount_DoesNotRaiseNotifications()
    {
        var collection = new BindableCollection<string>();
        collection.AddRange(["a"]);
        var collectionChanged = false;
        var propertyChanged = false;
        collection.CollectionChanged += (_, _) => collectionChanged = true;
        ((INotifyPropertyChanged)collection).PropertyChanged += (_, _) => propertyChanged = true;

        collection.RemoveRange(0, 0);

        await Assert.That(collection.Count).IsEqualTo(1);
        await Assert.That(collectionChanged).IsFalse();
        await Assert.That(propertyChanged).IsFalse();
    }

    // --- Move tests ---

    [Test]
    public async Task Move_MovesItem()
    {
        var collection = new BindableCollection<string>();
        collection.AddRange(["a", "b", "c"]);

        collection.Move(0, 2);

        await Assert.That(collection[0]).IsEqualTo("b");
        await Assert.That(collection[1]).IsEqualTo("c");
        await Assert.That(collection[2]).IsEqualTo("a");
    }

    [Test]
    public async Task Move_RaisesCollectionChanged_Move()
    {
        var collection = new BindableCollection<string>();
        collection.AddRange(["a", "b", "c"]);
        NotifyCollectionChangedEventArgs? args = null;
        collection.CollectionChanged += (_, e) => args = e;

        collection.Move(0, 2);

        await Assert.That(args).IsNotNull();
        await Assert.That(args!.Action).IsEqualTo(NotifyCollectionChangedAction.Move);
    }

    [Test]
    public async Task Move_ToSameIndex_DoesNotRaiseNotifications()
    {
        var collection = new BindableCollection<string>();
        collection.AddRange(["a"]);
        var collectionChanged = false;
        var propertyChanged = false;
        collection.CollectionChanged += (_, _) => collectionChanged = true;
        ((INotifyPropertyChanged)collection).PropertyChanged += (_, _) => propertyChanged = true;

        collection.Move(0, 0);

        await Assert.That(collectionChanged).IsFalse();
        await Assert.That(propertyChanged).IsFalse();
    }

    [Test]
    public async Task Move_ToSameIndex_DuringNotification_DoesNotThrowWithMultipleSubscribers()
    {
        var collection = new BindableCollection<string>();
        collection.AddRange(["a"]);
        var nestedMoveStarted = false;
        collection.CollectionChanged += (_, _) =>
        {
            if (nestedMoveStarted) return;
            nestedMoveStarted = true;
            collection.Move(0, 0);
        };
        collection.CollectionChanged += (_, _) => { };

        collection.Add("b");

        await Assert.That(collection.Count).IsEqualTo(2);
    }

    [Test]
    public async Task Move_InvalidNewIndex_DoesNotChangeCollection()
    {
        var collection = new BindableCollection<string>();
        collection.AddRange(["a", "b"]);

        await Assert.That(() => collection.Move(0, collection.Count)).Throws<ArgumentOutOfRangeException>();
        await Assert.That(collection.Count).IsEqualTo(2);
        await Assert.That(collection[0]).IsEqualTo("a");
        await Assert.That(collection[1]).IsEqualTo("b");
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
    public async Task SuspendNotifications_SuppressesClearNotifications()
    {
        var collection = new BindableCollection<string>();
        collection.AddRange(["a", "b"]);
        var collectionChanged = false;
        var propertyChanged = false;
        collection.CollectionChanged += (_, _) => collectionChanged = true;
        ((INotifyPropertyChanged)collection).PropertyChanged += (_, _) => propertyChanged = true;

        using (collection.SuspendNotifications())
        {
            collection.Clear();
        }

        await Assert.That(collection.Count).IsEqualTo(0);
        await Assert.That(collectionChanged).IsFalse();
        await Assert.That(propertyChanged).IsFalse();
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
        var collection = new BindableCollection<string>();
        collection.AddRange(["a"]);
        NotifyCollectionChangedEventArgs? args = null;
        collection.CollectionChanged += (_, e) => args = e;

        collection.Refresh();

        await Assert.That(args).IsNotNull();
        await Assert.That(args!.Action).IsEqualTo(NotifyCollectionChangedAction.Reset);
    }

    [Test]
    public async Task Refresh_RaisesPropertyChanged_Count()
    {
        var collection = new BindableCollection<string>();
        collection.AddRange(["a"]);
        var properties = new List<string?>();
        ((INotifyPropertyChanged)collection).PropertyChanged += (_, e) => properties.Add(e.PropertyName);

        collection.Refresh();

        await Assert.That(properties).Contains("Count");
    }

    [Test]
    public async Task Refresh_RaisesPropertyChanged_ItemIndexer()
    {
        var collection = new BindableCollection<string>();
        collection.AddRange(["a"]);
        var properties = new List<string?>();
        ((INotifyPropertyChanged)collection).PropertyChanged += (_, e) => properties.Add(e.PropertyName);

        collection.Refresh();

        await Assert.That(properties).Contains("Item[]");
    }

    [Test]
    public async Task Refresh_WhenSuspended_DoesNotRaise()
    {
        var collection = new BindableCollection<string>();
        collection.AddRange(["a"]);
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
        var collection = new BindableCollection<string>();
        collection.AddRange(["a", "b"]);
        NotifyCollectionChangedEventArgs? args = null;
        collection.CollectionChanged += (_, e) => args = e;

        collection[0] = "x";

        await Assert.That(args).IsNotNull();
        await Assert.That(args!.Action).IsEqualTo(NotifyCollectionChangedAction.Replace);
    }

    [Test]
    public async Task Indexer_SetSameInstance_RaisesReplaceNotification()
    {
        var collection = new BindableCollection<object>();
        var item = new object();
        collection.Add(item);
        NotifyCollectionChangedEventArgs? args = null;
        collection.CollectionChanged += (_, e) => args = e;

        collection[0] = item;

        await Assert.That(args).IsNotNull();
        await Assert.That(args!.Action).IsEqualTo(NotifyCollectionChangedAction.Replace);
    }

    [Test]
    public async Task NonGenericIList_ProvidesListOperations()
    {
        IList list = new BindableCollection<string>();

        var index = list.Add("item");

        await Assert.That(index).IsEqualTo(0);
        await Assert.That(list[0]).IsEqualTo("item");
        await Assert.That(list.IsReadOnly).IsFalse();
        await Assert.That(list.IsFixedSize).IsFalse();
        await Assert.That(list.IsSynchronized).IsFalse();
        await Assert.That(list.SyncRoot).IsNotNull();
        await Assert.That(list.Contains("item")).IsTrue();
    }

    [Test]
    public async Task NonGenericIList_AddWithIncompatibleValue_ThrowsArgumentException()
    {
        IList list = new BindableCollection<string>();

        await Assert.That(() => list.Add(42)).Throws<ArgumentException>();
    }

    [Test]
    public async Task CollectionChanged_ReentrantMutationWithSingleSubscriber_IsAllowed()
    {
        var collection = new BindableCollection<string>();
        var nestedMutationStarted = false;
        collection.CollectionChanged += (_, _) =>
        {
            if (nestedMutationStarted) return;
            nestedMutationStarted = true;
            collection.Add("nested");
        };

        collection.Add("outer");

        await Assert.That(collection.Count).IsEqualTo(2);
    }

    [Test]
    public async Task CollectionChanged_ReentrantMutationWithMultipleSubscribers_Throws()
    {
        var collection = new BindableCollection<string>();
        collection.CollectionChanged += (_, _) => collection.Add("nested");
        collection.CollectionChanged += (_, _) => { };

        await Assert.That(() => collection.Add("outer")).Throws<InvalidOperationException>();
        await Assert.That(collection.Count).IsEqualTo(1);
    }

}
