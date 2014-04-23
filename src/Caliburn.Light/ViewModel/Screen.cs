using System;

namespace Caliburn.Light
{
    /// <summary>
    /// A base implementation of <see cref = "IScreen" />.
    /// </summary>
    public class Screen : ViewAware, IScreen, IChild
    {
        private static readonly ILogger Log = LogManager.GetLogger(typeof(Screen));

        private bool _isActive;
        private bool _isInitialized;
        private object _parent;
        private string _displayName;

        /// <summary>
        /// Creates an instance of the screen.
        /// </summary>
        public Screen()
        {
            _displayName = GetType().FullName;
        }

        /// <summary>
        /// Gets or Sets the Parent <see cref = "IConductor" />
        /// </summary>
        public virtual object Parent
        {
            get { return _parent; }
            set { Set(ref _parent, value); }
        }

        /// <summary>
        /// Gets or Sets the Display Name
        /// </summary>
        public virtual string DisplayName
        {
            get { return _displayName; }
            set { Set(ref _displayName, value); }
        }

        /// <summary>
        /// Indicates whether or not this instance is currently active.
        /// </summary>
        public bool IsActive
        {
            get { return _isActive; }
            private set { Set(ref _isActive, value); }
        }

        /// <summary>
        /// Indicates whether or not this instance is currently initialized.
        /// </summary>
        public bool IsInitialized
        {
            get { return _isInitialized; }
            private set { Set(ref _isInitialized, value); }
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

        private void OnActivated(ActivationEventArgs args)
        {
            var handler = Activated;
            if (handler != null)
                handler(this, args);
        }

        private void OnDeactivating(DeactivationEventArgs args)
        {
            var handler = Deactivating;
            if (handler != null)
                handler(this, args);
        }

        private void OnDeactivated(DeactivationEventArgs args)
        {
            var handler = Deactivated;
            if (handler != null)
                handler(this, args);
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

            OnActivated(new ActivationEventArgs(initialized));
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
                OnDeactivating(new DeactivationEventArgs(close));

                IsActive = false;
                Log.Info("Deactivating {0}.", this);
                OnDeactivate(close);

                OnDeactivated(new DeactivationEventArgs(close));

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
            UIContext.View.TryClose(this, Views.Values, dialogResult);
        }
    }
}
