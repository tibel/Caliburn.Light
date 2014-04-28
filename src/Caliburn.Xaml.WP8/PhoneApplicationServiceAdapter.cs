using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Shell;

namespace Caliburn.Xaml
{
    /// <summary>
    /// An implementation of <see cref = "IPhoneService" /> that adapts <see cref = "PhoneApplicationService" />.
    /// </summary>
    public class PhoneApplicationServiceAdapter : IPhoneService {
        readonly PhoneApplicationService _service;

        /// <summary>
        ///   Creates an instance of <see cref = "PhoneApplicationServiceAdapter" />.
        /// </summary>
        public PhoneApplicationServiceAdapter(PhoneApplicationService phoneApplicationServiceservice, Frame rootFrame) {
            _service = phoneApplicationServiceservice;
            _service.Activated += (sender, args) =>
            {
                if (!args.IsApplicationInstancePreserved)
                {
                    IsResurrecting = true;
                    OnResurrecting();
                    NavigatedEventHandler onNavigated = null;
                    onNavigated = (s2, e2) =>
                    {
                        IsResurrecting = false;
                        OnResurrected();
                        rootFrame.Navigated -= onNavigated;
                    };
                    rootFrame.Navigated += onNavigated;
                }
                else
                {
                    OnContinuing();
                    NavigatedEventHandler onNavigated = null;
                    onNavigated = (s2, e2) =>
                    {
                        OnContinued();
                        rootFrame.Navigated -= onNavigated;
                    };
                    rootFrame.Navigated += onNavigated;
                }
            };
        }

        /// <summary>
        /// Gets if the app is currently resurrecting.
        /// </summary>
        public bool IsResurrecting { get; private set; }

        /// <summary>
        ///   The state that is persisted during the tombstoning process.
        /// </summary>
        public IDictionary<string, object> State {
            get { return _service.State; }
        }

        /// <summary>
        ///   Gets the mode in which the application was started.
        /// </summary>
        public StartupMode StartupMode {
            get { return _service.StartupMode; }
        }

        /// <summary>
        ///   Occurs when a fresh instance of the application is launching.
        /// </summary>
        public event EventHandler<LaunchingEventArgs> Launching {
            add { _service.Launching += value; }
            remove { _service.Launching -= value; }
        }

        /// <summary>
        ///   Occurs when a previously paused/tombstoned application instance is resumed/resurrected.
        /// </summary>
        public event EventHandler<ActivatedEventArgs> Activated {
            add { _service.Activated += value; }
            remove { _service.Activated -= value; }
        }

        /// <summary>
        ///   Occurs when the application is being paused or tombstoned.
        /// </summary>
        public event EventHandler<DeactivatedEventArgs> Deactivated {
            add { _service.Deactivated += value; }
            remove { _service.Deactivated -= value; }
        }

        /// <summary>
        ///   Occurs when the application is closing.
        /// </summary>
        public event EventHandler<ClosingEventArgs> Closing {
            add { _service.Closing += value; }
            remove { _service.Closing -= value; }
        }

        /// <summary>
        ///   Occurs when the app is continuing from a temporarily paused state.
        /// </summary>
        public event EventHandler Continuing;

        /// <summary>
        ///   Occurs after the app has continued from a temporarily paused state.
        /// </summary>
        public event EventHandler Continued;

        /// <summary>
        ///   Occurs when the app is "resurrecting" from a tombstoned state.
        /// </summary>
        public event EventHandler Resurrecting;

        /// <summary>
        ///   Occurs after the app has "resurrected" from a tombstoned state.
        /// </summary>
        public event EventHandler Resurrected;

        private void OnContinuing()
        {
            var handler = Continuing;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void OnContinued()
        {
            var handler = Continued;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void OnResurrecting()
        {
            var handler = Resurrecting;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void OnResurrected()
        {
            var handler = Resurrected;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        /// <summary>
        ///   Gets or sets whether user idle detection is enabled.
        /// </summary>
        public IdleDetectionMode UserIdleDetectionMode {
            get { return _service.UserIdleDetectionMode; }
            set { _service.UserIdleDetectionMode = value; }
        }

        /// <summary>
        ///   Gets or sets whether application idle detection is enabled.
        /// </summary>
        public IdleDetectionMode ApplicationIdleDetectionMode {
            get { return _service.ApplicationIdleDetectionMode; }
            set { _service.ApplicationIdleDetectionMode = value; }
        }
    }
}
