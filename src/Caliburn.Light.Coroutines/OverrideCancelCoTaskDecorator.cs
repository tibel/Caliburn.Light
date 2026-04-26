namespace Caliburn.Light;

internal sealed class OverrideCancelCoTaskDecorator : CoTaskDecorator
{
    public OverrideCancelCoTaskDecorator(ICoTask coTask)
        : base(coTask)
    {
    }

    protected override void OnInnerCoTaskCompleted(CommandExecutionContext context, ICoTask innerCoTask, CoTaskCompletedEventArgs args)
    {
        OnCompleted(args.Error, false);
    }
}

internal sealed class OverrideCancelCoTaskDecorator<TResult> : CoTaskDecorator, ICoTask<TResult>
{
    private readonly TResult _cancelResult;

    public OverrideCancelCoTaskDecorator(ICoTask<TResult> coTask, TResult cancelResult)
        : base(coTask)
    {
        _cancelResult = cancelResult;
        Result = default!;
    }

    public TResult Result { get; private set; }

    protected override void OnInnerCoTaskCompleted(CommandExecutionContext context, ICoTask innerCoTask, CoTaskCompletedEventArgs args)
    {
        if (args.WasCancelled)
            Result = _cancelResult;
        else if (args.Error is null)
            Result = ((ICoTask<TResult>)innerCoTask).Result;

        OnCompleted(args.Error, false);
    }
}
