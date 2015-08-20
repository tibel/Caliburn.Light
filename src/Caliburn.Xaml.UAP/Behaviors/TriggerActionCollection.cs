using System;
using Weakly;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;

namespace Caliburn.Light
{
    /// <summary>
    /// Represents a collection of <see cref="T:TriggerAction"/> instances.
    /// </summary>
    public sealed class TriggerActionCollection : DependencyObjectCollection, IAttachedObject
    {
        private DependencyObject _associatedObject;

        /// <summary>
        /// Creates an instance of <see cref="TriggerActionCollection"/>
        /// </summary>
        public TriggerActionCollection()
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
            this.ForEach(x => ((TriggerAction)x).Attach(_associatedObject));
        }

        /// <summary>
        /// Detaches the collection.
        /// </summary>
        public void Detach()
        {
            this.ForEach(x => ((TriggerAction)x).Detach());
            _associatedObject = null;
        }

        DependencyObject IAttachedObject.AssociatedObject
        {
            get { return _associatedObject; }
        }

        private static void OnVectorChanged(IObservableVector<DependencyObject> sender, IVectorChangedEventArgs e)
        {
            var associatedObject = ((TriggerActionCollection) sender)._associatedObject;

            switch (e.CollectionChange)
            {
                case CollectionChange.ItemInserted:
                case CollectionChange.ItemChanged:
                    var item = sender[(int) e.Index];
                    VerifyType(item);
                    OnItemAdded(item, associatedObject);
                    break;
                case CollectionChange.ItemRemoved:
                    OnItemRemoved(sender[(int)e.Index]);
                    break;
                case CollectionChange.Reset:
                    sender.ForEach(VerifyType);
                    sender.ForEach(OnItemRemoved);
                    sender.ForEach(x => OnItemAdded(x, associatedObject));
                    break;
            }
        }

        private static void OnItemAdded(DependencyObject item, DependencyObject associatedObject)
        {
            var attachable = (IAttachedObject)item;
            if (associatedObject != null)
                attachable.Attach(associatedObject);
        }

        private static void OnItemRemoved(DependencyObject item)
        {
            var attachable = (IAttachedObject)item;
            if (attachable.AssociatedObject != null)
                attachable.Detach();
        }

        // ReSharper disable once UnusedParameter.Local
        private static void VerifyType(DependencyObject item)
        {
            if (!(item is TriggerAction)) throw new InvalidOperationException("An invalid item was added to the collection.");
        }
    }
}
