using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Caliburn.Light.WPF;

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

        CreateWindow(viewModel, context).Show();
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
        window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
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
        return window?.Activate() ?? false;
    }

    /// <summary>
    /// Shows a message box.
    /// </summary>
    /// <param name="options">The message box options.</param>
    /// <param name="ownerViewModel">The owner view model.</param>
    /// <returns>The message box result.</returns>
    public Task<MessageBoxResult> ShowMessageBox(MessageBoxSettings options, object ownerViewModel)
    {
        ArgumentNullException.ThrowIfNull(ownerViewModel);
        ArgumentNullException.ThrowIfNull(options);

        var owner = GetWindow(ownerViewModel);
        if (owner is null)
            throw new InvalidOperationException("Cannot determine window from ownerViewModel.");

        var result = MessageBox.Show(owner, options.Text, options.Caption, options.Button, options.Image, options.DefaultResult, options.Options);
        return Task.FromResult(result);
    }

    /// <summary>
    /// Shows a file open dialog.
    /// </summary>
    /// <param name="options">The dialog options.</param>
    /// <param name="ownerViewModel">The owner view model.</param>
    /// <returns>A list of selected files.</returns>
    public Task<IReadOnlyList<string>> ShowOpenFileDialog(OpenFileDialogOptions options, object ownerViewModel)
    {
        ArgumentNullException.ThrowIfNull(ownerViewModel);
        ArgumentNullException.ThrowIfNull(options);

        var owner = GetWindow(ownerViewModel);
        if (owner is null)
            throw new InvalidOperationException("Cannot determine window from ownerViewModel.");

        var dialog = new OpenFileDialog();
        options.ApplyTo(dialog);

        bool? result;
        try
        {
            result = dialog.ShowDialog(owner);
        }
        catch when (!string.IsNullOrEmpty(dialog.InitialDirectory))
        {
            dialog.InitialDirectory = null;
            result = dialog.ShowDialog(owner);
        }

        var selectedFiles = result.GetValueOrDefault() ? dialog.FileNames : Array.Empty<string>();
        return Task.FromResult<IReadOnlyList<string>>(selectedFiles);
    }

    /// <summary>
    /// Shows a file save dialog.
    /// </summary>
    /// <param name="options">The dialog options.</param>
    /// <param name="ownerViewModel">The owner view model.</param>
    /// <returns>The selected file.</returns>
    public Task<string> ShowSaveFileDialog(SaveFileDialogOptions options, object ownerViewModel)
    {
        ArgumentNullException.ThrowIfNull(ownerViewModel);
        ArgumentNullException.ThrowIfNull(options);

        var owner = GetWindow(ownerViewModel);
        if (owner is null)
            throw new InvalidOperationException("Cannot determine window from ownerViewModel.");

        var dialog = new SaveFileDialog();
        options.ApplyTo(dialog);

        bool? result;
        try
        {
            result = dialog.ShowDialog(owner);
        }
        catch when (!string.IsNullOrEmpty(dialog.InitialDirectory))
        {
            dialog.InitialDirectory = null;
            result = dialog.ShowDialog(owner);
        }

        var selectedFile = result.GetValueOrDefault() ? dialog.FileName : string.Empty;
        return Task.FromResult(selectedFile);
    }

    /// <summary>
    /// Shows an open folder dialog.
    /// </summary>
    /// <param name="options">The dialog options.</param>
    /// <param name="ownerViewModel">The owner view model.</param>
    /// <returns>A list of selected folders.</returns>
    public Task<IReadOnlyList<string>> ShowOpenFolderDialog(OpenFolderDialogOptions options, object ownerViewModel)
    {
        ArgumentNullException.ThrowIfNull(ownerViewModel);
        ArgumentNullException.ThrowIfNull(options);

        var owner = GetWindow(ownerViewModel);
        if (owner is null)
            throw new InvalidOperationException("Cannot determine window from ownerViewModel.");

        var dialog = new OpenFolderDialog();
        options.ApplyTo(dialog);

        bool? result;
        try
        {
            result = dialog.ShowDialog(owner);
        }
        catch when (!string.IsNullOrEmpty(dialog.InitialDirectory))
        {
            dialog.InitialDirectory = null;
            result = dialog.ShowDialog(owner);
        }

        var selectedFolders = result.GetValueOrDefault() ? dialog.FolderNames : Array.Empty<string>();
        return Task.FromResult<IReadOnlyList<string>>(selectedFolders);
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
        View.SetViewModelLocator(view, _viewModelLocator);

        view.DataContext = viewModel;

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
        if (view is not Window window)
        {
            window = new Window
            {
                Content = view,
                SizeToContent = SizeToContent.WidthAndHeight,
            };

            View.SetIsGenerated(window, true);

            if (viewModel is IHaveDisplayName haveDisplayName && viewModel is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.RegisterPropertyChangedWeak(window, static (t, s, e) =>
                {
                    if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(IHaveDisplayName.DisplayName))
                        t.Title = ((IHaveDisplayName)s!).DisplayName ?? string.Empty;
                });

                window.Title = haveDisplayName.DisplayName ?? string.Empty;
            }
        }

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

        return view is DependencyObject d
            ? Window.GetWindow(d)
            : null;
    }
}
