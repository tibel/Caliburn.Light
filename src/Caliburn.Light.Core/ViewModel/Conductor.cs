﻿using System;
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
            if (item is null)
            {
                await DeactivateItemAsync(ActiveItem, true);
                return;
            }

            var isActiveItem = ReferenceEquals(item, ActiveItem);
            if (isActiveItem)
            {
                if (IsActive)
                {
                    if (item is IActivate activeItem)
                        activeItem.Activate();

                    OnActivationProcessed(item, true);
                }

                return;
            }

            var result = await CloseStrategy.ExecuteAsync(new[] { ActiveItem });
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
        public override async Task DeactivateItemAsync(T item, bool close)
        {
            if (item is null || !ReferenceEquals(item, ActiveItem))
                return;

            if (close)
            {
                var result = await CloseStrategy.ExecuteAsync(new[] { item });
                if (result.CanClose)
                    ChangeActiveItem(null, true);
            }
            else
            {
                if (item is IDeactivate deactivator)
                    deactivator.Deactivate(false);
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
            return result.CanClose;
        }

        /// <summary>
        /// Called when activating.
        /// </summary>
        protected override void OnActivate()
        {
            if (ActiveItem is IActivate activator)
                activator.Activate();
        }

        /// <summary>
        /// Called when deactivating.
        /// </summary>
        /// <param name="close">Indicates whether this instance will be closed.</param>
        protected override void OnDeactivate(bool close)
        {
            if (close)
            {
                ChangeActiveItem(null, true);
            }
            else
            {
                if (ActiveItem is IDeactivate deactivator)
                    deactivator.Deactivate(false);
            }
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <returns>The collection of children.</returns>
        public override IEnumerable<T> GetChildren()
        {
            return ActiveItem is null ? Array.Empty<T>() : new[] {ActiveItem};
        }
    }
}
