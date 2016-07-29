using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Weakly;

namespace Caliburn.Light
{
    /// <summary>
    /// Enables loosely-coupled publication of and subscription to events.
    /// </summary>
    public sealed class EventAggregator : IEventAggregator
    {
        private readonly object _lockObject = new object();
        private readonly WeakDelegateTable _table = new WeakDelegateTable();
        private readonly List<IEventAggregatorHandler> _handlers = new List<IEventAggregatorHandler>();

        /// <summary>
        /// Subscribes the specified handler for messages of type <typeparamref name="TMessage" />.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="target">The message handler target.</param>
        /// <param name="handler">The message handler to register.</param>
        /// <param name="threadOption">Specifies on which Thread the <paramref name="handler" /> is executed.</param>
        /// <returns>The <see cref="IEventAggregatorHandler" />.</returns>
        public IEventAggregatorHandler Subscribe<TMessage>(object target, Action<TMessage> handler, ThreadOption threadOption)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            Func<TMessage, Task> wrapper = m =>
            {
                handler(m);
                return TaskHelper.Completed();
            };

            return SubscribeCore(target, wrapper, threadOption);
        }

        /// <summary>
        /// Subscribes the specified handler for messages of type <typeparamref name="TMessage" />.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="target">The message handler target.</param>
        /// <param name="handler">The message handler to register.</param>
        /// <param name="threadOption">Specifies on which Thread the <paramref name="handler" /> is executed.</param>
        /// <returns>The <see cref="IEventAggregatorHandler" />.</returns>
        public IEventAggregatorHandler Subscribe<TMessage>(object target, Func<TMessage, Task> handler, ThreadOption threadOption)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            return SubscribeCore(target, handler, threadOption);
        }

        private IEventAggregatorHandler SubscribeCore<TMessage>(object target, Func<TMessage, Task> handler, ThreadOption threadOption)
        {
            var item = new EventAggregatorHandler<TMessage>(target, handler, threadOption);

            lock (_lockObject)
            {
                _handlers.RemoveAll(h => h.IsDead);
                _handlers.Add(item);

                _table.AddDelegate(target, handler);
            }

            return item;
        }

        /// <summary>
        /// Unsubscribes the specified handler.
        /// </summary>
        /// <param name="handler">The handler to unsubscribe.</param>
        public void Unsubscribe(IEventAggregatorHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            lock (_lockObject)
            {
                object target;
                Delegate del;
                if (handler.TryGetTargetAndHandler(out target, out del))
                {
                    _table.RemoveDelegate(target, del);
                }

                _handlers.RemoveAll(h => h.IsDead || ReferenceEquals(h, handler));
            }
        }

        /// <summary>
        /// Publishes a message.
        /// </summary>
        /// <param name="message">The message instance.</param>
        public void Publish(object message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            List<IEventAggregatorHandler> selectedHandlers;
            lock (_lockObject)
            {
                _handlers.RemoveAll(h => h.IsDead);
                selectedHandlers = _handlers.FindAll(h => h.CanHandle(message));
            }

            if (selectedHandlers.Count == 0) return;

            var isUIThread = UIContext.CheckAccess();
            var currentThreadHandlers = selectedHandlers.FindAll(h => h.ThreadOption == ThreadOption.PublisherThread || isUIThread && h.ThreadOption == ThreadOption.UIThread);
            PublishCore(message, currentThreadHandlers).ObserveException().Watch();

            if (!isUIThread)
            {
                var uiThreadHandlers = selectedHandlers.FindAll(h => h.ThreadOption == ThreadOption.UIThread);
                if (uiThreadHandlers.Count > 0)
                    UIContext.Run(() => PublishCore(message, uiThreadHandlers)).ObserveException().Watch();
            }

            var backgroundThreadHandlers = selectedHandlers.FindAll(h => h.ThreadOption == ThreadOption.BackgroundThread);
            if (backgroundThreadHandlers.Count > 0)
                Task.Run(() => PublishCore(message, backgroundThreadHandlers)).ObserveException().Watch();
        }

        private static Task PublishCore(object message, IEnumerable<IEventAggregatorHandler> handlers)
        {
            var tasks = new List<Task>();

            foreach (var h in handlers)
            {
                var task = h.HandleAsync(message);
                if (!task.IsCompleted) tasks.Add(task);
            }

            return Task.WhenAll(tasks);
        }
    }
}
