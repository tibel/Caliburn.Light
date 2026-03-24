using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;

namespace Caliburn.Light.WinUI;

/// <summary>
/// Integrate framework life-cycle handling with <see cref="Window"/> events.
/// </summary>
public sealed class WindowLifecycle
{
    private bool _actuallyClosing;

    /// <summary>
    /// Initializes a new instance of <see cref="WindowLifecycle"/>
    /// </summary>
    /// <param name="view">The view.</param>
    /// <param name="context">The context in which the view appears.</param>
    /// <param name="activateWithWindow">Whether the view model shall be activated when the window gets activated and deactivated when the window gets deactivated.</param>
    public WindowLifecycle(Window view, string? context, bool activateWithWindow)
    {
        View = view;
        Context = context;

        view.Closed += OnViewClosed;

        var viewModel = GetViewModel(view);

        if (viewModel is IViewAware viewAware)
            viewAware.AttachView(view.Content, context);

        if (viewModel is IActivatable activatable)
        {
            if (activateWithWindow)
                view.Activated += OnViewActivated;
            else
                activatable.ActivateAsync().Observe();
        }

        if (viewModel is ICloseGuard)
            view.AppWindow.Closing += OnAppWindowClosing;
    }

    /// <summary>
    /// Gets the view.
    /// </summary>
    public Window View { get; }

    /// <summary>
    /// Gets the context in which the view appears.
    /// </summary>
    public string? Context { get; }

    private static object? GetViewModel(Window view) => view.Content is FrameworkElement fe ? fe.DataContext : null;

    private void OnViewActivated(object sender, WindowActivatedEventArgs e)
    {
        var viewModel = GetViewModel(View);
        if (viewModel is not IActivatable activatable)
            return;

        if (e.WindowActivationState == WindowActivationState.CodeActivated || e.WindowActivationState == WindowActivationState.PointerActivated)
            activatable.ActivateAsync().Observe();
        else if (e.WindowActivationState == WindowActivationState.Deactivated)
            activatable.DeactivateAsync(false).Observe();
    }

    private void OnAppWindowClosing(AppWindow sender, AppWindowClosingEventArgs e)
    {
        if (e.Cancel)
            return;

        if (_actuallyClosing)
        {
            _actuallyClosing = false;
            return;
        }

        var viewModel = GetViewModel(View);
        if (viewModel is ICloseGuard closeGuard)
            e.Cancel = !EvaluateCanClose(closeGuard);
    }

    private async void OnViewClosed(object sender, WindowEventArgs e)
    {
        View.Closed -= OnViewClosed;
        View.Activated -= OnViewActivated;
        View.AppWindow.Closing -= OnAppWindowClosing;

        var viewModel = GetViewModel(View);

        if (viewModel is IActivatable activatable)
            await activatable.DeactivateAsync(true).ConfigureAwait(true);

        if (viewModel is IViewAware viewAware)
            viewAware.DetachView(View.Content, Context);
    }

    private bool EvaluateCanClose(ICloseGuard guard)
    {
        var task = guard.CanCloseAsync();
        if (task.IsCompleted)
            return task.Result;

        CloseViewAsync(task);
        return false;
    }

    private async void CloseViewAsync(Task<bool> task)
    {
        var canClose = await task.ConfigureAwait(true);
        if (!canClose)
            return;

        _actuallyClosing = true;
        View.Close();
        _actuallyClosing = false;
    }
}
