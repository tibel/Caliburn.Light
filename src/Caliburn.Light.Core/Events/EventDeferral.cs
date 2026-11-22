using System;
using System.Threading;

namespace Caliburn.Light;

/// <summary>
/// Represents work that an event handler must complete before an asynchronous event invocation can finish.
/// </summary>
public sealed class EventDeferral : IDisposable
{
    private DeferredEventArgs? _eventArgs;

    internal EventDeferral(DeferredEventArgs eventArgs, CancellationToken cancellationToken)
    {
        _eventArgs = eventArgs;
        CancellationToken = cancellationToken;
    }

    /// <summary>
    /// Gets the cancellation token associated with the event invocation. Pass this token to cancellable work
    /// started by the handler.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Completes the deferral. Calling this method more than once has no additional effect.
    /// </summary>
    public void Complete()
    {
        Interlocked.Exchange(ref _eventArgs, null)?.CompleteDeferral();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Complete();
    }
}
