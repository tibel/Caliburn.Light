namespace Caliburn.Light;

/// <summary>
/// Handles items added to or removed from a <see cref="BindableCollection{T}"/>.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public interface IBindableCollectionItemHandler<in T>
{
    /// <summary>
    /// Called when an item is added to the collection.
    /// </summary>
    /// <param name="item">The item that was added.</param>
    void OnItemAdded(T item);

    /// <summary>
    /// Called when an item is removed from the collection.
    /// </summary>
    /// <param name="item">The item that was removed.</param>
    void OnItemRemoved(T item);
}
