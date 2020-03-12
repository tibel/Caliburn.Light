using System;
using Windows.Foundation;
using Windows.UI.Core;

namespace Caliburn.Light.WinUI
{
    /// <summary>
    /// The UI execution context.
    /// </summary>
    public sealed class ViewDispatcher : IDispatcher
    {
        private readonly CoreDispatcher _dispatcher;

        /// <summary>
        /// Initializes a new instance of <see cref="ViewDispatcher"/>.
        /// </summary>
        /// <param name="dispatcher">The dispatcher.</param>
        public ViewDispatcher(CoreDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        /// <summary>
        /// Determines whether the calling thread is the thread associated with the UI context. 
        /// </summary>
        public bool CheckAccess() => _dispatcher.HasThreadAccess;

        /// <summary>
        /// Queues the specified work to run on the UI thread.
        /// </summary>
        /// <param name="action">The work to execute asynchronously.</param>
        public void BeginInvoke(Action action) => Observe(_dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()));

        private static async void Observe(IAsyncAction asyncAction) => await asyncAction;
    }
}
