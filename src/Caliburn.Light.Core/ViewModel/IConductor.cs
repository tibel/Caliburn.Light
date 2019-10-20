﻿using System;

namespace Caliburn.Light
{
    /// <summary>
    /// Denotes an instance which conducts other objects by maintaining a strict lifecycle.
    /// </summary>
    /// <remarks>
    /// Conducted instances can opt-in to the lifecycle by impelenting any of the follosing 
    /// <see cref="IActivate"/>, <see cref="IDeactivate"/>, <see cref="ICloseGuard"/>, <see cref="IChild"/>.
    /// </remarks>
    public interface IConductor : IParent, IBindableObject
    {
        /// <summary>
        /// Activates the specified item.
        /// </summary>
        /// <param name="item">The item to activate.</param>
        void ActivateItem(object item);

        /// <summary>
        /// Deactivates the specified item.
        /// </summary>
        /// <param name="item">The item to close.</param>
        /// <param name="close">Indicates whether or not to close the item after deactivating it.</param>
        void DeactivateItem(object item, bool close);

        /// <summary>
        /// Occurs when an activation request is processed.
        /// </summary>
        event EventHandler<ActivationProcessedEventArgs> ActivationProcessed;
    }

    /// <summary>
    /// An <see cref="IConductor"/> that also implements <see cref="IHaveActiveItem"/>.
    /// </summary>
    public interface IConductActiveItem : IConductor, IHaveActiveItem
    {
    }
}
