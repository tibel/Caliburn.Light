using System;

namespace Caliburn.Light;

internal sealed class WeakAsyncCommandExecutingHandler<TSubscriber> :
    WeakEventHandlerBase<TSubscriber, TaskEventArgs>
    where TSubscriber : class
{
    public WeakAsyncCommandExecutingHandler(TSubscriber subscriber,
        Action<TSubscriber, object?, TaskEventArgs> weakHandler)
        : base(subscriber, weakHandler)
    {
        AsyncCommand.Executing += OnEvent;
    }

    protected override void RemoveEventHandler()
    {
        AsyncCommand.Executing -= OnEvent;
    }
}
