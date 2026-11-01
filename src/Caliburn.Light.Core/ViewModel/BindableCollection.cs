using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Caliburn.Light;

/// <summary>
/// Represents a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed.
/// </summary>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
public sealed class BindableCollection<T> : IBindableCollection<T>, IReadOnlyBindableCollection<T>, IList
{
    private readonly List<T> _items = new();
    private readonly IBindableCollectionItemHandler<T>? _itemHandler;
    private int _suspensionCount;
    private int _notificationDepth;

    /// <summary>
    /// Initializes a new instance of BindableCollection that is empty and has default initial capacity.
    /// </summary>
    public BindableCollection()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="BindableCollection{T}"/> that notifies the handler when items are added or removed.
    /// </summary>
    /// <param name="itemHandler">The handler to notify when an item is added or removed.</param>
    /// <exception cref="ArgumentNullException"><paramref name="itemHandler"/> is <see langword="null"/>.</exception>
    public BindableCollection(IBindableCollectionItemHandler<T> itemHandler)
    {
        ArgumentNullException.ThrowIfNull(itemHandler);
        _itemHandler = itemHandler;
    }

    /// <inheritdoc />
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    public int Count => _items.Count;

    /// <inheritdoc />
    bool ICollection<T>.IsReadOnly => false;

    /// <inheritdoc />
    public T this[int index]
    {
        get => _items[index];
        set => SetItem(index, value);
    }

    /// <inheritdoc />
    public void Add(T item) => InsertItem(_items.Count, item);

    /// <inheritdoc />
    public void Clear() => ClearItems();

    /// <inheritdoc />
    public bool Contains(T item) => _items.Contains(item);

    /// <inheritdoc />
    public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    /// <inheritdoc />
    public int IndexOf(T item) => _items.IndexOf(item);

    /// <inheritdoc />
    public void Insert(int index, T item) => InsertItem(index, item);

    /// <inheritdoc />
    public bool Remove(T item)
    {
        var index = _items.IndexOf(item);
        if (index < 0) return false;
        RemoveItem(index);
        return true;
    }

    /// <inheritdoc />
    public void RemoveAt(int index) => RemoveItem(index);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Suspends the notifications.
    /// </summary>
    public IDisposable SuspendNotifications()
    {
        _suspensionCount++;
        return new DisposableAction(ResumeNotifications);
    }

    private void ResumeNotifications() => _suspensionCount--;

    /// <inheritdoc />
    public void Refresh()
    {
        CheckReentrancy();
        OnCollectionRefreshed();
    }

    /// <inheritdoc />
    public void Move(int oldIndex, int newIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(oldIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(oldIndex, _items.Count);
        ArgumentOutOfRangeException.ThrowIfNegative(newIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(newIndex, _items.Count);

        if (oldIndex == newIndex) return;

        CheckReentrancy();
        var item = _items[oldIndex];
        _items.RemoveAt(oldIndex);
        _items.Insert(newIndex, item);

        OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);
        OnCollectionChanged(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex: oldIndex);
    }

    /// <inheritdoc />
    public void AddRange(IEnumerable<T> items)
    {
        InsertRange(_items.Count, items);
    }

    /// <inheritdoc />
    public void InsertRange(int index, IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, _items.Count);

        CheckReentrancy();
        var oldCount = _items.Count;

        if (items is ICollection<T> collection)
        {
            var insertedCount = collection.Count;
            if (ReferenceEquals(items, this))
                _items.InsertRange(index, _items);
            else
                _items.InsertRange(index, collection);

            for (var i = index; i < index + insertedCount; i++)
                _itemHandler?.OnItemAdded(_items[i]);
        }
        else
        {
            foreach (var item in items)
            {
                _items.Insert(index++, item);
                _itemHandler?.OnItemAdded(item);
            }
        }

        if (oldCount != _items.Count)
            OnCollectionRefreshed();
    }

    /// <inheritdoc />
    public int RemoveAll(Predicate<T> match)
    {
        ArgumentNullException.ThrowIfNull(match);
        CheckReentrancy();

        var removedItems = new List<T>();
        var removedCount = _items.RemoveAll(item =>
        {
            if (!match(item)) return false;
            removedItems.Add(item);
            return true;
        });

        foreach (var item in removedItems)
            _itemHandler?.OnItemRemoved(item);

        if (removedCount > 0)
            OnCollectionRefreshed();

        return removedCount;
    }

    /// <inheritdoc />
    public int RemoveRange(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (ReferenceEquals(items, this))
        {
            var count = _items.Count;
            ClearItems();
            return count;
        }

        CheckReentrancy();

        var oldCount = _items.Count;

        foreach (var item in items)
        {
            var index = _items.IndexOf(item);
            if (index < 0) continue;

            var removedItem = _items[index];
            _items.RemoveAt(index);
            _itemHandler?.OnItemRemoved(removedItem);
        }

        if (oldCount != _items.Count)
            OnCollectionRefreshed();

        return oldCount - _items.Count;
    }

    /// <inheritdoc />
    public void RemoveRange(int index, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, _items.Count);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (count > _items.Count - index)
            ThrowRangeExceedsCollectionBounds();

        if (count == 0)
            return;

        CheckReentrancy();
        var removedItems = _items.GetRange(index, count);
        _items.RemoveRange(index, count);
        foreach (var item in removedItems)
            _itemHandler?.OnItemRemoved(item);

        OnCollectionRefreshed();
    }

    private void InsertItem(int index, T item)
    {
        CheckReentrancy();
        _items.Insert(index, item);
        _itemHandler?.OnItemAdded(item);

        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
        OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);
        OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
    }

    private void RemoveItem(int index)
    {
        CheckReentrancy();
        var item = _items[index];
        _items.RemoveAt(index);
        _itemHandler?.OnItemRemoved(item);

        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
        OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);
        OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
    }

    private void SetItem(int index, T item)
    {
        CheckReentrancy();
        var oldItem = _items[index];
        _items[index] = item;
        _itemHandler?.OnItemRemoved(oldItem);
        _itemHandler?.OnItemAdded(item);

        // Indexer assignment is a replacement operation; notify even when oldItem equals item.
        OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);
        OnCollectionChanged(NotifyCollectionChangedAction.Replace, item, index, oldItem);
    }

    private void ClearItems()
    {
        if (_items.Count == 0) return;

        CheckReentrancy();
        var oldItems = _items.ToArray();
        _items.Clear();
        foreach (var item in oldItems)
            _itemHandler?.OnItemRemoved(item);

        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
        OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);
        OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
    }

    private void OnCollectionRefreshed()
    {
        if (_suspensionCount > 0) return;
        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
        OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);
        OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
    }

    private void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (_suspensionCount == 0)
            PropertyChanged?.Invoke(this, e);
    }

    private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (_suspensionCount > 0) return;

        var handlers = CollectionChanged;
        if (handlers is null) return;

        _notificationDepth++;
        try
        {
            handlers(this, e);
        }
        finally
        {
            _notificationDepth--;
        }
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, T item, int index, T? oldItem = default, int oldIndex = -1)
    {
        if (_suspensionCount > 0) return;

        var handlers = CollectionChanged;
        if (handlers is null) return;

        var args = action switch
        {
            NotifyCollectionChangedAction.Add => new NotifyCollectionChangedEventArgs(action, item, index),
            NotifyCollectionChangedAction.Remove => new NotifyCollectionChangedEventArgs(action, item, index),
            NotifyCollectionChangedAction.Move => new NotifyCollectionChangedEventArgs(action, item, index, oldIndex),
            NotifyCollectionChangedAction.Replace => new NotifyCollectionChangedEventArgs(action, oldItem, item, index),
            _ => EventArgsCache.ResetCollectionChanged
        };

        _notificationDepth++;
        try
        {
            handlers(this, args);
        }
        finally
        {
            _notificationDepth--;
        }
    }

    private void CheckReentrancy()
    {
        // Allow a single listener to mutate during notification for compatibility with selection controls;
        // with multiple listeners, a nested mutation could invalidate the change seen by later listeners.
        if (_notificationDepth > 0 && CollectionChanged is { HasSingleTarget: false })
            ThrowCannotChangeDuringCollectionChanged();
    }

    [DoesNotReturn]
    private static void ThrowRangeExceedsCollectionBounds() => throw new ArgumentException("The range exceeds the collection bounds.");

    [DoesNotReturn]
    private static void ThrowCannotChangeDuringCollectionChanged() => throw new InvalidOperationException("Cannot change the collection during a CollectionChanged event.");

    bool IList.IsReadOnly => false;
    bool IList.IsFixedSize => false;
    bool ICollection.IsSynchronized => false;
    object ICollection.SyncRoot => ((ICollection)_items).SyncRoot;
    void ICollection.CopyTo(Array array, int index) => ((ICollection)_items).CopyTo(array, index);

    object? IList.this[int index]
    {
        get => this[index];
        set => this[index] = ConvertItem(value);
    }

    int IList.Add(object? value)
    {
        Add(ConvertItem(value));
        return Count - 1;
    }

    bool IList.Contains(object? value) => IsCompatibleObject(value) && Contains((T)value!);
    int IList.IndexOf(object? value) => IsCompatibleObject(value) ? IndexOf((T)value!) : -1;
    void IList.Insert(int index, object? value) => Insert(index, ConvertItem(value));
    void IList.Remove(object? value)
    {
        if (IsCompatibleObject(value)) Remove((T)value!);
    }

    private static bool IsCompatibleObject(object? value) => value is T || (value is null && default(T) is null);

    private static T ConvertItem(object? value)
    {
        if (IsCompatibleObject(value)) return (T)value!;
        return ThrowIncompatibleValue(nameof(value));
    }

    [DoesNotReturn]
    private static T ThrowIncompatibleValue(string paramName) => throw new ArgumentException($"The value must be of type {typeof(T)} or null.", paramName);
}

internal static class EventArgsCache
{
    internal static readonly PropertyChangedEventArgs CountPropertyChanged = new("Count");
    internal static readonly PropertyChangedEventArgs IndexerPropertyChanged = new("Item[]");
    internal static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new(NotifyCollectionChangedAction.Reset);
}
