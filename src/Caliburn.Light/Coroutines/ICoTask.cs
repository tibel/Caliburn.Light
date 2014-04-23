using System;

namespace Caliburn.Light
{
    /// <summary>
    /// Allows custom code to execute after the return of an action.
    /// </summary>
    public interface ICoTask
    {
        /// <summary>
        /// Executes the CoTask using the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        void BeginExecute(CoroutineExecutionContext context);

        /// <summary>
        /// Occurs when execution has completed.
        /// </summary>
        event EventHandler<CoTaskCompletedEventArgs> Completed;
    }

    /// <summary>
    /// Allows custom code to execute after the return of an action.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public interface ICoTask<out TResult> : ICoTask
    {
        /// <summary>
        /// Gets the result of the asynchronous operation.
        /// </summary>
        TResult Result { get; }
    }
}
