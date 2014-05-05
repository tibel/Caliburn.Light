using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Weakly;

namespace Caliburn.Light
{
    /// <summary>
    /// Enables loosely-coupled publication of and subscription to events.
    /// </summary>
    public sealed class EventAggregator : IEventAggregator
    {
        private readonly List<Handler> _handlers = new List<Handler>(); 

        /// <summary>
        /// Subscribes the specified handler for messages of type <typeparamref name="TMessage" />.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="handler">The message handler to register.</param>
        /// <param name="threadOption">Specifies on which Thread the <paramref name="handler" /> is executed.</param>
        public void Subscribe<TMessage>(Action<TMessage> handler, ThreadOption threadOption = ThreadOption.PublisherThread)
        {
            SubscribeInternal<TMessage>(handler, threadOption);
        }

        /// <summary>
        /// Subscribes the specified handler for messages of type <typeparamref name="TMessage" />.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="handler">The message handler to register.</param>
        /// <param name="threadOption">Specifies on which Thread the <paramref name="handler" /> is executed.</param>
        public void Subscribe<TMessage, TResult>(Func<TMessage, TResult> handler, ThreadOption threadOption = ThreadOption.PublisherThread)
        {
            SubscribeInternal<TMessage>(handler, threadOption);
        }

        private void SubscribeInternal<TMessage>(Delegate handler, ThreadOption threadOption)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");
            if (handler.Target != null && handler.GetMethodInfo().IsClosure())
                throw new ArgumentException("A closure cannot be used to subscribe.", "handler");

            lock (_handlers)
            {
                _handlers.RemoveAll(h => h.IsDead);
                _handlers.Add(new Handler(typeof(TMessage), handler.Target, handler.GetMethodInfo(), threadOption));
            }
        }

        /// <summary>
        /// Unsubscribes the specified handler.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="handler">The handler to unsubscribe.</param>
        public void Unsubscribe<TMessage>(Action<TMessage> handler)
        {
            UnsubscribeInternal<TMessage>(handler);
        }

        /// <summary>
        /// Unsubscribes the specified handler.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="handler">The handler to unsubscribe.</param>
        public void Unsubscribe<TMessage, TResult>(Func<TMessage, TResult> handler)
        {
            UnsubscribeInternal<TMessage>(handler);
        }

        private void UnsubscribeInternal<TMessage>(Delegate handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            lock (_handlers)
            {
                _handlers.RemoveAll(
                    h => h.IsDead || (h.MessageType == typeof(TMessage) && h.Target == handler.Target && h.Method == handler.GetMethodInfo()));
            }
        }

        /// <summary>
        /// Publishes a message.
        /// </summary>
        /// <param name="message">The message instance.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Publish(object message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            List<Handler> selectedHandlers;
            lock (_handlers)
            {
                _handlers.RemoveAll(h => h.IsDead);
                var messageType = message.GetType();
                selectedHandlers = _handlers.Where(h => h.MessageType.GetTypeInfo().IsAssignableFrom(messageType.GetTypeInfo())).ToList();
            }

            selectedHandlers.ForEach(h => h.Invoke(message));
        }

        private sealed class Handler
        {
            private readonly Type _messageType;
            private readonly WeakReference _reference;
            private readonly MethodInfo _method;
            private readonly ThreadOption _threadOption;

            public Handler(Type messageType, object target, MethodInfo method, ThreadOption threadOption)
            {
                _messageType = messageType;
                _method = method;
                _threadOption = threadOption;

                if (target != null)
                {
                    _reference = new WeakReference(target);
                }
            }

            public Type MessageType
            {
                get { return _messageType; }
            }

            public object Target
            {
                get { return (_reference != null) ? _reference.Target : null; }
            }

            public MethodInfo Method
            {
                get { return _method; }
            }

            public bool IsDead
            {
                get { return _reference != null && _reference.Target == null; }
            }

            public void Invoke(object message)
            {
                object target = null;
                if (_reference != null)
                {
                    target = _reference.Target;
                    if (target == null) return;
                }

                if (_threadOption == ThreadOption.BackgroundThread)
                {
                    Task.Factory.StartNew(() => InvokeInternal(target, message),
                        CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
                }
                else if (_threadOption == ThreadOption.PublisherThread ||
                    _threadOption == ThreadOption.UIThread && UIContext.CheckAccess())
                {
                    InvokeInternal(target, message);
                }
                else if (_threadOption == ThreadOption.UIThread)
                {
                    Task.Factory.StartNew(() => InvokeInternal(target, message),
                        CancellationToken.None, TaskCreationOptions.None, UIContext.TaskScheduler);
                }
            }

            private void InvokeInternal(object target, object message)
            {
                var returnValue = DynamicDelegate.From(_method).Invoke(target, new[] { message });
                if (returnValue == null) return;

                var enumerable = returnValue as IEnumerable<ICoTask>;
                if (enumerable != null)
                {
                    returnValue = enumerable.GetEnumerator();
                }

                var enumerator = returnValue as IEnumerator<ICoTask>;
                if (enumerator != null)
                {
                    returnValue = enumerator.AsCoTask();
                }

                var coTask = returnValue as ICoTask;
                if (coTask != null)
                {
                    var context = new CoroutineExecutionContext
                    {
                        Source = this,
                        Target = target,
                    };

                    coTask.ExecuteAsync(context);
                }
            }
        }
    }
}
