using Avalonia.Controls;

namespace Caliburn.Light.Avalonia;

/// <summary>
/// Locates view and view-model instances.
/// </summary>
public interface IViewModelLocator
{
    /// <summary>
    /// Locates the view for the specified model instance.
    /// </summary>
    /// <param name="model">The model instance.</param>
    /// <param name="context">The context (or null).</param>
    /// <returns>The view.</returns>
    Control LocateForModel(object model, string? context);

    /// <summary>
    /// Locates the view model for the specified view instance.
    /// </summary>
    /// <param name="view">The view instance.</param>
    /// <returns>The view model.</returns>
    object? LocateForView(Control view);
}
