using System;
using Windows.Foundation;
using Windows.UI.Core;

namespace Caliburn.Light.WinUI
{
    internal sealed class ViewDispatcher : IDispatcher
    {
        private readonly CoreDispatcher _dispatcher;

        public ViewDispatcher(CoreDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public bool CheckAccess() => _dispatcher.HasThreadAccess;

        public void BeginInvoke(Action action) => Observe(_dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()));

        private static async void Observe(IAsyncAction asyncAction) => await asyncAction;
    }
}
