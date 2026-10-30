using System;
using System.Runtime.CompilerServices;

namespace Caliburn.Light;

/// <summary>
/// Provides an awaitable that switches execution to a view's UI thread.
/// </summary>
public readonly struct ViewSwitchAwaitable : INotifyCompletion
{
    private readonly IViewAdapter _viewAdapter;
    private readonly object _view;

    internal ViewSwitchAwaitable(IViewAdapter viewAdapter, object view)
    {
        _viewAdapter = viewAdapter;
        _view = view;
    }

    /// <summary>
    /// Gets the awaiter used to await this instance.
    /// </summary>
    public ViewSwitchAwaitable GetAwaiter() => this;

    /// <summary>
    /// Completes the await operation.
    /// </summary>
    public void GetResult()
    {
    }

    /// <summary>
    /// Gets whether execution already has access to the view.
    /// </summary>
    public bool IsCompleted => _viewAdapter.CheckAccess(_view);

    /// <summary>
    /// Schedules the continuation on the view's UI thread.
    /// </summary>
    /// <param name="continuation">The continuation to schedule.</param>
    public void OnCompleted(Action continuation)
    {
        _viewAdapter.BeginInvoke(_view, continuation);
    }
}
