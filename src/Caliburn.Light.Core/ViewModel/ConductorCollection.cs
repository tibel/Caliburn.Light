namespace Caliburn.Light;

public partial class Conductor<T>
{
    public static partial class Collection
    {
        private sealed class ConductorCollection : BindableCollection<T>
        {
            private readonly object _parent;

            public ConductorCollection(object parent)
            {
                _parent = parent;
            }

            protected override void OnItemAdded(T item)
            {
                if (item is IParentAware parentAware)
                    parentAware.AttachParent(_parent);
            }

            protected override void OnItemRemoved(T item)
            {
                if (item is IParentAware parentAware)
                    parentAware.DetachParent(_parent);
            }
        }
    }
}
