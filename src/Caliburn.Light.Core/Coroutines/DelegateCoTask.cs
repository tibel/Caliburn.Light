using System;

namespace Caliburn.Light
{
    /// <summary>
    /// A CoTask that executes an <see cref="System.Action"/>.
    /// </summary>
    internal sealed class DelegateCoTask : CoTask
    {
        private readonly Action _toExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCoTask"/> class.
        /// </summary>
        /// <param name="action">The action.</param>
        public DelegateCoTask(Action action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            _toExecute = action;
        }

        /// <summary>
        /// Executes the CoTask using the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void BeginExecute(CoroutineExecutionContext context)
        {
            Exception error = null;

            try
            {
                _toExecute();
            }
            catch (Exception ex)
            {
                error = ex;
            }

            OnCompleted(new CoTaskCompletedEventArgs(error, false));
        }
    }

    /// <summary>
    /// A CoTask that executes a <see cref="System.Func&lt;TResult&gt;"/>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    internal sealed class DelegateCoTask<TResult> : CoTask, ICoTask<TResult>
    {
        private readonly Func<TResult> _toExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCoTask&lt;TResult&gt;"/> class.
        /// </summary>
        /// <param name="action">The action.</param>
        public DelegateCoTask(Func<TResult> action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            _toExecute = action;
        }

        /// <summary>
        /// Executes the coTask using the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void BeginExecute(CoroutineExecutionContext context)
        {
            Exception error = null;

            try
            {
                Result = _toExecute();
            }
            catch (Exception ex)
            {
                error = ex;
            }

            OnCompleted(new CoTaskCompletedEventArgs(error, false));
        }

        /// <summary>
        /// Gets the coTask.
        /// </summary>
        public TResult Result { get; private set; }
    }
}
