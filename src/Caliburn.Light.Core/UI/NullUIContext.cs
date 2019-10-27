using System;
using System.Threading;
using System.Threading.Tasks;

namespace Caliburn.Light
{
    internal sealed class NullUIContext : IUIContext
    {
        public static readonly NullUIContext Instance = new NullUIContext();

        private NullUIContext()
        {
        }

        public bool CheckAccess()
        {
            return true;
        }

        public void VerifyAccess()
        {
        }

        public void BeginInvoke(Action action)
        {
            action();
        }

        public void BeginInvoke(SendOrPostCallback callback, object state)
        {
            callback(state);
        }

        public Task Run(Action action)
        {
            action();
            return Task.CompletedTask;
        }

        public Task<TResult> Run<TResult>(Func<Task<TResult>> function)
        {
            return function();
        }

        public Task Run(Func<Task> function)
        {
            return function();
        }

        public Task<TResult> Run<TResult>(Func<TResult> function)
        {
            return Task.FromResult(function());
        }

        public TaskScheduler TaskScheduler => TaskScheduler.Default;
    }
}
