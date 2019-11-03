using System;
using System.Threading.Tasks;

namespace Caliburn.Light
{
    /// <summary>
    /// A base implementation of <see cref = "IScreen" />.
    /// </summary>
    public class Screen : ViewAware, IScreen
    {
        private bool _isActive;
        private bool _isInitialized;

        /// <summary>
        /// Indicates whether or not this instance is currently active.
        /// </summary>
        public bool IsActive
        {
            get { return _isActive; }
            private set { SetProperty(ref _isActive, value); }
        }

        /// <summary>
        /// Indicates whether or not this instance is currently initialized.
        /// </summary>
        public bool IsInitialized
        {
            get { return _isInitialized; }
            private set { SetProperty(ref _isInitialized, value); }
        }

        /// <summary>
        /// Raised after activation occurs.
        /// </summary>
        public event EventHandler<ActivationEventArgs> Activated;

        /// <summary>
        /// Raised before deactivation.
        /// </summary>
        public event EventHandler<DeactivationEventArgs> Deactivating;

        /// <summary>
        /// Raised after deactivation.
        /// </summary>
        public event EventHandler<DeactivationEventArgs> Deactivated;

        private void OnActivated(bool wasInitialized)
        {
            Activated?.Invoke(this, new ActivationEventArgs(wasInitialized));
        }

        private void OnDeactivating(bool wasClosed)
        {
            Deactivating?.Invoke(this, new DeactivationEventArgs(wasClosed));
        }

        private void OnDeactivated(bool wasClosed)
        {
            Deactivated?.Invoke(this, new DeactivationEventArgs(wasClosed));
        }

        void IActivate.Activate()
        {
            if (IsActive) return;

            var initialized = false;
            if (!IsInitialized)
            {
                IsInitialized = initialized = true;
                OnInitialize();
            }

            IsActive = true;
            Log.Info("Activating {0}.", this);
            OnActivate();

            OnActivated(initialized);
        }

        /// <summary>
        /// Called when initializing.
        /// </summary>
        protected virtual void OnInitialize()
        {
        }

        /// <summary>
        /// Called when activating.
        /// </summary>
        protected virtual void OnActivate()
        {
        }

        void IDeactivate.Deactivate(bool close)
        {
            if (IsActive || (IsInitialized && close))
            {
                OnDeactivating(close);

                IsActive = false;
                Log.Info("Deactivating {0} (close={1}).", this, close);
                OnDeactivate(close);

                OnDeactivated(close);
            }
        }

        /// <summary>
        /// Called when deactivating.
        /// </summary>
        /// <param name = "close">Indicates whether this instance will be closed.</param>
        protected virtual void OnDeactivate(bool close)
        {
        }

        /// <summary>
        /// Called to check whether or not this instance can close.
        /// </summary>
        /// <returns>A task containing the result of the close check.</returns>
        public virtual Task<bool> CanCloseAsync()
        {
            return TaskHelper.TrueTask;
        }

        /// <summary>
        /// Tries to close this instance by asking its Parent to initiate shutdown or by asking its corresponding view to close.
        /// Also provides an opportunity to pass a dialog result to it's corresponding view.
        /// </summary>
        /// <param name="dialogResult">The dialog result.</param>
        public virtual void TryClose(bool? dialogResult = null)
        {
            if (this is IChild child && child.Parent is IConductor conductor)
            {
                conductor.DeactivateItem(this, true);
                return;
            }

            foreach (var entry in Views)
            {
                if (ViewHelper.TryClose(entry.Value.Target, dialogResult))
                    return;
            }

            Log.Info("TryClose requires an IChild.Parent of IConductor or a view with a Close method or IsOpen property.");
        }
    }
}
