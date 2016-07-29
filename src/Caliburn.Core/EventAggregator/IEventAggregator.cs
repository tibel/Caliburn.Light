using System;
using System.Threading.Tasks;

namespace Caliburn.Light
{
    /// <summary>
    /// Enables loosely-coupled publication of and subscription to events.
    /// </summary>
    public interface IEventAggregator
    {
        /// <summary>
        /// Subscribes the specified handler for messages of type <typeparamref name="TMessage"/>.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="target">The message handler target.</param>
        /// <param name="handler">The message handler to register.</param>
        /// <param name="threadOption">Specifies on which Thread the <paramref name="handler"/> is executed.</param>
        /// <returns>The <see cref="IEventAggregatorHandler"/>.</returns>
        IEventAggregatorHandler Subscribe<TMessage>(object target, Action<TMessage> handler, ThreadOption threadOption = ThreadOption.PublisherThread);

        /// <summary>
        /// Subscribes the specified handler for messages of type <typeparamref name="TMessage"/>.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="target">The message handler target.</param>
        /// <param name="handler">The message handler to register.</param>
        /// <param name="threadOption">Specifies on which Thread the <paramref name="handler"/> is executed.</param>
        /// <returns>The <see cref="IEventAggregatorHandler"/>.</returns>
        IEventAggregatorHandler Subscribe<TMessage>(object target, Func<TMessage, Task> handler, ThreadOption threadOption = ThreadOption.PublisherThread);

        /// <summary>
        /// Unsubscribes the specified handler.
        /// </summary>
        /// <param name="handler">The handler to unsubscribe.</param>
        void Unsubscribe(IEventAggregatorHandler handler);

        /// <summary>
        /// Publishes a message.
        /// </summary>
        /// <param name="message">The message instance.</param>
        void Publish(object message);
    }
}
