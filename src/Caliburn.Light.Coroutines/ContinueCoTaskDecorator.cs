using System;

namespace Caliburn.Light;

internal sealed class ContinueCoTaskDecorator : CoTaskDecorator
{
    private readonly Func<ICoTask> _coroutine;

    public ContinueCoTaskDecorator(ICoTask coTask, Func<ICoTask> coroutine)
        : base(coTask)
    {
        ArgumentNullException.ThrowIfNull(coroutine);

        _coroutine = coroutine;
    }

    protected override void OnInnerCoTaskCompleted(CommandExecutionContext context, ICoTask innerCoTask, CoTaskCompletedEventArgs args)
    {
        if (!args.WasCancelled || args.Error is not null)
        {
            OnCompleted(args.Error, false);
        }
        else
        {
            Continue(context);
        }
    }

    private void Continue(CommandExecutionContext context)
    {
        ICoTask continueCoTask;
        try
        {
            continueCoTask = _coroutine();
        }
        catch (Exception ex)
        {
            OnCompleted(ex, false);
            return;
        }

        if (continueCoTask is null)
        {
            OnCompleted(new InvalidOperationException("The coroutine delegate returned null."), false);
            return;
        }

        try
        {
            continueCoTask.Completed += ContinueCompleted;
            continueCoTask.BeginExecute(context);
        }
        catch (Exception ex)
        {
            ContinueCompleted(continueCoTask, new CoTaskCompletedEventArgs(ex, false));
        }
    }

    private void ContinueCompleted(object? sender, CoTaskCompletedEventArgs args)
    {
        ((ICoTask)sender!).Completed -= ContinueCompleted;
        OnCompleted(args.Error, args.Error is null);
    }
}
