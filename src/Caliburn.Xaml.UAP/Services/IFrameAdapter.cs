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
    }
}
