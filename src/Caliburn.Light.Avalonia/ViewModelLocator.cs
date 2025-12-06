using Avalonia.Controls;
using System;
using System.Diagnostics;

namespace Caliburn.Light.Avalonia;

/// <summary>
/// Locates view and view-model instances.
/// </summary>
public sealed class ViewModelLocator : IViewModelLocator
{
    private readonly ViewModelLocatorConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Creates an instance of <see cref="ViewModelLocator"/>.
    /// </summary>
    /// <param name="configuration">The view-model type configuration.</param>
    /// <param name="serviceProvider">The service provider.</param>
    public ViewModelLocator(ViewModelLocatorConfiguration configuration, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Locates the view for the specified model instance.
    /// </summary>
    /// <param name="model">the model instance.</param>
    /// <param name="context">The context (or null).</param>
    /// <returns>The view.</returns>
    public Control LocateForModel(object model, string? context)
    {
        ArgumentNullException.ThrowIfNull(model);

        var view = TryGetViewFromViewAware(model, context);
        if (view is not null)
        {
            Trace.TraceInformation("Using cached view for {0}.", model);
            return view;
        }

        var modelType = model.GetType();
        var viewType = _configuration.FindViewType(modelType, context);
        if (viewType is null)
        {
            Trace.TraceError("Cannot find view for {0}.", modelType);
            return new TextBlock { Text = string.Format("Cannot find view for {0}.", modelType) };
        }

#pragma warning disable IL2072 // ViewModelTypeConfiguration.AddMapping<TView, TViewModel> requires that TView has a public parameterless constructor.
        return _serviceProvider.GetService(viewType) as Control
            ?? (Control)Activator.CreateInstance(viewType)!;
#pragma warning restore IL2072 // ViewModelTypeConfiguration.AddMapping<TView, TViewModel> requires that TView has a public parameterless constructor.
    }

    private static Control? TryGetViewFromViewAware(object model, string? context)
    {
        if (model is IViewAware viewAware)
        {
            if (viewAware.GetView(context) is Control view)
            {
                if (view is Window window && (window.IsLoaded || !window.IsInitialized))
                    return null;

                // remove from parent
                if (view is Control fe && fe.Parent is ContentControl parent)
                    parent.Content = null;

                return view;
            }
        }

        return null;
    }

    /// <summary>
    /// Locates the view model for the specified view instance.
    /// </summary>
    /// <param name="view">The view instance.</param>
    /// <returns>The view model.</returns>
    public object? LocateForView(Control view)
    {
        ArgumentNullException.ThrowIfNull(view);

        if (view is Control Control && Control.DataContext is not null)
        {
            Trace.TraceInformation("Using current data context for {0}.", view);
            return Control.DataContext;
        }

        var viewType = view.GetType();
        var modelType = _configuration.FindViewModelType(viewType);
        if (modelType is null)
        {
            Trace.TraceError("Cannot find model for {0}.", viewType);
            return null;
        }

        var model = _serviceProvider.GetService(modelType);
        if (model is null)
        {
            Trace.TraceError("Cannot locate {0}.", modelType);
            return null;
        }

        return model;
    }
}
