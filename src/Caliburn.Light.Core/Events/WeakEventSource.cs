﻿using System;

namespace Caliburn.Light
{
    /// <summary>
    /// A weak event source that does not hold any strong reference to the event listeners.
    /// </summary>
    public sealed class WeakEventSource : WeakEventSourceBase<EventHandler>
    {
        /// <summary>
        /// Raises the event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An object that contains the event data.</param>
        public void Raise(object sender, EventArgs e)
        {
            var handlers = GetHandlers();

            for (var i = 0; i < handlers.Count; i++)
            {
                handlers[i].Invoke(sender, e);
            }
        }
    }

    /// <summary>
    /// A weak event source that does not hold any strong reference to the event listeners.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event data generated by the event.</typeparam>
    public sealed class WeakEventSource<TEventArgs> : WeakEventSourceBase<EventHandler<TEventArgs>>
        where TEventArgs : EventArgs
    {
        /// <summary>
        /// Raises the event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An object that contains the event data.</param>
        public void Raise(object sender, TEventArgs e)
        {
            var handlers = GetHandlers();

            for (var i = 0; i < handlers.Count; i++)
            {
                handlers[i].Invoke(sender, e);
            }
        }
    }
}
