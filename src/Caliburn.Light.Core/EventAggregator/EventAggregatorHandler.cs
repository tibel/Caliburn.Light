using System;
using System.Threading.Tasks;

namespace Caliburn.Light
{
    internal sealed class EventAggregatorHandler<TTarget, TMessage> : IEventAggregatorHandler
        where TTarget : class
    {
        private readonly WeakReference<TTarget> _weakTarget; 
        private readonly Func<TTarget, TMessage, Task> _handler;
        private readonly ThreadOption _threadOption;

        public EventAggregatorHandler(TTarget target, Func<TTarget, TMessage, Task> handler, ThreadOption threadOption)
        {
            _weakTarget = new WeakReference<TTarget>(target);
            _handler = handler;
            _threadOption = threadOption;
        }

        public ThreadOption ThreadOption
        {
            get { return _threadOption; }
        }

        public bool IsDead
        {
            get
            {
                TTarget target;
                return !_weakTarget.TryGetTarget(out target);
            }
        }

        public bool CanHandle(object message)
        {
            return message is TMessage;
        }

        public Task HandleAsync(object message)
        {
            TTarget target;
            if (!_weakTarget.TryGetTarget(out target))
                return Task.CompletedTask;

            return _handler(target, (TMessage)message);
        }
    }
}
