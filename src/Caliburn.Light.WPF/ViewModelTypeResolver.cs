using System;
using System.Linq;

namespace Caliburn.Light.WPF;

/// <summary>
/// Resolves view and view-model types.
/// </summary>
public sealed class ViewModelTypeResolver : IViewModelTypeResolver
{
    private readonly ViewModelTypeConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewModelTypeResolver"/> class.
    /// </summary>
    /// <param name="configuration">The view-model type configuration.</param>
    public ViewModelTypeResolver(ViewModelTypeConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _configuration = configuration;
    }

    /// <summary>
    /// Determines the view model type based on the specified view type.
    /// </summary>
    /// <param name="viewType">The view type.</param>
    /// <returns>The view model type or null, if not found.</returns>
    public Type? GetModelType(Type viewType)
    {
        ArgumentNullException.ThrowIfNull(viewType);
        return _configuration.Mappings.FirstOrDefault(x => x.ViewType == viewType && x.Context is null).ViewModelType;
    }

    /// <summary>
    /// Locates the view type based on the specified model type.
    /// </summary>
    /// <param name="modelType">The model type.</param>
    /// <param name="context">The context instance (or null).</param>
    /// <returns>The view type or null, if not found.</returns>
    public Type? GetViewType(Type modelType, string? context)
    {
        ArgumentNullException.ThrowIfNull(modelType);
        return _configuration.Mappings.FirstOrDefault(x => x.ViewModelType == modelType && string.Equals(x.Context, context, StringComparison.Ordinal)).ViewType;
    }
}
