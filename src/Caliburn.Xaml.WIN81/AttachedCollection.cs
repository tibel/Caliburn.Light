using System.Linq;
using Weakly;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;

namespace Caliburn.Xaml
{
    /// <summary>
    /// A collection that can exist as part of a behavior.
    /// </summary>
    /// <typeparam name="T">The type of item in the attached collection.</typeparam>
    public class AttachedCollection<T> : DependencyObjectCollection, IAttachedObject
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
            this.OfType<T>().ForEach(x => x.Attach(_associatedObject));
        }

        /// <summary>
        /// Detaches the collection.
        /// </summary>
        public void Detach()
        {
            this.OfType<T>().ForEach(x => x.Detach());
            _associatedObject = null;
        }

        /// <summary>
        /// The currently attached object.
        /// </summary>
        public DependencyObject AssociatedObject
        {
            get { return _associatedObject; }
        }

        /// <summary>
        /// Called when an item is added from the collection.
        /// </summary>
        /// <param name="item">The item that was added.</param>
        protected void OnItemAdded(DependencyObject item)
        {
            var attachable = item as T;
            if (_associatedObject != null && attachable != null)
                attachable.Attach(_associatedObject);
        }

        /// <summary>
        /// Called when an item is removed from the collection.
        /// </summary>
        /// <param name="item">The item that was removed.</param>
        protected void OnItemRemoved(DependencyObject item)
        {
            var attachable = item as T;
            if (attachable != null && attachable.AssociatedObject != null)
                attachable.Detach();
        }

        private void OnVectorChanged(IObservableVector<DependencyObject> sender, IVectorChangedEventArgs e)
        {
            switch (e.CollectionChange)
            {
                case CollectionChange.ItemInserted:
                    OnItemAdded(this[(int)e.Index]);
                    break;
                case CollectionChange.ItemRemoved:
                    OnItemRemoved(this[(int)e.Index]);
                    break;
                case CollectionChange.Reset:
                    this.ForEach(OnItemRemoved);
                    this.ForEach(OnItemAdded);
                    break;
            }
        }
    }
}
