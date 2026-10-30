using System;

namespace Caliburn.Light;

/// <summary>
/// Extension methods for <see cref="IViewAware"/>.
/// </summary>
public static class ViewAwareExtensions
{
    /// <summary>
    /// Gets the first attached view that is still alive.
    /// </summary>
    /// <param name="viewAware">The view-aware instance.</param>
    /// <returns>The first attached view that is still alive, or <see langword="null"/> if none exists.</returns>
    public static object? GetFirstAttachedView(this IViewAware viewAware)
    {
        ArgumentNullException.ThrowIfNull(viewAware);

        var views = viewAware.GetViews();
        if (views is null)
            return null;

        for (var i = 0; i < views.Count; i++)
        {
            var view = views[i].Key.Target;
            if (view is not null)
                return view;
        }

        return null;
    }
}
