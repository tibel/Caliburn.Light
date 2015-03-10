using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Weakly;

namespace Caliburn.Light
{
    /// <summary>
    /// Enables loosely-coupled publication of and subscription to events.
    /// </summary>
    public sealed class EventAggregator : IEventAggregator
    {
        private readonly List<IEventAggregatorHandler> _handlers = new List<IEventAggregatorHandler>();

        private static void VerifyTarget(object target)
        {
            if (target == null)
                throw new ArgumentNullException("target");
        }

        private static void VerifyDelegate(Delegate weakHandler)
        {
            if (weakHandler == null)
                throw new ArgumentNullException("weakHandler");
            if (weakHandler.GetMethodInfo().IsClosure())
                throw new ArgumentException("A closure cannot be used to subscribe.", "weakHandler");
        }

        private void AddHandler(IEventAggregatorHandler handler)
        {
            lock (_handlers)
            {
                _handlers.RemoveAll(h => h.IsDead);
                _handlers.Add(handler);
            }
        }

        public IEventAggregatorHandler Subscribe<TMessage>(Action<TMessage> weakHandler, ThreadOption threadOption)
        {
            VerifyDelegate(weakHandler);

            var handler = new EventAggregatorHandler<TMessage>(weakHandler, threadOption);
            AddHandler(handler);
            return handler;
        }

        public IEventAggregatorHandler Subscribe<TTarget, TMessage>(TTarget target, Action<TTarget, TMessage> weakHandler, ThreadOption threadOption)
            where TTarget : class
        {
            VerifyTarget(target);
            VerifyDelegate(weakHandler);

            var handler = new EventAggregatorHandler<TTarget, TMessage>(target, weakHandler, threadOption);
            AddHandler(handler);
            return handler;
        }

        public IEventAggregatorHandler SubscribeAsync<TMessage>(Func<TMessage, Task> weakHandler, ThreadOption threadOption)
        {
            VerifyDelegate(weakHandler);

            var handler = new EventAggregatorHandler<TMessage>(m => weakHandler(m).ObserveException(), threadOption);
            AddHandler(handler);
            return handler;
        }

        public IEventAggregatorHandler SubscribeAsync<TTarget, TMessage>(TTarget target, Func<TTarget, TMessage, Task> weakHandler, ThreadOption threadOption)
            where TTarget : class
        {
            VerifyTarget(target);
            VerifyDelegate(weakHandler);

            var handler = new EventAggregatorHandler<TTarget, TMessage>(target, (t, m) => weakHandler(t, m).ObserveException(), threadOption);
            AddHandler(handler);
            return handler;
        }

        public void Unsubscribe(IEventAggregatorHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            lock (_handlers)
            {
                _handlers.RemoveAll(h => h.IsDead || ReferenceEquals(h, handler));
            }
        }

        public void Publish(object message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            List<IEventAggregatorHandler> selectedHandlers;
            lock (_handlers)
            {
                _handlers.RemoveAll(h => h.IsDead);
                var messageType = message.GetType();
                selectedHandlers = _handlers.FindAll(h => h.CanHandle(messageType));
            }

            if (selectedHandlers.Count == 0) return;
            var isUIThread = UIContext.CheckAccess();

            selectedHandlers
                .Where(h => h.ThreadOption == ThreadOption.PublisherThread ||
                            isUIThread && h.ThreadOption == ThreadOption.UIThread)
                .ForEach(h => h.Handle(message));

            if (!isUIThread)
            {
                var uiThreadHandlers = selectedHandlers.FindAll(h => h.ThreadOption == ThreadOption.UIThread);
                if (uiThreadHandlers.Count > 0)
                    UIContext.Run(() => uiThreadHandlers.ForEach(h => h.Handle(message))).ObserveException();
            }

            var backgroundHandlers = selectedHandlers.FindAll(h => h.ThreadOption == ThreadOption.BackgroundThread);
            if (backgroundHandlers.Count > 0)
                Task.Run(() => backgroundHandlers.ForEach(h => h.Handle(message))).ObserveException();
        }
    }
}
