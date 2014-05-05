using System;
using System.Collections.Generic;
using Microsoft.Phone.Shell;

namespace Caliburn.Light
{
    /// <summary>
    /// Implemented by services that provide access to the basic phone capabilities.
    /// </summary>
    public interface IPhoneService {
        /// <summary>
        ///   The state that is persisted during the tombstoning process.
        /// </summary>
        IDictionary<string, object> State { get; }

        /// <summary>
        ///   Gets the mode in which the application was started.
        /// </summary>
        StartupMode StartupMode { get; }

        /// <summary>
        ///   Occurs when a fresh instance of the application is launching.
        /// </summary>
        event EventHandler<LaunchingEventArgs> Launching;

        /// <summary>
        ///   Occurs when a previously paused/tombstoned app is resumed/resurrected.
        /// </summary>
        event EventHandler<ActivatedEventArgs> Activated;

        /// <summary>
        ///   Occurs when the application is being paused or tombstoned.
        /// </summary>
        event EventHandler<DeactivatedEventArgs> Deactivated;

        /// <summary>
        ///   Occurs when the application is closing.
        /// </summary>
        event EventHandler<ClosingEventArgs> Closing;

        /// <summary>
        ///   Occurs when the app is continuing from a temporarily paused state.
        /// </summary>
        event EventHandler Continuing;

        /// <summary>
        ///   Occurs after the app has continued from a temporarily paused state.
        /// </summary>
        event EventHandler Continued;

        /// <summary>
        ///   Occurs when the app is "resurrecting" from a tombstoned state.
        /// </summary>
        event EventHandler Resurrecting;

        /// <summary>
        ///   Occurs after the app has "resurrected" from a tombstoned state.
        /// </summary>
        event EventHandler Resurrected;

        /// <summary>
        ///   Gets or sets whether user idle detection is enabled.
        /// </summary>
        IdleDetectionMode UserIdleDetectionMode { get; set; }

        /// <summary>
        ///   Gets or sets whether application idle detection is enabled.
        /// </summary>
        IdleDetectionMode ApplicationIdleDetectionMode { get; set; }

        /// <summary>
        /// Gets if the app is currently resurrecting.
        /// </summary>
        bool IsResurrecting { get; }
    }
}
