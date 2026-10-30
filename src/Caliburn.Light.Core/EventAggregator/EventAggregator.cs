using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Caliburn.Light;

/// <summary>
/// Enables loosely-coupled publication of and subscription to events.
/// </summary>
public sealed class EventAggregator : IEventAggregator
{
    private readonly Lock _lockObject = new();

    private List<IEventAggregatorHandler>? _handlers;

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
            var handlers = _handlers;
            var copy = new List<IEventAggregatorHandler>((handlers?.Count ?? 0) + 1);

            if (handlers is not null)
            {
                for (var i = 0; i < handlers.Count; i++)
                {
                    var existingHandler = handlers[i];

                    if (!existingHandler.IsDead)
                        copy.Add(existingHandler);
                }
            }

            copy.Add(handler);
            _handlers = copy;
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
            if (_handlers is null) return;

            var handlers = _handlers;
            var handlerIndex = -1;
            for (var i = 0; i < handlers.Count; i++)
            {
                if (ReferenceEquals(handlers[i], handler))
                {
                    handlerIndex = i;
                    break;
                }
            }

            if (handlerIndex < 0) return;

            var copy = new List<IEventAggregatorHandler>(handlers.Count - 1);

            for (var i = 0; i < handlers.Count; i++)
            {
                if (i == handlerIndex)
                    continue;

                var existingHandler = handlers[i];

                if (!existingHandler.IsDead)
                    copy.Add(existingHandler);
            }

            if (copy.Count == 0)
                _handlers = null;
            else
                _handlers = copy;
        }
    }

    /// <summary>
    /// Publishes a message.
    /// </summary>
    /// <param name="message">The message instance.</param>
    public void Publish(object message)
    {
        ArgumentNullException.ThrowIfNull(message);

        List<IEventAggregatorHandler>? handlers;

        lock (_lockObject)
        {
            handlers = _handlers;
        }

        if (handlers is null)
            return;

        PublishCore(message, handlers, new bool[handlers.Count]);
    }

    private static void PublishCore(object message, List<IEventAggregatorHandler> handlers, bool[] processedHandlers)
    {
        IDispatcher? queuedDispatcher = null;

        for (var i = 0; i < handlers.Count; i++)
        {
            if (processedHandlers[i])
                continue;

            var handler = handlers[i];

            if (!handler.CanHandle(message))
            {
                processedHandlers[i] = true;
                continue;
            }

            if (!handler.Dispatcher.CheckAccess())
            {
                queuedDispatcher ??= handler.Dispatcher;
                continue;
            }

            processedHandlers[i] = true;

            var task = handler.HandleAsync(message);

            task.Observe();

            if (!task.IsCompleted)
                Executing?.Invoke(null, new TaskEventArgs(task));
        }

        queuedDispatcher?.BeginInvoke(() => PublishCore(message, handlers, processedHandlers));
    }

    /// <summary>
    /// Occurs when <see cref="IEventAggregatorHandler.HandleAsync(object)"/> is invoked and the operation has not completed synchronously.
    /// </summary>
    public static event EventHandler<TaskEventArgs>? Executing;
}
