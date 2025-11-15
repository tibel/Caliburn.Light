using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Xaml;

namespace Caliburn.Light.WinUI;

/// <summary>
/// Resolves view and view-model types.
/// </summary>
public sealed class ViewModelTypeResolver : IViewModelTypeResolver
{
    private readonly Dictionary<Type, Type> _modelTypeLookup = new Dictionary<Type, Type>();
    private readonly Dictionary<ViewTypeLookupKey, Type> _viewTypeLookup = new Dictionary<ViewTypeLookupKey, Type>(new ViewTypeLookupKeyComparer());

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewModelTypeResolver"/> class.
    /// </summary>
    /// <param name="mappings">The view-model type mappings.</param>
    public ViewModelTypeResolver(IEnumerable<ViewModelTypeMapping> mappings)
    {
        ArgumentNullException.ThrowIfNull(mappings);

        foreach (var mapping in mappings)
        {
            if (mapping.Context is null)
                _modelTypeLookup.Add(mapping.ViewType, mapping.ModelType);

            _viewTypeLookup.Add(new ViewTypeLookupKey(mapping.ModelType, mapping.Context ?? string.Empty), mapping.ViewType);
        }
    }

    /// <summary>
    /// Determines the view model type based on the specified view type.
    /// </summary>
    /// <param name="viewType">The view type.</param>
    /// <returns>The view model type or null, if not found.</returns>
    public Type? GetModelType(Type viewType)
    {
        ArgumentNullException.ThrowIfNull(viewType);

        _modelTypeLookup.TryGetValue(viewType, out var modelType);
        return modelType;
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

        _viewTypeLookup.TryGetValue(new ViewTypeLookupKey(modelType, context ?? string.Empty), out var viewType);
        return viewType;
    }

    [DebuggerDisplay("ModelType = {ModelType.Name} Context = {Context}")]
    private readonly struct ViewTypeLookupKey
    {
        public ViewTypeLookupKey(Type modelType, string context)
        {
            ModelType = modelType;
            Context = context;
        }

        public Type ModelType { get; }

        public string Context { get; }
    }

    private readonly struct ViewTypeLookupKeyComparer : IEqualityComparer<ViewTypeLookupKey>
    {
        public bool Equals(ViewTypeLookupKey x, ViewTypeLookupKey y)
        {
            return x.ModelType.Equals(y.ModelType) && x.Context.Equals(y.Context);
        }

        public int GetHashCode(ViewTypeLookupKey obj)
        {
            var h1 = obj.ModelType.GetHashCode();
            var h2 = obj.Context.GetHashCode();
            return ((h1 << 5) + h1) ^ h2;
        }
    }
}
