using System;

namespace Caliburn.Light
{
    /// <summary>
    /// The event args for the <see cref="ICoTask.Completed"/> event.
    /// </summary>
    public sealed class CoTaskCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoTaskCompletedEventArgs"/> class.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="wasCancelled">if set to <c>true</c> the <see cref="ICoTask"/> was canceled.</param>
        public CoTaskCompletedEventArgs(Exception error, bool wasCancelled)
        {
            Error = error;
            WasCancelled = wasCancelled;
        }

        /// <summary>
        /// Gets the error if one occurred.
        /// </summary>
        /// <value>The error.</value>
        public Exception Error { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ICoTask"/> was cancelled.
        /// </summary>
        /// <value><c>true</c> if cancelled; otherwise, <c>false</c>.</value>
        public bool WasCancelled { get; private set; }
    }
}
