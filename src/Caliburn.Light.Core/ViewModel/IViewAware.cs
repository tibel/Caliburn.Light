using System;
using System.Collections.Generic;

namespace Caliburn.Light;

/// <summary>
/// Denotes a class which is aware of its view(s).
/// </summary>
public interface IViewAware
{
    /// <summary>
    /// Attaches a view to this instance.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <param name="context">The context in which the view appears.</param>
    void AttachView(object view, string? context);

    /// <summary>
    /// Detaches a view from this instance.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <param name="context">The context in which the view appears.</param>
    bool DetachView(object view, string? context);

    /// <summary>
    /// Gets a view previously attached to this instance.
    /// </summary>
    /// <param name="context">The context denoting which view to retrieve.</param>
    /// <returns>The view.</returns>
    object? GetView(string? context = null);

    /// <summary>
    /// Gets all views previously attached to this instance and their contexts.
    /// </summary>
    /// <returns>A read-only list of attached views and contexts. The list is empty if no views have been attached.</returns>
    IReadOnlyList<ViewAwareEntry> GetViews();
}
