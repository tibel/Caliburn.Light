using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Caliburn.Light
{
    internal sealed class WeakEventList
    {
        private readonly object _staticTarget = new object();
        private readonly List<WeakEventListener> _list = new List<WeakEventListener>();
        private readonly ConditionalWeakTable<object, object> _cwt = new ConditionalWeakTable<object, object>();

        public void AddHandler(Delegate handler)
        {
            var target = handler.Target ?? _staticTarget;

            // optimize weak event handler case
            if (target is IWeakEventHandler)
                target = _staticTarget;

            // add a record to the main list
            _list.Add(new WeakEventListener(target, handler));

            // add the handler to the CWT - this keeps the handler alive throughout
            // the lifetime of the target, without prolonging the lifetime of
            // the target
            object value;
            if (!_cwt.TryGetValue(target, out value))
            {
                // 99% case - the target only listens once
                _cwt.Add(target, handler);
            }
            else
            {
                // 1% case - the target listens multiple times
                // we store the delegates in a list
                var list = value as List<Delegate>;
                if (list is null)
                {
                    // lazily allocate the list, and add the old handler
                    var oldHandler = value as Delegate;
                    list = new List<Delegate>();
                    list.Add(oldHandler);

                    // install the list as the CWT value
                    _cwt.Remove(target);
                    _cwt.Add(target, list);
                }

                // add the new handler to the list
                list.Add(handler);
            }
        }

        public void RemoveHandler(Delegate handler)
        {
            var target = handler.Target ?? _staticTarget;

            // optimize weak event handler case
            if (target is IWeakEventHandler)
                target = _staticTarget;

            // remove the record from the main list
            for (var i = _list.Count - 1; i >= 0; --i)
            {
                if (_list[i].Matches(target, handler))
                {
                    _list.RemoveAt(i);
                    break;
                }
            }

            // remove the handler from the CWT
            object value;
            if (_cwt.TryGetValue(target, out value))
            {
                var list = value as List<Delegate>;
                if (list is null)
                {
                    // 99% case - the target is removing its single handler
                    _cwt.Remove(target);
                }
                else
                {
                    // 1% case - the target had multiple handlers, and is removing one
                    list.Remove(handler);
                    if (list.Count == 0)
                    {
                        _cwt.Remove(target);
                    }
                }
            }
        }

        public bool Purge()
        {
            return _list.RemoveAll(l => l.Target is null) > 0;
        }

        public List<WeakEventListener> GetCopy()
        {
            return new List<WeakEventListener>(_list);
        }
    }
}
