using System;

namespace Caliburn.Light;

internal sealed class WeakActivationProcessedHandler<TSubscriber> :
    WeakEventHandlerBase<IConductor, TSubscriber, ActivationProcessedEventArgs>
    where TSubscriber : class
{
    public WeakActivationProcessedHandler(IConductor source, TSubscriber subscriber,
        Action<TSubscriber, object?, ActivationProcessedEventArgs> weakHandler)
        : base(source, subscriber, weakHandler)
    {
        source.ActivationProcessed += OnEvent;
    }

    protected override void RemoveEventHandler(IConductor source)
    {
        source.ActivationProcessed -= OnEvent;
    }
}
