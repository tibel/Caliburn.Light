using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace Caliburn.Light.WPF;

/// <summary>
/// Integrate framework life-cycle handling with <see cref="Window"/> events.
/// </summary>
public sealed class WindowLifecycle
{
    private bool _deactivatingFromView;
    private bool _deactivateFromViewModel;
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

        var viewModel = view.DataContext;

        if (viewModel is IViewAware viewAware)
            viewAware.AttachView(view, context);

        if (viewModel is IActivatable activatable)
        {
            if (activateWithWindow)
            {
                view.Activated += (s, _) => ((IActivatable)((FrameworkElement)s!).DataContext).ActivateAsync().Observe();
                view.Deactivated += (s, _) => ((IActivatable)((FrameworkElement)s!).DataContext).DeactivateAsync(false).Observe();
            }
            else
            {
                activatable.ActivateAsync().Observe();
            }

            activatable.Deactivated += OnModelDeactivated;
        }

        if (viewModel is ICloseGuard guard)
            view.Closing += OnViewClosing;
    }

    /// <summary>
    /// Gets the view.
    /// </summary>
    public Window View { get; }

    /// <summary>
    /// Gets the context in which the view appears.
    /// </summary>
    public string? Context { get; }

    private void OnViewClosing(object? sender, CancelEventArgs e)
    {
        if (e.Cancel)
            return;

        if (_actuallyClosing)
        {
            _actuallyClosing = false;
            return;
        }

        e.Cancel = !EvaluateCanClose((ICloseGuard)View.DataContext);
    }

    private async void OnViewClosed(object? sender, EventArgs e)
    {
        View.Closed -= OnViewClosed;
        View.Closing -= OnViewClosing;

        if (View.DataContext is IActivatable activatable)
        {
            activatable.Deactivated -= OnModelDeactivated;

            if (!_deactivateFromViewModel)
            {
                _deactivatingFromView = true;
                await activatable.DeactivateAsync(true).ConfigureAwait(true);
                _deactivatingFromView = false;
            }
        }

        if (View.DataContext is IViewAware viewAware)
            viewAware.DetachView(View, Context);
    }

    private void OnModelDeactivated(object? sender, DeactivationEventArgs e)
    {
        if (!e.WasClosed)
            return;

        ((IActivatable)sender!).Deactivated -= OnModelDeactivated;

        if (_deactivatingFromView)
            return;

        _deactivateFromViewModel = true;
        _actuallyClosing = true;
        View.Close();
        _actuallyClosing = false;
        _deactivateFromViewModel = false;
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
