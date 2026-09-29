using System;

namespace Caliburn.Light;

internal sealed class WeakActivatedHandler<TSubscriber> :
    WeakEventHandlerBase<IActivatable, TSubscriber, ActivationEventArgs>
    where TSubscriber : class
{
    public WeakActivatedHandler(IActivatable source, TSubscriber subscriber,
        Action<TSubscriber, object?, ActivationEventArgs> weakHandler)
        : base(source, subscriber, weakHandler)
    {
        source.Activated += OnEvent;
    }

    protected override void RemoveEventHandler(IActivatable source)
    {
        source.Activated -= OnEvent;
    }
}

internal sealed class WeakDeactivatingHandler<TSubscriber> :
    WeakEventHandlerBase<IActivatable, TSubscriber, DeactivationEventArgs>
    where TSubscriber : class
{
    public WeakDeactivatingHandler(IActivatable source, TSubscriber subscriber,
        Action<TSubscriber, object?, DeactivationEventArgs> weakHandler)
        : base(source, subscriber, weakHandler)
    {
        source.Deactivating += OnEvent;
    }

    protected override void RemoveEventHandler(IActivatable source)
    {
        source.Deactivating -= OnEvent;
    }
}

internal sealed class WeakDeactivatedHandler<TSubscriber> :
    WeakEventHandlerBase<IActivatable, TSubscriber, DeactivationEventArgs>
    where TSubscriber : class
{
    public WeakDeactivatedHandler(IActivatable source, TSubscriber subscriber,
        Action<TSubscriber, object?, DeactivationEventArgs> weakHandler)
        : base(source, subscriber, weakHandler)
    {
        source.Deactivated += OnEvent;
    }

    protected override void RemoveEventHandler(IActivatable source)
    {
        source.Deactivated -= OnEvent;
    }
}
