using System;

namespace Caliburn.Light;

internal sealed class WeakEventAggregatorExecutingHandler<TSubscriber> :
    WeakEventHandlerBase<TSubscriber, TaskEventArgs>
    where TSubscriber : class
{
    public WeakEventAggregatorExecutingHandler(TSubscriber subscriber,
        Action<TSubscriber, object?, TaskEventArgs> weakHandler)
        : base(subscriber, weakHandler)
    {
        EventAggregator.Executing += OnEvent;
    }

    protected override void RemoveEventHandler()
    {
        EventAggregator.Executing -= OnEvent;
    }
}
