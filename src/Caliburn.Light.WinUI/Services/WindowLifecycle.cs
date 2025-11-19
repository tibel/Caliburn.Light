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

    private void OnViewActivated(object sender, WindowActivatedEventArgs args)
    {
        var viewModel = GetViewModel(View);
        if (viewModel is not IActivatable activatable)
            return;

        if (args.WindowActivationState == WindowActivationState.CodeActivated || args.WindowActivationState == WindowActivationState.PointerActivated)
            activatable.ActivateAsync().Observe();
        else if (args.WindowActivationState == WindowActivationState.Deactivated)
            activatable.DeactivateAsync(false).Observe();
    }

    private async void OnViewClosed(object sender, WindowEventArgs e)
    {
        if (e.Handled)
            return;

        var viewModel = GetViewModel(View);

        if (!_actuallyClosing && viewModel is ICloseGuard closeGuard && !EvaluateCanClose(closeGuard))
        {
            e.Handled = true;
            return;
        }

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
