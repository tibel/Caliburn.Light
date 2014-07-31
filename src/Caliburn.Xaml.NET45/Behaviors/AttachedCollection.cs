using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Interactivity;
using Weakly;

namespace Caliburn.Light
{
    /// <summary>
    /// A collection that can exist as part of a behavior.
    /// </summary>
    /// <typeparam name="T">The type of item in the attached collection.</typeparam>
    public class AttachedCollection<T> : FreezableCollection<T>, IAttachedObject
        where T : DependencyObject, IAttachedObject
    {
        private DependencyObject _associatedObject;

        /// <summary>
        /// Creates an instance of <see cref="AttachedCollection{T}"/>
        /// </summary>
        public AttachedCollection()
        {
            ((INotifyCollectionChanged)this).CollectionChanged += OnCollectionChanged;
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <returns>The new instance.</returns>
        protected override Freezable CreateInstanceCore()
        {
            return new AttachedCollection<T>();
        }

        /// <summary>
        /// Attached the collection.
        /// </summary>
        /// <param name="dependencyObject">The dependency object to attach the collection to.</param>
        public void Attach(DependencyObject dependencyObject)
        {
            WritePreamble();
            _associatedObject = dependencyObject;
            WritePostscript();

            this.ForEach(x => x.Attach(_associatedObject));
        }

        /// <summary>
        /// Detaches the collection.
        /// </summary>
        public void Detach()
        {
            this.ForEach(x => x.Detach());
            WritePreamble();
            _associatedObject = null;
            WritePostscript();
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

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    e.NewItems.Cast<T>().Where(x => !Contains(x)).ForEach(OnItemAdded);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    e.OldItems.Cast<T>().ForEach(OnItemRemoved);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    e.OldItems.Cast<T>().ForEach(OnItemRemoved);
                    e.NewItems.Cast<T>().Where(x => !Contains(x)).ForEach(OnItemAdded);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    this.ForEach(OnItemRemoved);
                    this.ForEach(OnItemAdded);
                    break;
            }
        }
    }
}
