using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Caliburn.Light
{
    /// <summary>
    /// Enables loosely-coupled publication of and subscription to events.
    /// </summary>
    public sealed class EventAggregator : IEventAggregator
    {
        private readonly List<IEventAggregatorHandler> _handlers = new List<IEventAggregatorHandler>();
        private readonly IUIContext _uiContext;
        private readonly WaitCallback _publishWaitCallback;
        private readonly SendOrPostCallback _publishSendOrPostCallback;

        /// <summary>
        /// Initializes a new instance of <see cref="EventAggregator"/>.
        /// </summary>
        /// <param name="uiContext">The UI context.</param>
        public EventAggregator(IUIContext uiContext)
        {
            if (uiContext is null)
                throw new ArgumentNullException(nameof(uiContext));

            _uiContext = uiContext;
            _publishWaitCallback = PlublishCore;
            _publishSendOrPostCallback = PlublishCore;  
        }

        /// <summary>
        /// Subscribes the specified handler for messages of type <typeparamref name="TMessage" />.
        /// </summary>
        /// <typeparam name="TTarget">The type of the handler target.</typeparam>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="target">The message handler target.</param>
        /// <param name="handler">The message handler to register.</param>
        /// <param name="threadOption">Specifies on which Thread the <paramref name="handler" /> is executed.</param>
        /// <returns>The <see cref="IEventAggregatorHandler" />.</returns>
        public IEventAggregatorHandler Subscribe<TTarget, TMessage>(TTarget target, Action<TTarget, TMessage> handler, ThreadOption threadOption = ThreadOption.PublisherThread)
            where TTarget : class
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));
            if (handler is null)
                throw new ArgumentNullException(nameof(handler));

            Func<TTarget, TMessage, Task> wrapper = (t, m) =>
            {
                handler(t, m);
                return Task.CompletedTask;
            };

            return SubscribeCore(target, wrapper, threadOption);
        }

        /// <summary>
        /// Subscribes the specified handler for messages of type <typeparamref name="TMessage" />.
        /// </summary>
        /// <typeparam name="TTarget">The type of the handler target.</typeparam>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="target">The message handler target.</param>
        /// <param name="handler">The message handler to register.</param>
        /// <param name="threadOption">Specifies on which Thread the <paramref name="handler" /> is executed.</param>
        /// <returns>The <see cref="IEventAggregatorHandler" />.</returns>
        public IEventAggregatorHandler Subscribe<TTarget, TMessage>(TTarget target, Func<TTarget, TMessage, Task> handler, ThreadOption threadOption = ThreadOption.PublisherThread)
            where TTarget : class
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));
            if (handler is null)
                throw new ArgumentNullException(nameof(handler));

            return SubscribeCore(target, handler, threadOption);
        }

        private IEventAggregatorHandler SubscribeCore<TTarget, TMessage>(TTarget target, Func<TTarget, TMessage, Task> handler, ThreadOption threadOption)
            where TTarget : class
        {
            var item = new EventAggregatorHandler<TTarget, TMessage>(target, handler, threadOption);

            lock (_handlers)
            {
                _handlers.RemoveAll(h => h.IsDead);
                _handlers.Add(item);
            }

            return item;
        }

        /// <summary>
        /// Unsubscribes the specified handler.
        /// </summary>
        /// <param name="handler">The handler to unsubscribe.</param>
        public void Unsubscribe(IEventAggregatorHandler handler)
        {
            if (handler is null)
                throw new ArgumentNullException(nameof(handler));

            lock (_handlers)
            {
                _handlers.RemoveAll(h => h.IsDead || ReferenceEquals(h, handler));
            }
        }

        /// <summary>
        /// Publishes a message.
        /// </summary>
        /// <param name="message">The message instance.</param>
        public void Publish(object message)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            List<IEventAggregatorHandler> selectedHandlers;
            lock (_handlers)
            {
                _handlers.RemoveAll(h => h.IsDead);
                selectedHandlers = _handlers.FindAll(h => h.CanHandle(message));
            }

            if (selectedHandlers.Count == 0) return;

            var isUIThread = _uiContext.CheckAccess();
            var currentThreadHandlers = selectedHandlers.FindAll(h => h.ThreadOption == ThreadOption.PublisherThread || isUIThread && h.ThreadOption == ThreadOption.UIThread);
            if (currentThreadHandlers.Count > 0)
                PublishCore(message, currentThreadHandlers);

            if (!isUIThread)
            {
                var uiThreadHandlers = selectedHandlers.FindAll(h => h.ThreadOption == ThreadOption.UIThread);
                if (uiThreadHandlers.Count > 0)
                    _uiContext.BeginInvoke(_publishSendOrPostCallback, Tuple.Create(message, uiThreadHandlers));
            }

            var backgroundThreadHandlers = selectedHandlers.FindAll(h => h.ThreadOption == ThreadOption.BackgroundThread);
            if (backgroundThreadHandlers.Count > 0)
                ThreadPool.QueueUserWorkItem(_publishWaitCallback, Tuple.Create(message, backgroundThreadHandlers));
        }

        private void PlublishCore(object state)
        {
            var tuple = (Tuple<object, List<IEventAggregatorHandler>>)state;
            PublishCore(tuple.Item1, tuple.Item2);
        }

        private static void PublishCore(object message, List<IEventAggregatorHandler> handlers)
        {
            for (var i = 0; i < handlers.Count; i++)
            {
                var task = handlers[i].HandleAsync(message);
                task.Observe();
                if (!task.IsCompleted)
                    Executing?.Invoke(null, new TaskEventArgs(task));
            }
        }

        /// <summary>
        /// Occurs when <see cref="IEventAggregatorHandler.HandleAsync(object)"/> is invoked and the operation has not completed synchronously.
        /// </summary>
        public static event EventHandler<TaskEventArgs> Executing;
    }
}
