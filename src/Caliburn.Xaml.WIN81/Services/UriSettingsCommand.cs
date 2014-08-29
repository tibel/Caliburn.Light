using System;
using Weakly;
using Windows.System;

namespace Caliburn.Light
{
    /// <summary>
    /// Represents a URI command registered with the <see cref="ISettingsService" />.
    /// </summary>
    public class UriSettingsCommand : SettingsCommandBase
    {
        private readonly Uri _uri;

        /// <summary>
        /// Initializes a new instance of the <see cref="UriSettingsCommand"/> class.
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="uri">The URI.</param>
        public UriSettingsCommand(string label, Uri uri) : base(label)
        {
            _uri = uri;
        }

        /// <summary>
        /// Gets the URI.
        /// </summary>
        public Uri Uri
        {
            get { return _uri; }
        }

        /// <summary>
        /// Called when the command was selected in the Settings Charm.
        /// </summary>
        public override void OnSelected()
        {
            Launcher.LaunchUriAsync(Uri).AsTask().ObserveException();
        }
    }
}
