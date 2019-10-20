using System;

namespace Caliburn.Light
{
    internal struct WeakEventListener
    {
        private readonly WeakReference _target;
        private readonly WeakReference _handler;

        public WeakEventListener(object target, Delegate handler)
        {
            _target = new WeakReference(target);
            _handler = new WeakReference(handler);
        }

        public bool Matches(object target, Delegate handler)
        {
            return ReferenceEquals(target, Target) && Equals(handler, Handler);
        }

        public object Target
        {
            get { return _target.Target; }
        }

        public Delegate Handler
        {
            get { return (Delegate)_handler.Target; }
        }
    }
}
