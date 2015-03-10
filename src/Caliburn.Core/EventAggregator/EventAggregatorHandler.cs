using System;

namespace Caliburn.Light
{
    internal sealed class EventAggregatorHandler<TMessage> : IEventAggregatorHandler
    {
        private readonly Action<TMessage> _weakHandler;
        private readonly ThreadOption _threadOption;

        public EventAggregatorHandler(Action<TMessage> weakHandler, ThreadOption threadOption)
        {
            _weakHandler = weakHandler;
            _threadOption = threadOption;
        }

        public ThreadOption ThreadOption
        {
            get { return _threadOption; }
        }

        public bool IsDead
        {
            get { return false; }
        }

        public bool CanHandle(object message)
        {
            return message is TMessage;
        }

        public void Handle(object message)
        {
            _weakHandler((TMessage) message);
        }
    }

    internal sealed class EventAggregatorHandler<TTarget, TMessage> : IEventAggregatorHandler
        where TTarget : class
    {
        private readonly WeakReference<TTarget> _target; 
        private readonly Action<TTarget, TMessage> _weakHandler;
        private readonly ThreadOption _threadOption;

        public EventAggregatorHandler(TTarget target, Action<TTarget, TMessage> weakHandler, ThreadOption threadOption)
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

        public void Handle(object message)
        {
            TTarget target;
            if (_target.TryGetTarget(out target))
            {
                _weakHandler(target, (TMessage)message);
            }
        }
    }
}
