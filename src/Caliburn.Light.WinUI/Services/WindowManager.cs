using Microsoft.UI.Xaml;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Caliburn.Light.WinUI;

/// <summary>
/// A service that manages windows.
/// </summary>
public class WindowManager : IWindowManager
{
    private readonly IViewModelLocator _viewModelLocator;

    /// <summary>
    /// Creates an instance of <see cref="WindowManager"/>.
    /// </summary>
    /// <param name="viewModelLocator">The view-model locator.</param>
    public WindowManager(IViewModelLocator viewModelLocator)
    {
        ArgumentNullException.ThrowIfNull(viewModelLocator);

        _viewModelLocator = viewModelLocator;
    }

    /// <summary>
    /// Shows a non-modal window for the specified model.
    /// </summary>
    /// <param name="viewModel">The view model.</param>
    /// <param name="context">The context.</param>
    public void ShowWindow(object viewModel, string? context)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        CreateWindow(viewModel, context).Activate();
    }

    /// <summary>
    /// Shows a modal window for the specified model.
    /// </summary>
    /// <param name="viewModel">The view model.</param>
    /// <param name="ownerViewModel">The owner view model.</param>
    /// <param name="context">The context.</param>
    public Task ShowDialog(object viewModel, object ownerViewModel, string? context)
    {
        ArgumentNullException.ThrowIfNull(ownerViewModel);
        ArgumentNullException.ThrowIfNull(viewModel);

        var owner = GetWindow(ownerViewModel);
        if (owner is null)
            throw new InvalidOperationException("Cannot determine window from ownerViewModel.");

        var window = CreateWindow(viewModel, context);
        return window.ShowModal(owner);
    }

    /// <summary>
    /// Attempts to bring the window to the foreground and activates it.
    /// </summary>
    /// <param name="viewModel">The view model of the window.</param>
    /// <returns>true if the window was successfully activated; otherwise, false.</returns>
    public bool Activate(object viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        var window = GetWindow(viewModel);
        if (window is null)
            return false;

        window.Activate();
        return true;
    }

    /// <summary>
    /// Creates a window.
    /// </summary>
    /// <param name="viewModel">The view model.</param>
    /// <param name="context">The view context.</param>
    /// <returns>The window.</returns>
    protected Window CreateWindow(object viewModel, string? context)
    {
        var view = EnsureWindow(viewModel, _viewModelLocator.LocateForModel(viewModel, context));
        View.SetViewModelLocator(view.Content, _viewModelLocator);

        if (view.Content is FrameworkElement element)
            element.DataContext = viewModel;

        if (viewModel is IHaveDisplayName haveDisplayName && viewModel is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.RegisterPropertyChangedWeak(view, static (t, s, e) =>
            {
                if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(IHaveDisplayName.DisplayName))
                    t.Title = ((IHaveDisplayName)s!).DisplayName ?? string.Empty;
            });

            view.Title = haveDisplayName.DisplayName ?? string.Empty;
        }

        return new WindowLifecycle(view, context, false).View;
    }

    /// <summary>
    /// Makes sure the view is a window or is wrapped by one.
    /// </summary>
    /// <param name="viewModel">The view model.</param>
    /// <param name="view">The view.</param>
    /// <returns>The window.</returns>
    protected virtual Window EnsureWindow(object viewModel, UIElement view)
    {
        var window = new Window
        {
            Content = view,
        };

        View.SetWindow(view, window);

        return window;
    }

    /// <summary>
    /// Gets the window from the given view model.
    /// </summary>
    /// <param name="viewModel">The view model.</param>
    /// <returns>The window or null.</returns>
    protected static Window? GetWindow(object? viewModel)
    {
        object? view = null;

        while (viewModel is not null)
        {
            if (viewModel is IViewAware viewAware)
                view = viewAware.GetViews().FirstOrDefault().Value;

            if (view is not null)
                break;

            viewModel = viewModel is IChild child
                ? child.Parent
                : null;
        }

        return view is UIElement element
            ? View.GetWindow(element.XamlRoot.Content)
            : null;
    }
}
