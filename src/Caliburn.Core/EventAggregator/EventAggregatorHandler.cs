using System;
using System.Threading.Tasks;
using Weakly;

namespace Caliburn.Light
{
    internal sealed class EventAggregatorHandler<TMessage> : IEventAggregatorHandler
    {
        private readonly WeakReference<object> _weakTarget;
        private readonly WeakReference<Func<TMessage, Task>> _weakHandler;
        private readonly ThreadOption _threadOption;

        public EventAggregatorHandler(object target, Func<TMessage, Task> handler, ThreadOption threadOption)
        {
            _weakTarget = new WeakReference<object>(target);
            _weakHandler = new WeakReference<Func<TMessage, Task>>(handler);
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
                Func<TMessage, Task> handler;
                return !_weakHandler.TryGetTarget(out handler);
            }
        }

        public bool TryGetTargetAndHandler(out object target, out Delegate handler)
        {
            Func<TMessage, Task> typedHandler = null;
            var result = _weakTarget.TryGetTarget(out target) && _weakHandler.TryGetTarget(out typedHandler);
            handler = typedHandler;
            return result;
        }

        public bool CanHandle(object message)
        {
            return message is TMessage;
        }

        public Task HandleAsync(object message)
        {
            Func<TMessage, Task> handler;
            if (!_weakHandler.TryGetTarget(out handler))
                return TaskHelper.Completed();

            return handler((TMessage)message);
        }
    }
}
