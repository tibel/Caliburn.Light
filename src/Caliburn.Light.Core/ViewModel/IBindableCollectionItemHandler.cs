namespace Caliburn.Light;

internal interface IBindableCollectionItemHandler<in T>
{
    void OnItemAdded(T item);
    void OnItemRemoved(T item);
}
