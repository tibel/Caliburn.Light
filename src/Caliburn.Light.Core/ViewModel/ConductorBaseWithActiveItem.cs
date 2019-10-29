using System.Collections.Generic;

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

        /// <summary>
        /// The currently active item.
        /// </summary>
        /// <value></value>
        object IHaveActiveItem.ActiveItem
        {
            get { return ActiveItem; }
            set { ActiveItem = (T) value; }
        }

        /// <summary>
        /// Changes the active item.
        /// </summary>
        /// <param name="newItem">The new item to activate.</param>
        /// <param name="closePrevious">Indicates whether or not to close the previous active item.</param>
        protected virtual void ChangeActiveItem(T newItem, bool closePrevious)
        {
            if (EqualityComparer<T>.Default.Equals(_activeItem, newItem))
                return;

            RaisePropertyChanging(nameof(ActiveItem));

            if (_activeItem is IDeactivate deactivator)
                deactivator.Deactivate(closePrevious);

            newItem = EnsureItem(newItem);

            if (IsActive && newItem is IActivate activator)
                activator.Activate();

            _activeItem = newItem;
            RaisePropertyChanged(nameof(ActiveItem));

            OnActivationProcessed(_activeItem, true);
        }
    }
}
