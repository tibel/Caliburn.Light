using System;
using System.Windows.Threading;

namespace Caliburn.Light.WPF
{
    /// <summary>
    /// The UI execution context.
    /// </summary>
    public sealed class ViewDispatcher : IDispatcher
    {
        private readonly Dispatcher _dispatcher;

        /// <summary>
        /// Initializes a new instance of <see cref="ViewDispatcher"/>.
        /// </summary>
        /// <param name="dispatcher">The dispatcher.</param>
        public ViewDispatcher(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        /// <summary>
        /// Determines whether the calling thread is the thread associated with the UI context. 
        /// </summary>
        public bool CheckAccess() => _dispatcher.CheckAccess();

        /// <summary>
        /// Queues the specified work to run on the UI thread.
        /// </summary>
        /// <param name="action">The work to execute asynchronously.</param>
        public void BeginInvoke(Action action) => _dispatcher.InvokeAsync(action);
    }
}
