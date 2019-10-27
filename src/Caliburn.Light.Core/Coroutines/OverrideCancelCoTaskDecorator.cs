
namespace Caliburn.Light
{
    /// <summary>
    /// A decorator that overrides <see cref="CoTaskCompletedEventArgs.WasCancelled" />.
    /// </summary>
    internal sealed class OverrideCancelCoTaskDecorator : CoTaskDecorator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OverrideCancelCoTaskDecorator"/> class.
        /// </summary>
        /// <param name="coTask">The CoTask to decorate.</param>
        public OverrideCancelCoTaskDecorator(ICoTask coTask)
            : base(coTask)
        {
        }

        /// <summary>
        /// Called when the execution of the decorated CoTask has completed.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="innerCoTask">The decorated CoTask.</param>
        /// <param name="args">The <see cref="Caliburn.Light.CoTaskCompletedEventArgs" /> instance containing the event data.</param>
        protected override void OnInnerResultCompleted(CoroutineExecutionContext context, ICoTask innerCoTask,
            CoTaskCompletedEventArgs args)
        {
            OnCompleted(new CoTaskCompletedEventArgs(args.Error, false));
        }
    }

    /// <summary>
    /// A decorator that overrides <see cref="CoTaskCompletedEventArgs.WasCancelled" />.
    /// </summary>
    internal sealed class OverrideCancelCoTaskDecorator<TResult> : CoTaskDecorator, ICoTask<TResult>
    {
        private readonly TResult _cancelResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="OverrideCancelCoTaskDecorator&lt;TResult&gt;"/> class.
        /// </summary>
        /// <param name="coTask">The CoTask to decorate.</param>
        /// <param name="cancelResult">The canceled result value.</param>
        public OverrideCancelCoTaskDecorator(ICoTask<TResult> coTask, TResult cancelResult = default(TResult))
            : base(coTask)
        {
            _cancelResult = cancelResult;
        }

        /// <summary>
        /// Gets the result of the asynchronous operation.
        /// </summary>
        public TResult Result { get; private set; }

        /// <summary>
        /// Called when the execution of the decorated CoTask has completed.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="innerCoTask">The decorated CoTask.</param>
        /// <param name="args">The <see cref="Caliburn.Light.CoTaskCompletedEventArgs" /> instance containing the event data.</param>
        protected override void OnInnerResultCompleted(CoroutineExecutionContext context, ICoTask innerCoTask,
            CoTaskCompletedEventArgs args)
        {
            if (args.Error is null)
                Result = args.WasCancelled ? _cancelResult : ((ICoTask<TResult>)innerCoTask).Result;

            OnCompleted(new CoTaskCompletedEventArgs(args.Error, false));
        }
    } 
}
