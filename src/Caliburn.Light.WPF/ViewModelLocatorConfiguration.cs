using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace Caliburn.Light.WPF;

/// <summary>
/// Represents a configuration for associating view types with corresponding view model types and optional context
/// identifiers.
/// </summary>
public sealed class ViewModelLocatorConfiguration
{
    private readonly List<(Type ViewType, Type ViewModelType, string? Context)> _mappings = new();

    /// <summary>
    /// Gets the collection of mappings between view types and view model types with optional context identifiers.
    /// </summary>
    public IReadOnlyList<(Type ViewType, Type ViewModelType, string? Context)> Mappings => _mappings;

    /// <summary>
    /// Adds a view view-model mapping.
    /// </summary>
    /// <typeparam name="TView">The view type.</typeparam>
    /// <typeparam name="TViewModel">The view-model type.</typeparam>
    /// <param name="context">The context instance (or null).</param>
    public ViewModelLocatorConfiguration AddMapping<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TView, TViewModel>(string? context = null)
       where TView : UIElement
       where TViewModel : class, INotifyPropertyChanged
    {
        _mappings.Add((typeof(TView), typeof(TViewModel), context));
        return this;
    }
}
