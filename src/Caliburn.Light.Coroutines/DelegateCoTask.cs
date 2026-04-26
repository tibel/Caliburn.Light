using System;

namespace Caliburn.Light;

internal sealed class DelegateCoTask : ICoTask
{
    private readonly Action _toExecute;

    public DelegateCoTask(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        _toExecute = action;
    }

    public void BeginExecute(CommandExecutionContext context)
    {
        Exception? error = null;

        try
        {
            _toExecute();
        }
        catch (Exception ex)
        {
            error = ex;
        }

        Completed?.Invoke(this, new CoTaskCompletedEventArgs(error, false));
    }

    public event EventHandler<CoTaskCompletedEventArgs>? Completed;
}

internal sealed class DelegateCoTask<TResult> : ICoTask<TResult>
{
    private readonly Func<TResult> _toExecute;

    public DelegateCoTask(Func<TResult> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        _toExecute = action;
        Result = default!;
    }

    public void BeginExecute(CommandExecutionContext context)
    {
        Exception? error = null;

        try
        {
            Result = _toExecute();
        }
        catch (Exception ex)
        {
            error = ex;
        }

        Completed?.Invoke(this, new CoTaskCompletedEventArgs(error, false));
    }

    public event EventHandler<CoTaskCompletedEventArgs>? Completed;

    public TResult Result { get; private set; }
}
