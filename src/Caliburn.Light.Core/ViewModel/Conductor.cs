using System.Collections.Generic;
using System.Threading.Tasks;

namespace Caliburn.Light
{
    /// <summary>
    /// An implementation of <see cref="IConductor"/> that holds on to and activates only one item at a time.
    /// </summary>
    public partial class Conductor<T> : ConductorBaseWithActiveItem<T> where T : class
    {
        /// <summary>
        /// Activates the specified item.
        /// </summary>
        /// <param name="item">The item to activate.</param>
        public override async void ActivateItem(T item)
        {
            if (item is object && ReferenceEquals(item, ActiveItem))
            {
                if (IsActive)
                {
                    ScreenHelper.TryActivate(item);
                    OnActivationProcessed(item, true);
                }
                return;
            }

            var result = await CloseStrategy.ExecuteAsync(new[] {ActiveItem});
            if (result.CanClose)
                ChangeActiveItem(item, true);
            else
                OnActivationProcessed(item, false);
        }

        /// <summary>
        /// Deactivates the specified item.
        /// </summary>
        /// <param name="item">The item to close.</param>
        /// <param name="close">Indicates whether or not to close the item after deactivating it.</param>
        public override async void DeactivateItem(T item, bool close)
        {
            if (item is null || !ReferenceEquals(item, ActiveItem))
            {
                return;
            }

            var result = await CloseStrategy.ExecuteAsync(new[] {ActiveItem});
            if (result.CanClose)
                ChangeActiveItem(default(T), close);
        }

        /// <summary>
        /// Called to check whether or not this instance can close.
        /// </summary>
        /// <returns>A task containing the result of the close check.</returns>
        public override async Task<bool> CanCloseAsync()
        {
            var result = await CloseStrategy.ExecuteAsync(new[] { ActiveItem });
            return result.CanClose;
        }

        /// <summary>
        /// Called when activating.
        /// </summary>
        protected override void OnActivate()
        {
            ScreenHelper.TryActivate(ActiveItem);
        }

        /// <summary>
        /// Called when deactivating.
        /// </summary>
        /// <param name="close">Indicates whether this instance will be closed.</param>
        protected override void OnDeactivate(bool close)
        {
            ScreenHelper.TryDeactivate(ActiveItem, close);
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <returns>The collection of children.</returns>
        public override IEnumerable<T> GetChildren()
        {
            return new[] {ActiveItem};
        }
    }
}
