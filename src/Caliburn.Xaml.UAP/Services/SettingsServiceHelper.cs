using System;
using System.Collections.Generic;

namespace Caliburn.Light
{
    /// <summary>
    /// Extensions for <see cref="ISettingsService"/>.
    /// </summary>
    public static class SettingsServiceHelper
    {
        /// <summary>
        /// Registers a flyout command with the service.
        /// </summary>
        /// <typeparam name="TViewModel">The commands view model.</typeparam>
        /// <param name="settingsService">The settings service.</param>
        /// <param name="label">The command label.</param>
        /// <param name="viewSettings">The optional flyout view settings.</param>
        public static void RegisterFlyoutCommand<TViewModel>(this ISettingsService settingsService, string label, IDictionary<string, object> viewSettings = null)
        {
            settingsService.RegisterCommand(new FlyoutSettingsCommand(label, typeof(TViewModel), viewSettings));
        }

        /// <summary>
        /// Registers a URI command with the service.
        /// </summary>
        /// <param name="settingsService">The settings service.</param>
        /// <param name="label">The label.</param>
        /// <param name="uri">The URI.</param>
        public static void RegisterUriCommand(this ISettingsService settingsService, string label, Uri uri)
        {
            settingsService.RegisterCommand(new UriSettingsCommand(label, uri));
        }
    }
}
