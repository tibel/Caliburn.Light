using System;
using System.Threading;
using System.Threading.Tasks;

namespace Caliburn.Light;

/// <summary>
/// Provides asynchronous invocation for event handlers that support deferrals.
/// </summary>
public static class DeferredEventHandlerExtensions
{
    /// <summary>
    /// Invokes the event handler and waits for every deferral requested during invocation.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of event arguments.</typeparam>
    /// <param name="eventHandler">The event handler to invoke, or <see langword="null"/>.</param>
    /// <param name="sender">The source of the event.</param>
    /// <param name="eventArgs">The event data.</param>
    /// <param name="cancellationToken">The token passed to each deferral and used to cancel waiting for completion.</param>
    /// <returns>A task that completes when all deferrals are complete.</returns>
    /// <remarks>
    /// Cancellation does not interrupt synchronous event handlers or automatically complete outstanding deferrals.
    /// Handlers can pass <see cref="EventDeferral.CancellationToken"/> to cancellable work and must still complete
    /// their deferrals. A synchronous exception thrown while invoking the event handler faults the returned task.
    /// Exceptions thrown after an <see langword="await"/> in an <see langword="async"/> <see langword="void"/>
    /// handler are not observed by this method and must be handled by the handler. Any deferrals acquired before an
    /// exception must be completed before the event arguments can be reused.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="eventArgs"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    /// <exception cref="InvalidOperationException">The event arguments are already in use by another invocation.</exception>
    public static async Task InvokeAsync<TEventArgs>(
        this EventHandler<TEventArgs>? eventHandler,
        object? sender,
        TEventArgs eventArgs,
        CancellationToken cancellationToken = default)
        where TEventArgs : DeferredEventArgs
    {
        ArgumentNullException.ThrowIfNull(eventArgs);
        cancellationToken.ThrowIfCancellationRequested();

        eventArgs.BeginInvocation(cancellationToken);

        try
        {
            eventHandler?.Invoke(sender, eventArgs);
        }
        catch
        {
            _ = eventArgs.EndInvocation();
            throw;
        }

        await eventArgs.EndInvocation().WaitAsync(cancellationToken).ConfigureAwait(false);
    }
}
