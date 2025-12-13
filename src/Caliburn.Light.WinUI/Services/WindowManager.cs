using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;
using System;
using System.Collections.Generic;
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
    /// Shows a <see cref="ContentControl"/> for the specified model.
    /// </summary>
    /// <param name="viewModel">The view model.</param>
    /// <param name="ownerViewModel">The owner view model.</param>
    /// <param name="context">The context.</param>
    /// <returns>The content dialog result.</returns>
    public Task<ContentDialogResult> ShowContentDialog(object viewModel, object ownerViewModel, string? context = null)
    {
        ArgumentNullException.ThrowIfNull(ownerViewModel);
        ArgumentNullException.ThrowIfNull(viewModel);

        var owner = GetWindow(ownerViewModel);
        if (owner is null)
            throw new InvalidOperationException("Cannot determine window from ownerViewModel.");

        var contentDialog = CreateContentDialog(viewModel, context);
        contentDialog.XamlRoot = owner.Content.XamlRoot;

        return contentDialog.ShowAsync(ContentDialogPlacement.Popup).AsTask();
    }

    /// <summary>
    /// Displays an open file picker dialog.
    /// </summary>
    /// <param name="options">The dialog options.</param>
    /// <param name="ownerViewModel">The owner view model.</param>
    /// <returns>A list of selected files.</returns>
    public Task<IReadOnlyList<PickFileResult>> ShowFileOpenPickerAsync(FileOpenPickerOptions options, object ownerViewModel)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(ownerViewModel);

        var owner = GetWindow(ownerViewModel);
        if (owner is null)
            throw new InvalidOperationException("Cannot determine window from ownerViewModel.");

        var picker = new FileOpenPicker(owner.AppWindow.Id);
        options.ApplyTo(picker);

        if (options.AllowMultiple)
            return picker.PickMultipleFilesAsync().AsTask();

        static async Task<IReadOnlyList<PickFileResult>> PickSingleFileAsync(FileOpenPicker picker)
        {
            var file = await picker.PickSingleFileAsync();
            if (file is null)
                return Array.Empty<PickFileResult>();
            return new PickFileResult[] { file };
        }

        return PickSingleFileAsync(picker);
    }

    /// <summary>
    /// Displays a save file picker dialog.
    /// </summary>
    /// <param name="options">The dialog options.</param>
    /// <param name="ownerViewModel">The owner view model.</param>
    /// <returns>The selected file</returns>
    public Task<PickFileResult?> ShowFileSavePickerAsync(FileSavePickerOptions options, object ownerViewModel)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(ownerViewModel);

        var owner = GetWindow(ownerViewModel);
        if (owner is null)
            throw new InvalidOperationException("Cannot determine window from ownerViewModel.");

        var picker = new FileSavePicker(owner.AppWindow.Id);
        options.ApplyTo(picker);

        return picker.PickSaveFileAsync().AsTask();
    }

    /// <summary>
    /// Displays an open folder picker dialog.
    /// </summary>
    /// <param name="options">The dialog options.</param>
    /// <param name="ownerViewModel">The owner view model.</param>
    /// <returns>The selected folder.</returns>
    public Task<PickFolderResult?> ShowFolderPickerAsync(FolderPickerOptions options, object ownerViewModel)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(ownerViewModel);

        var owner = GetWindow(ownerViewModel);
        if (owner is null)
            throw new InvalidOperationException("Cannot determine window from ownerViewModel.");

        var picker = new FolderPicker(owner.AppWindow.Id);
        options.ApplyTo(picker);

        return picker.PickSingleFolderAsync().AsTask();
    }

    /// <summary>
    /// Creates a content dialog.
    /// </summary>
    /// <param name="viewModel">The view model.</param>
    /// <param name="context">The view context.</param>
    /// <returns>The content dialog.</returns>
    protected ContentDialog CreateContentDialog(object viewModel, string? context)
    {
        var view = EnsureContentDialog(viewModel, _viewModelLocator.LocateForModel(viewModel, context));
        View.SetViewModelLocator(view, _viewModelLocator);

        view.DataContext = viewModel;

        return new ContentDialogLifecycle(view, context).View;
    }

    /// <summary>
    /// Makes sure the view is a content dialog or is wrapped by one.
    /// </summary>
    /// <param name="viewModel">The view model.</param>
    /// <param name="view">The view.</param>
    /// <returns>The content dialog.</returns>
    protected virtual ContentDialog EnsureContentDialog(object viewModel, UIElement view)
    {
        if (view is not ContentDialog contentDialog)
        {
            contentDialog = new ContentDialog
            {
                Content = view,
                CloseButtonText = "Close",
            };

            View.SetIsGenerated(contentDialog, true);

            if (viewModel is IHaveDisplayName haveDisplayName && viewModel is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.RegisterPropertyChangedWeak(contentDialog, static (t, s, e) =>
                {
                    if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(IHaveDisplayName.DisplayName))
                        t.Title = ((IHaveDisplayName)s!).DisplayName ?? string.Empty;
                });

                contentDialog.Title = haveDisplayName.DisplayName ?? string.Empty;
            }
        }

        return contentDialog;
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

        // https://github.com/microsoft/microsoft-ui-xaml/issues/8322
        View.SetWindow(view, window);

        if (viewModel is IHaveDisplayName haveDisplayName && viewModel is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.RegisterPropertyChangedWeak(window, static (t, s, e) =>
            {
                if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(IHaveDisplayName.DisplayName))
                    t.Title = ((IHaveDisplayName)s!).DisplayName ?? string.Empty;
            });

            window.Title = haveDisplayName.DisplayName ?? string.Empty;
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

        return view is UIElement element
            ? View.GetWindow(element.XamlRoot.Content)
            : null;
    }
}
