using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Caliburn.Light;

/// <summary>
/// Enables loosely-coupled publication of and subscription to events.
/// </summary>
public sealed class EventAggregator : IEventAggregator
{
    private readonly object _lockObject = new object();

    private List<KeyValuePair<IDispatcher, List<IEventAggregatorHandler>>>? _contexts;

    /// <summary>
    /// Subscribes the specified handler for messages of type <typeparamref name="TMessage" />.
    /// </summary>
    /// <typeparam name="TTarget">The type of the handler target.</typeparam>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    /// <param name="target">The message handler target.</param>
    /// <param name="handler">The message handler to register.</param>
    /// <param name="dispatcher">Specifies in which context the <paramref name="handler"/> is executed</param>
    /// <returns>The <see cref="IEventAggregatorHandler" />.</returns>
    public IEventAggregatorHandler Subscribe<TTarget, TMessage>(TTarget target, Action<TTarget, TMessage> handler, IDispatcher? dispatcher = default)
        where TTarget : class
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(handler);

        Task wrapper(TTarget t, TMessage m)
        {
            handler(t, m);
            return Task.CompletedTask;
        }

        var item = new EventAggregatorHandler<TTarget, TMessage>(target, wrapper, dispatcher ?? CurrentThreadDispatcher.Instance);
        SubscribeCore(item);
        return item;
    }

    /// <summary>
    /// Subscribes the specified handler for messages of type <typeparamref name="TMessage" />.
    /// </summary>
    /// <typeparam name="TTarget">The type of the handler target.</typeparam>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    /// <param name="target">The message handler target.</param>
    /// <param name="handler">The message handler to register.</param>
    /// <param name="dispatcher">Specifies in which context the <paramref name="handler"/> is executed.</param>
    /// <returns>The <see cref="IEventAggregatorHandler" />.</returns>
    public IEventAggregatorHandler Subscribe<TTarget, TMessage>(TTarget target, Func<TTarget, TMessage, Task> handler, IDispatcher? dispatcher = default)
        where TTarget : class
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(handler);

        var item = new EventAggregatorHandler<TTarget, TMessage>(target, handler, dispatcher ?? CurrentThreadDispatcher.Instance);
        SubscribeCore(item);
        return item;
    }

    private void SubscribeCore(IEventAggregatorHandler handler)
    {
        lock (_lockObject)
        {
            if (_contexts is null)
            {
                _contexts = [new KeyValuePair<IDispatcher, List<IEventAggregatorHandler>>(handler.Dispatcher, [handler])];
                return;
            }

            _contexts = Clone(_contexts);
            var targetContext = _contexts.Find(x => x.Key.Equals(handler.Dispatcher));
            if (targetContext.Key is null)
                _contexts.Add(new KeyValuePair<IDispatcher, List<IEventAggregatorHandler>>(handler.Dispatcher, [handler]));
            else
                targetContext.Value.Add(handler);
        }
    }

    /// <summary>
    /// Unsubscribes the specified handler.
    /// </summary>
    /// <param name="handler">The handler to unsubscribe.</param>
    public void Unsubscribe(IEventAggregatorHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (_lockObject)
        {
            if (_contexts is null) return;

            _contexts = Clone(_contexts);
            var targetContext = _contexts.Find(x => x.Key.Equals(handler.Dispatcher));
            if (targetContext.Key is not null)
            {
                targetContext.Value.Remove(handler);

                if (targetContext.Value.Count == 0)
                    _contexts.Remove(targetContext);
            }

            if (_contexts.Count == 0)
                _contexts = null;
        }
    }

    /// <summary>
    /// Publishes a message.
    /// </summary>
    /// <param name="message">The message instance.</param>
    public void Publish(object message)
    {
        ArgumentNullException.ThrowIfNull(message);

        IEnumerable<KeyValuePair<IDispatcher, List<IEventAggregatorHandler>>> contexts;

        lock (_lockObject)
        {
            contexts = _contexts ?? Enumerable.Empty<KeyValuePair<IDispatcher, List<IEventAggregatorHandler>>>();
        }

        // publish to current context
        foreach (var context in contexts)
        {
            if (context.Key.CheckAccess())
                PublishCore(message, context.Value);
        }

        // publish to other contexts
        foreach (var context in contexts)
        {
            if (!context.Key.CheckAccess())
                context.Key.BeginInvoke(() => PublishCore(message, context.Value));
        }
    }

    private static List<KeyValuePair<IDispatcher, List<IEventAggregatorHandler>>> Clone(List<KeyValuePair<IDispatcher, List<IEventAggregatorHandler>>> contexts)
    {
        var copy = new List<KeyValuePair<IDispatcher, List<IEventAggregatorHandler>>>(contexts.Count);

        for (var i = 0; i < contexts.Count; i++)
        {
            var context = contexts[i];

            var handlers = context.Value.FindAll(static h => !h.IsDead);
            if (handlers.Count > 0)
                copy.Add(new KeyValuePair<IDispatcher, List<IEventAggregatorHandler>>(context.Key, handlers));
        }

        return copy;
    }

    private static void PublishCore(object message, List<IEventAggregatorHandler> handlers)
    {
        for (var i = 0; i < handlers.Count; i++)
        {
            var handler = handlers[i];

            if (!handler.CanHandle(message))
                continue;

            var task = handler.HandleAsync(message);

            task.Observe();

            if (!task.IsCompleted)
                Executing?.Invoke(null, new TaskEventArgs(task));
        }
    }

    /// <summary>
    /// Occurs when <see cref="IEventAggregatorHandler.HandleAsync(object)"/> is invoked and the operation has not completed synchronously.
    /// </summary>
    public static event EventHandler<TaskEventArgs>? Executing;
}
