using System;
using System.Windows.Threading;

namespace Caliburn.Light.WPF
{
    internal sealed class ViewDispatcher : IDispatcher
    {
        private readonly Dispatcher _dispatcher;

        public ViewDispatcher(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public bool CheckAccess() => _dispatcher.CheckAccess();

        public void BeginInvoke(Action action) => _dispatcher.InvokeAsync(action);
    }
}
