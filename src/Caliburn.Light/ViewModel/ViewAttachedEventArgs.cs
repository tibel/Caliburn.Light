using System;

namespace Caliburn.Light
{
    /// <summary>
    /// The event args for the <see cref="IViewAware.ViewAttached"/> event.
    /// </summary>
    public class ViewAttachedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewAttachedEventArgs"/> class.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="context">The context.</param>
        public ViewAttachedEventArgs(object view, object context)
        {
            View = view;
            Context = context;
        }

        /// <summary>
        /// The view.
        /// </summary>
        public object View { get; private set; }

        /// <summary>
        /// The context.
        /// </summary>
        public object Context { get; private set; }
    }
}
