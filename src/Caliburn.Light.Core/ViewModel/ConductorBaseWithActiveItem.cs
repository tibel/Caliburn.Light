using System.ComponentModel;

namespace Caliburn.Light
{
    /// <summary>
    /// A base class for various implementations of <see cref="IConductor"/> that maintain an active item.
    /// </summary>
    /// <typeparam name="T">The type that is being conducted.</typeparam>
    public abstract class ConductorBaseWithActiveItem<T> : ConductorBase<T>, IConductActiveItem where T : class
    {
        private T _activeItem;

        /// <summary>
        /// The currently active item.
        /// </summary>
        public T ActiveItem
        {
            get { return _activeItem; }
            set { ActivateItem(value); }
        }

        object IHaveActiveItem.ActiveItem => ActiveItem;

        /// <summary>
        /// Sets the <see cref="ActiveItem"/> property to <paramref name="newItem"/>.
        /// </summary>
        /// <param name="newItem">The new item to set</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void SetActiveItem(T newItem)
        {
            SetProperty(ref _activeItem, newItem, nameof(ActiveItem));
        }
    }
}
