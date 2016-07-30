using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace Caliburn.Light
{
    /// <summary>
    /// The interface to attach the framework to a <see cref="Frame"/>.
    /// </summary>
    public interface IFrameAdapter
    {
        /// <summary>
        /// Attaches this instance to the <paramref name="frame"/>.
        /// </summary>
        /// <param name="frame">The frame to attach to.</param>
        void AttachTo(Frame frame);

        /// <summary>
        /// Detachtes this instrance from the <paramref name="frame"/>.
        /// </summary>
        /// <param name="frame">The frame to detatch from.</param>
        void DetatchFrom(Frame frame);

        /// <summary>
        /// Save the current state for <paramref name="frame"/>.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns>The internal frame state dictionary.</returns>
        IDictionary<string, object> SaveState(Frame frame);

        /// <summary>
        ///  Restores previously saved for <paramref name="frame"/>.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="frameState">The state dictionary that will be used.</param>
        void RestoreState(Frame frame, IDictionary<string, object> frameState);
    }
}
