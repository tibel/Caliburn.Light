using System;
using System.ComponentModel;

namespace Caliburn.Light;

/// <summary>
/// Represents a view attached to an <see cref="IViewAware"/> instance.
/// </summary>
public sealed class ViewAwareEntry
{
    private readonly WeakReference _view;

    /// <summary>
    /// Gets the context in which the view appears.
    /// </summary>
    public string? Context { get; }

    /// <summary>
    /// Gets the attached view, or <see langword="null"/> if it has been collected.
    /// </summary>
    public object? View => _view.Target;

    /// <summary>
    /// Creates an attached view reference.
    /// </summary>
    /// <param name="view">The view to reference.</param>
    /// <param name="context">The context in which the view appears.</param>
    public ViewAwareEntry(object view, string? context)
    {
        ArgumentNullException.ThrowIfNull(view);
        _view = new WeakReference(view);
        Context = context;
    }

    /// <summary>
    /// Updates the view referenced by this entry.
    /// </summary>
    /// <param name="view">The replacement view.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetView(object view)
    {
        ArgumentNullException.ThrowIfNull(view);
        _view.Target = view;
    }
}
