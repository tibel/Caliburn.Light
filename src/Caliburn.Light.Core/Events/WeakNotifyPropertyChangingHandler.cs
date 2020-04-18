using System;
using System.ComponentModel;

namespace Caliburn.Light
{
    internal sealed class WeakNotifyPropertyChangingHandler<TSubscriber> :
        WeakEventHandlerBase<INotifyPropertyChanging, TSubscriber, PropertyChangingEventArgs>
        where TSubscriber : class
    {
        public WeakNotifyPropertyChangingHandler(INotifyPropertyChanging source, TSubscriber subscriber,
            Action<TSubscriber, object, PropertyChangingEventArgs> weakHandler)
            : base(source, subscriber, weakHandler)
        {
            source.PropertyChanging += OnEvent;
        }

        protected override void RemoveEventHandler(INotifyPropertyChanging source)
        {
            source.PropertyChanging -= OnEvent;
        }
    }
}
