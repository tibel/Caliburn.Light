using System;

namespace Caliburn.Light
{
    /// <summary>
    /// Denotes an instance which requires deactivation.
    /// </summary>
    public interface IDeactivate
    {
        /// <summary>
        /// Raised before deactivation.
        /// </summary>
        event EventHandler<DeactivationEventArgs> Deactivating;

        /// <summary>
        /// Deactivates this instance.
        /// </summary>
        /// <param name="close">Indicates whether or not this instance is being closed.</param>
        void Deactivate(bool close);

        /// <summary>
        /// Raised after deactivation.
        /// </summary>
        event EventHandler<DeactivationEventArgs> Deactivated;
    }
}
