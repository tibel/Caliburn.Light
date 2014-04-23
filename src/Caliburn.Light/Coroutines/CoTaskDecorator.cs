using System;

namespace Caliburn.Light
{
    /// <summary>
    /// Base class for all <see cref="ICoTask"/> decorators.
    /// </summary>
    internal abstract class CoTaskDecorator : CoTask
    {
        private readonly ICoTask _innerCoTask;
        private CoroutineExecutionContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoTaskDecorator"/> class.
        /// </summary>
        /// <param name="coTask">The CoTask to decorate.</param>
        protected CoTaskDecorator(ICoTask coTask)
        {
            if (coTask == null)
                throw new ArgumentNullException("coTask");

            _innerCoTask = coTask;
        }

        /// <summary>
        /// Executes the CoTask using the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void BeginExecute(CoroutineExecutionContext context)
        {
            _context = context;

            try
            {
                _innerCoTask.Completed += InnerCoTaskCompleted;
                Coroutine.BuildUp(_innerCoTask);
                _innerCoTask.BeginExecute(_context);
            }
            catch (Exception ex)
            {
                InnerCoTaskCompleted(_innerCoTask, new CoTaskCompletedEventArgs(ex, false));
            }
        }

        private void InnerCoTaskCompleted(object sender, CoTaskCompletedEventArgs args)
        {
            _innerCoTask.Completed -= InnerCoTaskCompleted;
            OnInnerResultCompleted(_context, _innerCoTask, args);
            _context = null;
        }

        /// <summary>
        /// Called when the execution of the decorated CoTask has completed.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="innerCoTask">The decorated CoTask.</param>
        /// <param name="args">The <see cref="CoTaskCompletedEventArgs"/> instance containing the event data.</param>
        protected abstract void OnInnerResultCompleted(CoroutineExecutionContext context, ICoTask innerCoTask,
            CoTaskCompletedEventArgs args);
    }
}
