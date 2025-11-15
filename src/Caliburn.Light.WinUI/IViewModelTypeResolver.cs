using System;

namespace Caliburn.Light.WinUI;

/// <summary>
/// Resolves view and view-model types.
/// </summary>
public interface IViewModelTypeResolver
{
    /// <summary>
    /// Locates the view type based on the specified model type.
    /// </summary>
    /// <param name="modelType">The model type.</param>
    /// <param name="context">The context instance (or null).</param>
    /// <returns>The view type or null, if not found.</returns>
    Type? GetViewType(Type modelType, string? context);

    /// <summary>
    /// Determines the view model type based on the specified view type.
    /// </summary>
    /// <param name="viewType">The view type.</param>
    /// <returns>The view model type or null, if not found.</returns>
    Type? GetModelType(Type viewType);
}
