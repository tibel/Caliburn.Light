using System;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;

namespace Caliburn.Light
{
    /// <summary>
    /// Represents a collection of <see cref="T:Parameter"/> instances.
    /// </summary>
    public sealed class ParameterCollection : DependencyObjectCollection
    {
        /// <summary>
        /// Creates an instance of <see cref="ParameterCollection"/>
        /// </summary>
        public ParameterCollection()
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
                    foreach (var item in sender)
                    {
                        VerifyType(item);
                    }
                    break;
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private static void VerifyType(DependencyObject item)
        {
            if (!(item is Parameter)) throw new InvalidOperationException("An invalid item was added to the collection.");
        }
    }
}
