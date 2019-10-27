using System.Threading;

namespace Caliburn.Light
{
    /// <summary>
    /// Provides services for managing the queue of work items for a thread.
    /// </summary>
    public interface IDispatcher
    {
        /// <summary>
        /// Determines whether the calling thread is the thread associated with this <see cref="IDispatcher"/>.
        /// </summary>
        /// <returns></returns>
        bool CheckAccess();

        /// <summary>
        /// Executes the specified <paramref name="callback"/> asynchronously on the thread that the <see cref="IDispatcher"/> was created on.
        /// </summary>
        /// <param name="callback">A <see cref="WaitCallback"/> that represents the method to be executed.</param>
        /// <param name="state">An object containing data to be used by the method.</param>
        void BeginInvoke(WaitCallback callback, object state);
    }
}
