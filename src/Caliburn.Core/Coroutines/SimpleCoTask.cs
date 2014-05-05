using System;

namespace Caliburn.Light
{
    /// <summary>
    /// A simple CoTask.
    /// </summary>
    public sealed class SimpleCoTask : CoTask
    {
        private readonly bool _wasCancelled;
        private readonly Exception _error;

        private SimpleCoTask(bool wasCancelled, Exception error)
        {
            _wasCancelled = wasCancelled;
            _error = error;
        }

        /// <summary>
        /// A CoTask that is always succeeded.
        /// </summary>
        public static ICoTask Succeeded()
        {
            return new SimpleCoTask(false, null);
        }

        /// <summary>
        /// A CoTask that is always canceled.
        /// </summary>
        /// <returns>The coTask.</returns>
        public static ICoTask Cancelled()
        {
            return new SimpleCoTask(true, null);
        }

        /// <summary>
        /// A CoTask that is always failed.
        /// </summary>
        public static ICoTask Failed(Exception error)
        {
            return new SimpleCoTask(false, error);
        }

        /// <summary>
        /// Executes the CoTask using the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void BeginExecute(CoroutineExecutionContext context)
        {
            OnCompleted(new CoTaskCompletedEventArgs(_error, _wasCancelled));
        }
    }
}
