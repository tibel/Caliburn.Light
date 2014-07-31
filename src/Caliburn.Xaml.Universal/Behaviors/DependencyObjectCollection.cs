using System;
using Weakly;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;

namespace Caliburn.Light
{
    /// <summary>
    /// Represents a collection of <see cref="T:DependencyObject"/> instances of a specified type.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    public class DependencyObjectCollection<T> : DependencyObjectCollection
        where T : DependencyObject
    {
        /// <summary>
        /// Creates an instance of <see cref="DependencyObjectCollection&lt;T&gt;"/>
        /// </summary>
        public DependencyObjectCollection()
        {
            VectorChanged += OnVectorChanged;
        }

        private static void OnVectorChanged(IObservableVector<DependencyObject> sender, IVectorChangedEventArgs e)
        {
            switch (e.CollectionChange)
            {
                case CollectionChange.ItemInserted:
                case CollectionChange.ItemChanged:
                    VerifyType(sender[(int)e.Index]);
                    break;
                case CollectionChange.Reset:
                    sender.ForEach(VerifyType);
                    break;
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private static void VerifyType(DependencyObject item)
        {
            if (!(item is T)) throw new InvalidOperationException("An invalid item was added to the collection.");
        }
    }
}
