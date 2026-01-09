using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Caliburn.Light;

/// <summary>
/// A weak event source that does not hold any strong reference to the event handlers.
/// </summary>
/// <typeparam name="TEventHandler">The event handler type.</typeparam>
public abstract class WeakEventSourceBase<TEventHandler>
    where TEventHandler : Delegate
{
    private readonly object _lockObject = new object();

    private List<WeakReference<TEventHandler>>? _list;
    private ConditionalWeakTable<object, List<TEventHandler>>? _cwt;

    /// <summary>
    /// Adds the specified event handler.
    /// </summary>
    /// <param name="eventHandler">The event handler.</param>
    public void Add(TEventHandler eventHandler)
    {
        if (eventHandler is null) return;

        lock (_lockObject)
        {
            var target = GetTarget(eventHandler, _lockObject);

            _list = _list is null
                ? new List<WeakReference<TEventHandler>>(1)
                : new List<WeakReference<TEventHandler>>(_list);

            _list.RemoveAll(static wr => !wr.TryGetTarget(out var _));
            _list.Add(new WeakReference<TEventHandler>(eventHandler));

            _cwt ??= new ConditionalWeakTable<object, List<TEventHandler>>();
            _cwt.GetOrCreateValue(target).Add(eventHandler);
        }
    }

    /// <summary>
    /// Removes the specified event handler.
    /// </summary>
    /// <param name="eventHandler">The event handler.</param>
    public void Remove(TEventHandler eventHandler)
    {
        if (eventHandler is null) return;

        lock (_lockObject)
        {
            if (_list is null || _cwt is null) return;

            _list = new List<WeakReference<TEventHandler>>(_list);
            _list.RemoveAll(static wr => !wr.TryGetTarget(out var _));

            var index = _list.FindIndex(wr => wr.TryGetTarget(out var handler) && Equals(handler, eventHandler));
            if (index >= 0) _list.RemoveAt(index);

            var target = GetTarget(eventHandler, _lockObject);

            if (_cwt.TryGetValue(target, out var value))
            {
                value.Remove(eventHandler);
                if (value.Count == 0)
                    _cwt.Remove(target);
            }

            if (_list.Count == 0)
            {
                _list = null;
                _cwt = null;
            }
        }
    }

    /// <summary>
    /// Gets the handlers to raise the event.
    /// </summary>
    /// <returns>The list of active event handlers.</returns>
    protected IEnumerable<TEventHandler> GetHandlers()
    {
        lock (_lockObject)
        {
            return _list is null
                ? Enumerable.Empty<TEventHandler>()
                : GetHandlers(_list);
        }
    }

    private static object GetTarget(TEventHandler handler, object staticTarget)
    {
        var target = handler.Target ?? staticTarget;

        // protect weak event handlers from being collected
        if (target is IWeakEventHandler)
            target = staticTarget;

        return target;
    }

    private static IEnumerable<TEventHandler> GetHandlers(List<WeakReference<TEventHandler>> list)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (list[i].TryGetTarget(out var handler))
                yield return handler;
        }
    }
}
