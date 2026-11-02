using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Caliburn.Light;

/// <summary>
/// Enables loosely-coupled publication of and subscription to events.
/// </summary>
public sealed class EventAggregator : IEventAggregator
{
    private readonly Lock _lockObject = new();

    private List<IEventAggregatorHandlerEntry>? _handlers;

    /// <summary>
    /// Subscribes the specified handler for messages of type <typeparamref name="TMessage" />.
    /// </summary>
    /// <typeparam name="TTarget">The type of the handler target.</typeparam>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    /// <param name="target">The message handler target.</param>
    /// <param name="handler">The message handler to register.</param>
    /// <param name="execution">Specifies where the <paramref name="handler"/> is executed.</param>
    /// <returns>The <see cref="IEventAggregatorHandler" />.</returns>
    public IEventAggregatorHandler Subscribe<TTarget, TMessage>(TTarget target, Action<TTarget, TMessage> handler, EventAggregatorExecution execution = EventAggregatorExecution.PublisherThread)
        where TTarget : class
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(handler);
        ThrowIfInvalidEventAggregatorExecution(execution);

        Task wrapper(TTarget t, TMessage m)
        {
            handler(t, m);
            return Task.CompletedTask;
        }

        var item = new EventAggregatorHandlerEntry<TTarget, TMessage>(target, wrapper, execution);
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
    /// <param name="execution">Specifies where the <paramref name="handler"/> is executed.</param>
    /// <returns>The <see cref="IEventAggregatorHandler" />.</returns>
    public IEventAggregatorHandler Subscribe<TTarget, TMessage>(TTarget target, Func<TTarget, TMessage, Task> handler, EventAggregatorExecution execution = EventAggregatorExecution.PublisherThread)
        where TTarget : class
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(handler);
        ThrowIfInvalidEventAggregatorExecution(execution);

        var item = new EventAggregatorHandlerEntry<TTarget, TMessage>(target, handler, execution);
        SubscribeCore(item);
        return item;
    }

    private static void ThrowIfInvalidEventAggregatorExecution(EventAggregatorExecution value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value < EventAggregatorExecution.PublisherThread || value > EventAggregatorExecution.UIThread)
            ThrowOutOfRange(value, paramName);
    }

    [DoesNotReturn]
    private static void ThrowOutOfRange(EventAggregatorExecution value, string? paramName) => throw new ArgumentOutOfRangeException(paramName, value, null);

    private void SubscribeCore(IEventAggregatorHandlerEntry handler)
    {
        lock (_lockObject)
        {
            var handlers = _handlers;
            var copy = new List<IEventAggregatorHandlerEntry>((handlers?.Count ?? 0) + 1);

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

            var copy = new List<IEventAggregatorHandlerEntry>(handlers.Count - 1);

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

        List<IEventAggregatorHandlerEntry>? handlers;

        lock (_lockObject)
        {
            handlers = _handlers;
        }

        if (handlers is null)
            return;

        var processedHandlers = ArrayPool<bool>.Shared.Rent(handlers.Count);
        Array.Clear(processedHandlers, 0, handlers.Count);

        PublishCore(message, handlers, processedHandlers);
    }

    private static void PublishCore(object message, List<IEventAggregatorHandlerEntry> handlers, bool[] processedHandlers)
    {
        object? queuedView = null;
        var queueBackground = false;
        var isThreadPoolThread = Thread.CurrentThread.IsThreadPoolThread;

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

            if (!handler.TryGetTarget(out var target))
            {
                processedHandlers[i] = true;
                continue;
            }

            switch (handler.Execution)
            {
                case EventAggregatorExecution.PublisherThread:
                    processedHandlers[i] = true;
                    InvokeHandler(handler, message, target);
                    break;
                case EventAggregatorExecution.UIThread:
                    var view = FindView(target);
                    if (view is null || ViewHelper.CheckAccess(view))
                    {
                        processedHandlers[i] = true;
                        InvokeHandler(handler, message, target);
                    }
                    else if (queuedView is null && !queueBackground)
                    {
                        queuedView = view;
                    }
                    break;
                case EventAggregatorExecution.ThreadPool:
                    if (isThreadPoolThread)
                    {
                        processedHandlers[i] = true;
                        InvokeHandler(handler, message, target);
                    }
                    else if (queuedView is null && !queueBackground)
                    {
                        queueBackground = true;
                    }
                    break;
            }
        }

        if (queuedView is not null)
        {
            ViewHelper.BeginInvoke(queuedView, () => PublishCore(message, handlers, processedHandlers));
        }
        else if (queueBackground)
        {
            ThreadPool.QueueUserWorkItem(
                static (state) => PublishCore(state.Message, state.Handlers, state.ProcessedHandlers),
                (Message: message, Handlers: handlers, ProcessedHandlers: processedHandlers),
                preferLocal: false);
        }
        else
        {
            ArrayPool<bool>.Shared.Return(processedHandlers, clearArray: false);
        }
    }

    private static void InvokeHandler(IEventAggregatorHandlerEntry handler, object message, object target)
    {
        var task = handler.HandleAsync(message, target);

        task.Observe();

    }

    private static object? FindView(object target)
    {
        if (ViewHelper.IsView(target))
            return target;

        var current = target;
        while (current is not null)
        {
            if (current is IViewAware viewAware)
            {
                var view = viewAware.GetFirstAttachedView();
                if (view is not null)
                    return view;
            }

            current = (current as IParentAware)?.Parent;
        }

        Trace.TraceInformation("UIThread execution for EventAggregator target {0} could not find a view; the handler will run on the publishing thread.", target);
        return null;
    }

}
