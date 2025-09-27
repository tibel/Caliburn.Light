using System;

namespace Caliburn.Light
{
    /// <summary>
    /// Dispatches actions on the current thread.
    /// </summary>
    public sealed class CurrentThreadDispatcher : IDispatcher
    {
        /// <summary>
        /// Gets an instance of the <see cref="CurrentThreadDispatcher"/>.
        /// </summary>
        public static readonly CurrentThreadDispatcher Instance = new CurrentThreadDispatcher();

        private CurrentThreadDispatcher()
        {
        }

        /// <inheritdoc />
        public void BeginInvoke(Action action) => action.Invoke();

        /// <inheritdoc />
        public bool CheckAccess() => true;
    }
}
