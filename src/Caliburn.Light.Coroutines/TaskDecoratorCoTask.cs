using System;
using System.Threading.Tasks;

namespace Caliburn.Light;

internal sealed class TaskDecoratorCoTask : ICoTask
{
    private readonly Task _innerTask;

    public TaskDecoratorCoTask(Task task)
    {
        ArgumentNullException.ThrowIfNull(task);

        _innerTask = task;
    }

    public async void BeginExecute(CommandExecutionContext context)
    {
        Exception? error = null;
        var wasCancelled = false;

        try
        {
            await _innerTask.ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            if (_innerTask.IsCanceled)
                wasCancelled = true;
            else
                error = ex;
        }

        Completed?.Invoke(this, new CoTaskCompletedEventArgs(error, wasCancelled));
    }

    public event EventHandler<CoTaskCompletedEventArgs>? Completed;
}

internal sealed class TaskDecoratorCoTask<TResult> : ICoTask<TResult>
{
    private readonly Task<TResult> _innerTask;

    public TaskDecoratorCoTask(Task<TResult> task)
    {
        ArgumentNullException.ThrowIfNull(task);

        _innerTask = task;
        Result = default!;
    }

    public async void BeginExecute(CommandExecutionContext context)
    {
        Exception? error = null;
        var wasCancelled = false;

        try
        {
            Result = await _innerTask.ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            if (_innerTask.IsCanceled)
                wasCancelled = true;
            else
                error = ex;
        }

        Completed?.Invoke(this, new CoTaskCompletedEventArgs(error, wasCancelled));
    }

    public event EventHandler<CoTaskCompletedEventArgs>? Completed;

    public TResult Result { get; private set; }
}
