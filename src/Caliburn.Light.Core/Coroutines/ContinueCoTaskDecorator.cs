using System;

namespace Caliburn.Light
{
    /// <summary>
    /// A decorator which executes a coroutine when the wrapped <see cref="ICoTask"/> was canceled.
    /// </summary>
    internal sealed class ContinueCoTaskDecorator : CoTaskDecorator
    {
        private readonly Func<ICoTask> _coroutine;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContinueCoTaskDecorator"/> class.
        /// </summary>
        /// <param name="coTask">The CoTask to decorate.</param>
        /// <param name="coroutine">The coroutine to execute when <paramref name="coTask"/> was canceled.</param>
        public ContinueCoTaskDecorator(ICoTask coTask, Func<ICoTask> coroutine)
            : base(coTask)
        {
            if (coroutine == null)
                throw new ArgumentNullException(nameof(coroutine));

            _coroutine = coroutine;
        }

        /// <summary>
        /// Called when the execution of the decorated CoTask has completed.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="innerCoTask">The decorated CoTask.</param>
        /// <param name="args">The <see cref="CoTaskCompletedEventArgs" /> instance containing the event data.</param>
        protected override void OnInnerResultCompleted(CoroutineExecutionContext context, ICoTask innerCoTask,
            CoTaskCompletedEventArgs args)
        {
            if (args.Error != null || !args.WasCancelled)
            {
                OnCompleted(new CoTaskCompletedEventArgs(args.Error, false));
            }
            else
            {
                Continue(context);
            }
        }

        private void Continue(CoroutineExecutionContext context)
        {
            ICoTask continueCoTask;
            try
            {
                continueCoTask = _coroutine();
            }
            catch (Exception ex)
            {
                OnCompleted(new CoTaskCompletedEventArgs(ex, false));
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

        private void ContinueCompleted(object sender, CoTaskCompletedEventArgs args)
        {
            ((ICoTask) sender).Completed -= ContinueCompleted;
            OnCompleted(new CoTaskCompletedEventArgs(args.Error, args.Error == null));
        }
    }
}
