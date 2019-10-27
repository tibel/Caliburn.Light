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
        /// Initializes a new instance of <see cref="ConductorBaseWithActiveItem&lt;T&gt;"/>.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        protected ConductorBaseWithActiveItem(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

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
            ScreenHelper.TryDeactivate(_activeItem, closePrevious);

            newItem = EnsureItem(newItem);

            if (IsActive)
                ScreenHelper.TryActivate(newItem);

            _activeItem = newItem;
            RaisePropertyChanged(nameof(ActiveItem));

            OnActivationProcessed(_activeItem, true);
        }
    }
}
