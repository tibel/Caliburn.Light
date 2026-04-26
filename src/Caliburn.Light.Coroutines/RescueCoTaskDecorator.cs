using System;

namespace Caliburn.Light;

internal sealed class RescueCoTaskDecorator<TException> : CoTaskDecorator where TException : Exception
{
    private readonly bool _cancelCoTask;
    private readonly Func<TException, ICoTask> _coroutine;

    public RescueCoTaskDecorator(ICoTask coTask, Func<TException, ICoTask> coroutine, bool cancelCoTask)
        : base(coTask)
    {
        ArgumentNullException.ThrowIfNull(coroutine);

        _coroutine = coroutine;
        _cancelCoTask = cancelCoTask;
    }

    protected override void OnInnerCoTaskCompleted(CommandExecutionContext context, ICoTask innerCoTask, CoTaskCompletedEventArgs args)
    {
        if (args.Error is not TException error)
        {
            OnCompleted(args.Error, args.WasCancelled);
        }
        else
        {
            Rescue(context, error);
        }
    }

    private void Rescue(CommandExecutionContext context, TException exception)
    {
        ICoTask rescueCoTask;
        try
        {
            rescueCoTask = _coroutine(exception);
        }
        catch (Exception ex)
        {
            OnCompleted(ex, false);
            return;
        }

        if (rescueCoTask is null)
        {
            OnCompleted(new InvalidOperationException("The rescue delegate returned null."), false);
            return;
        }

        try
        {
            rescueCoTask.Completed += RescueCompleted;
            rescueCoTask.BeginExecute(context);
        }
        catch (Exception ex)
        {
            RescueCompleted(rescueCoTask, new CoTaskCompletedEventArgs(ex, false));
        }
    }

    private void RescueCompleted(object? sender, CoTaskCompletedEventArgs args)
    {
        ((ICoTask)sender!).Completed -= RescueCompleted;
        OnCompleted(args.Error, args.Error is null && (args.WasCancelled || _cancelCoTask));
    }
}
