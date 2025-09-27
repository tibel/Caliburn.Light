using System;
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
        public override async Task ActivateItemAsync(T item)
        {
            if (ReferenceEquals(ActiveItem, item))
            {
                if (IsActive && item is not null)
                {
                    if (item is IActivatable activeItem)
                        await activeItem.ActivateAsync();

                    OnActivationProcessed(item, true);
                }

                return;
            }

            if (ActiveItem is null)
            {
                await ChangeActiveItemAsync(item, true);
                return;
            }

            var result = await CloseStrategy.ExecuteAsync(new[] { ActiveItem });
            if (result.CanClose)
                await ChangeActiveItemAsync(item, true);
            else
                OnActivationProcessed(item, false);
        }

        /// <summary>
        /// Deactivates the specified item.
        /// </summary>
        /// <param name="item">The item to deactivate.</param>
        /// <param name="close">Indicates whether or not to close the item after deactivating it.</param>
        public override async Task DeactivateItemAsync(T item, bool close)
        {
            if (item is null || !ReferenceEquals(item, ActiveItem))
                return;

            if (close)
            {
                var result = await CloseStrategy.ExecuteAsync(new[] { item });
                if (result.CanClose)
                    await ChangeActiveItemAsync(null, true);
            }
            else
            {
                if (item is IActivatable deactivator)
                    await deactivator.DeactivateAsync(false);
            }
        }

        /// <summary>
        /// Called to check whether or not this instance can close.
        /// </summary>
        /// <returns>A task containing the result of the close check.</returns>
        public override async Task<bool> CanCloseAsync()
        {
            if (ActiveItem is null)
                return true;

            var result = await CloseStrategy.ExecuteAsync(new[] { ActiveItem });

            if (!result.CanClose && result.Closeables.Count > 0)
                await ChangeActiveItemAsync(null, true);

            return result.CanClose;
        }

        /// <summary>
        /// Called when activating.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            if (ActiveItem is IActivatable activator)
                await activator.ActivateAsync();
        }

        /// <summary>
        /// Called when deactivating.
        /// </summary>
        /// <param name="close">Indicates whether this instance will be closed.</param>
        protected override async Task OnDeactivateAsync(bool close)
        {
            if (close)
            {
                await ChangeActiveItemAsync(null, true);
            }
            else
            {
                if (ActiveItem is IActivatable deactivator)
                    await deactivator.DeactivateAsync(false);
            }
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <returns>The collection of children.</returns>
        public override IReadOnlyList<T> GetChildren()
        {
            return ActiveItem is null ? Array.Empty<T>() : new[] { ActiveItem };
        }
    }
}
