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
        /// <param name="handler">The message handler to register.</param>
        /// <param name="threadOption">Specifies on which Thread the <paramref name="handler"/> is executed.</param>
        void Subscribe<TMessage>(Action<TMessage> handler, ThreadOption threadOption = ThreadOption.PublisherThread);

        /// <summary>
        /// Subscribes the specified handler for messages of type <typeparamref name="TMessage"/>.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="handler">The message handler to register.</param>
        /// <param name="threadOption">Specifies on which Thread the <paramref name="handler"/> is executed.</param>
        void SubscribeAsync<TMessage>(Func<TMessage, Task> handler, ThreadOption threadOption = ThreadOption.PublisherThread);

        /// <summary>
        /// Unsubscribes the specified handler.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="handler">The handler to unsubscribe.</param>
        void Unsubscribe<TMessage>(Action<TMessage> handler);

        /// <summary>
        /// Unsubscribes the specified handler.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="handler">The handler to unsubscribe.</param>
        void UnsubscribeAsync<TMessage>(Func<TMessage, Task> handler);

        /// <summary>
        /// Publishes a message.
        /// </summary>
        /// <param name="message">The message instance.</param>
        void Publish(object message);
    }
}
