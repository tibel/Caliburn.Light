using System;
using System.Threading.Tasks;

namespace Caliburn.Light;

internal sealed class TaskFactoryCoTask : ICoTask
{
    private readonly Func<CommandExecutionContext, Task> _operationFactory;

    public TaskFactoryCoTask(Func<CommandExecutionContext, Task> operationFactory)
    {
        ArgumentNullException.ThrowIfNull(operationFactory);
        _operationFactory = operationFactory;
    }

    public async void BeginExecute(CommandExecutionContext context)
    {
        Exception? error = null;
        var wasCancelled = false;

        try
        {
            await _operationFactory(context).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            wasCancelled = true;
        }
        catch (Exception ex)
        {
            error = ex;
        }

        Completed?.Invoke(this, new CoTaskCompletedEventArgs(error, wasCancelled));
    }

    public event EventHandler<CoTaskCompletedEventArgs>? Completed;
}

internal sealed class TaskFactoryCoTask<TResult> : ICoTask<TResult>
{
    private readonly Func<CommandExecutionContext, Task<TResult>> _operationFactory;

    public TaskFactoryCoTask(Func<CommandExecutionContext, Task<TResult>> operationFactory)
    {
        ArgumentNullException.ThrowIfNull(operationFactory);

        _operationFactory = operationFactory;
        Result = default!;
    }

    public async void BeginExecute(CommandExecutionContext context)
    {
        Exception? error = null;
        var wasCancelled = false;

        try
        {
            Result = await _operationFactory(context).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            wasCancelled = true;
        }
        catch (Exception ex)
        {
            error = ex;
        }

        Completed?.Invoke(this, new CoTaskCompletedEventArgs(error, wasCancelled));
    }

    public event EventHandler<CoTaskCompletedEventArgs>? Completed;

    public TResult Result { get; private set; }
}
