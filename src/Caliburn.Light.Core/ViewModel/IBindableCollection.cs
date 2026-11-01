using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Caliburn.Light;

/// <summary>
/// Represents a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed.
/// </summary>
/// <typeparam name="T">The type of elements contained in the collection.</typeparam>
public interface IBindableCollection<T> : IList<T>, INotifyCollectionChanged, IBindableObject
{
    /// <summary>
    /// Moves the item at the specified index to a new location in the collection.
    /// </summary>
    /// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param>
    /// <param name="newIndex">The zero-based index specifying the new location of the item.</param>
    void Move(int oldIndex, int newIndex);

    /// <summary>
    /// Adds the range.
    /// </summary>
    /// <param name="items">The items.</param>
    void AddRange(IEnumerable<T> items);

    /// <summary>
    /// Inserts a range of items into the collection at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which the items should be inserted.</param>
    /// <param name="items">The items to insert.</param>
    void InsertRange(int index, IEnumerable<T> items);

    /// <summary>
    /// Removes the range.
    /// </summary>
    /// <param name="items">The items.</param>
    int RemoveRange(IEnumerable<T> items);

    /// <summary>
    /// Removes a range of elements from the collection.
    /// </summary>
    /// <param name="index">The zero-based index of the first element to remove.</param>
    /// <param name="count">The number of elements to remove.</param>
    void RemoveRange(int index, int count);

    /// <summary>
    /// Removes all items that match the specified predicate.
    /// </summary>
    /// <param name="match">The predicate that determines whether an item should be removed.</param>
    /// <returns>The number of items removed from the collection.</returns>
    int RemoveAll(Predicate<T> match);
}
