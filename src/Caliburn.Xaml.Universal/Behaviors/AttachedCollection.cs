using Weakly;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;

namespace Caliburn.Light
{
    /// <summary>
    /// A collection that can exist as part of a behavior.
    /// </summary>
    /// <typeparam name="T">The type of item in the attached collection.</typeparam>
    public class AttachedCollection<T> : DependencyObjectCollection<T>, IAttachedObject
        where T : DependencyObject, IAttachedObject
    {
        private DependencyObject _associatedObject;

        /// <summary>
        /// Creates an instance of <see cref="AttachedCollection&lt;T&gt;"/>
        /// </summary>
        public AttachedCollection()
        {
            VectorChanged += OnVectorChanged;
        }

        /// <summary>
        /// Attaches the collection.
        /// </summary>
        /// <param name="dependencyObject">The dependency object to attach the collection to.</param>
        public void Attach(DependencyObject dependencyObject)
        {
            _associatedObject = dependencyObject;
            this.ForEach(x => ((T)x).Attach(_associatedObject));
        }

        /// <summary>
        /// Detaches the collection.
        /// </summary>
        public void Detach()
        {
            this.ForEach(x => ((T)x).Detach());
            _associatedObject = null;
        }

        DependencyObject IAttachedObject.AssociatedObject
        {
            get { return _associatedObject; }
        }

        /// <summary>
        /// Called when an item is added from the collection.
        /// </summary>
        /// <param name="item">The item that was added.</param>
        protected void OnItemAdded(T item)
        {
            if (_associatedObject != null)
                item.Attach(_associatedObject);
        }

        /// <summary>
        /// Called when an item is removed from the collection.
        /// </summary>
        /// <param name="item">The item that was removed.</param>
        protected void OnItemRemoved(T item)
        {
            if (item.AssociatedObject != null)
                item.Detach();
        }

        private void OnVectorChanged(IObservableVector<DependencyObject> sender, IVectorChangedEventArgs e)
        {
            switch (e.CollectionChange)
            {
                case CollectionChange.ItemInserted:
                case CollectionChange.ItemChanged:
                    OnItemAdded((T)this[(int)e.Index]);
                    break;
                case CollectionChange.ItemRemoved:
                    OnItemRemoved((T)this[(int)e.Index]);
                    break;
                case CollectionChange.Reset:
                    this.ForEach(x => OnItemRemoved((T)x));
                    this.ForEach(x => OnItemAdded((T)x));
                    break;
            }
        }
    }
}
