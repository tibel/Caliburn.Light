using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Caliburn.Light;

/// <summary>
/// A base implementation of <see cref = "IViewAware" /> which is aware of its view(s).
/// </summary>
public class ViewAware : ParentAware, IViewAware
{
    private List<KeyValuePair<WeakReference, string?>>? _views;

    void IViewAware.AttachView(object view, string? context)
    {
        ArgumentNullException.ThrowIfNull(view);

        Trace.TraceInformation("Attaching view {0} to {1}.", view, this);
        _views ??= new List<KeyValuePair<WeakReference, string?>>(1);
        var index = _views.FindIndex(p => string.Equals(p.Value, context, StringComparison.Ordinal));
        if (index < 0)
            _views.Add(new KeyValuePair<WeakReference, string?>(new WeakReference(view), context));
        else
            _views[index].Key.Target = view;

        OnViewAttached(view, context);
    }

    bool IViewAware.DetachView(object view, string? context)
    {
        ArgumentNullException.ThrowIfNull(view);

        Trace.TraceInformation("Detaching view {0} from {1}.", view, this);
        var detached = _views?.RemoveAll(p => string.Equals(p.Value, context, StringComparison.Ordinal)) > 0;

        if (detached)
            OnViewDetached(view, context);

        return detached;
    }

    /// <summary>
    /// Called when a view is attached.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <param name="context">The context in which the view appears.</param>
    protected virtual void OnViewAttached(object view, string? context)
    {
    }

    /// <summary>
    /// Called when a view is detached.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <param name="context">The context in which the view appears.</param>
    protected virtual void OnViewDetached(object view, string? context)
    {
    }

    /// <summary>
    /// Gets a view previously attached to this instance.
    /// </summary>
    /// <param name="context">The context denoting which view to retrieve.</param>
    /// <returns>The view.</returns>
    protected object? GetAttachedView(string? context = null)
    {
        var entry = _views?.Find(p => string.Equals(p.Value, context, StringComparison.Ordinal)) ?? default;
        return entry.Key?.Target;
    }

    object? IViewAware.GetView(string? context) => GetAttachedView(context);

    IReadOnlyList<KeyValuePair<WeakReference, string?>>? IViewAware.GetViews() => _views;
}
