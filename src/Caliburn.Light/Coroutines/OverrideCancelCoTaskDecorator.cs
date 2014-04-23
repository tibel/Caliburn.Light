
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
        /// <param name="coTask">The coTask to decorate.</param>
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
}
