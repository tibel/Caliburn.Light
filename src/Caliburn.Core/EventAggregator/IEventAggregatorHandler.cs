using System;
using System.Threading.Tasks;

namespace Caliburn.Light
{
    /// <summary>
    /// An <see cref="IEventAggregator"/> handler.
    /// </summary>
    public interface IEventAggregatorHandler
    {
        /// <summary>
        /// Gets on which Thread the handler is executed.
        /// </summary>
        ThreadOption ThreadOption { get; }

        /// <summary>
        /// Gets a value indicating whether this handler is dead.
        /// </summary>
        bool IsDead { get; }

        /// <summary>
        /// Tries to retrieve the the internal target object and handler delegate.
        /// </summary>
        /// <param name="target">When this method returns, contains the target object, if it is available.</param>
        /// <param name="handler">When this method returns, contains the handler delegate, if it is available.</param>
        /// <returns>true if the values were retrieved; otherwise, false.</returns>
        bool TryGetTargetAndHandler(out object target, out Delegate handler);

        /// <summary>
        /// Determines whether this instance can handle the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>True, when the message can be handled.</returns>
        bool CanHandle(object message);

        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task HandleAsync(object message);
    }
}
