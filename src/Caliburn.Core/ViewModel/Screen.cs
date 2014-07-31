using System;

namespace Caliburn.Light
{
    /// <summary>
    /// A base implementation of <see cref = "IScreen" />.
    /// </summary>
    public class Screen : ViewAware, IScreen
    {
        private static readonly ILogger Log = LogManager.GetLogger(typeof(Screen));

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
            var handler = Activated;
            if (handler != null)
                handler(this, new ActivationEventArgs(wasInitialized));
        }

        private void OnDeactivating(bool wasClosed)
        {
            var handler = Deactivating;
            if (handler != null)
                handler(this, new DeactivationEventArgs(wasClosed));
        }

        private void OnDeactivated(bool wasClosed)
        {
            var handler = Deactivated;
            if (handler != null)
                handler(this, new DeactivationEventArgs(wasClosed));
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
                Log.Info("Deactivating {0}.", this);
                OnDeactivate(close);

                OnDeactivated(close);

                if (close)
                {
                    Views.Clear();
                    Log.Info("Closed {0}.", this);
                }
            }
        }

        /// <summary>
        /// Called when deactivating.
        /// </summary>
        /// <param name = "close">Inidicates whether this instance will be closed.</param>
        protected virtual void OnDeactivate(bool close)
        {
        }

        /// <summary>
        /// Called to check whether or not this instance can close.
        /// </summary>
        /// <param name = "callback">The implementor calls this action with the result of the close check.</param>
        public virtual void CanClose(Action<bool> callback)
        {
            callback(true);
        }

        /// <summary>
        /// Tries to close this instance by asking its Parent to initiate shutdown or by asking its corresponding view to close.
        /// Also provides an opportunity to pass a dialog result to it's corresponding view.
        /// </summary>
        /// <param name="dialogResult">The dialog result.</param>
        public virtual void TryClose(bool? dialogResult = null)
        {
            UIContext.TryClose(this, Views.Values, dialogResult);
        }
    }
}
