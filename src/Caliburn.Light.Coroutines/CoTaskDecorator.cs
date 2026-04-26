using System;

namespace Caliburn.Light;

internal abstract class CoTaskDecorator : ICoTask
{
    private readonly ICoTask _innerCoTask;
    private CommandExecutionContext? _context;

    protected CoTaskDecorator(ICoTask coTask)
    {
        ArgumentNullException.ThrowIfNull(coTask);

        _innerCoTask = coTask;
    }

    public void BeginExecute(CommandExecutionContext context)
    {
        _context = context;

        try
        {
            _innerCoTask.Completed += InnerCoTaskCompleted;
            _innerCoTask.BeginExecute(_context);
        }
        catch (Exception ex)
        {
            InnerCoTaskCompleted(_innerCoTask, new CoTaskCompletedEventArgs(ex, false));
        }
    }

    public event EventHandler<CoTaskCompletedEventArgs>? Completed;

    protected void OnCompleted(Exception? error, bool wasCancelled)
    {
        Completed?.Invoke(this, new CoTaskCompletedEventArgs(error, wasCancelled));
    }

    private void InnerCoTaskCompleted(object? sender, CoTaskCompletedEventArgs args)
    {
        _innerCoTask.Completed -= InnerCoTaskCompleted;

        var context = _context!;
        _context = null;

        OnInnerCoTaskCompleted(context, _innerCoTask, args);
    }

    protected abstract void OnInnerCoTaskCompleted(CommandExecutionContext context, ICoTask innerCoTask, CoTaskCompletedEventArgs args);
}
