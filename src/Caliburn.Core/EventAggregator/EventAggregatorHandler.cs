using System;
using System.Threading.Tasks;
using Weakly;

namespace Caliburn.Light
{
    internal sealed class EventAggregatorHandler<TTarget, TMessage> : IEventAggregatorHandler
        where TTarget : class
    {
        private readonly WeakReference<TTarget> _target; 
        private readonly Func<TTarget, TMessage, Task> _weakHandler;
        private readonly ThreadOption _threadOption;

        public EventAggregatorHandler(TTarget target, Func<TTarget, TMessage, Task> weakHandler, ThreadOption threadOption)
        {
            _target = new WeakReference<TTarget>(target);
            _weakHandler = weakHandler;
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
                return !_target.TryGetTarget(out target);
            }
        }

        public bool CanHandle(object message)
        {
            return message is TMessage;
        }

        public Task HandleAsync(object message)
        {
            TTarget target;
            if (!_target.TryGetTarget(out target))
                return TaskHelper.Completed();

            return _weakHandler(target, (TMessage)message);
        }
    }
}
