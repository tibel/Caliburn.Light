using System;
using System.Collections.Generic;

namespace Caliburn.Light
{
    /// <summary>
    /// A weak event source that does not hold any strong reference to the event listeners.
    /// </summary>
    /// <typeparam name="TEventHandler">Event delegate type.</typeparam>
    public abstract class WeakEventSourceBase<TEventHandler>
        where TEventHandler : Delegate
    {
        private readonly WeakEventList _list = new WeakEventList();

        /// <summary>
        /// Adds the specified event handler.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        public void Add(TEventHandler eventHandler)
        {
            if (eventHandler is null) return;
            lock (_list)
            {
                _list.AddHandler(eventHandler);
            }
        }

        /// <summary>
        /// Removes the specified event handler.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        public void Remove(TEventHandler eventHandler)
        {
            if (eventHandler is null) return;
            lock (_list)
            {
                _list.RemoveHandler(eventHandler);
            }
        }

        /// <summary>
        /// Gets the handlers to raise the event.
        /// </summary>
        /// <returns>The a</returns>
        protected IReadOnlyList<TEventHandler> GetHandlers()
        {
            lock (_list)
            {
                return _list.GetHandlers<TEventHandler>();
            }
        }
    }
}
