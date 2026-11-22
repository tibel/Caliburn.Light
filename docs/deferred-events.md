# Deferred Events

Deferred events let an event handler start asynchronous work and delay completion of the event until that work
finishes. The event publisher invokes the event with `InvokeAsync` and awaits the returned task. Event arguments
must derive from `DeferredEventArgs`.

## Example

```csharp
public sealed class RefreshingEventArgs : DeferredEventArgs
{
}

public sealed class DataService
{
    public event EventHandler<RefreshingEventArgs>? Refreshing;

    public Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        var eventArgs = new RefreshingEventArgs();
        return Refreshing.InvokeAsync(this, eventArgs, cancellationToken);
    }
}
```

A handler obtains a deferral before starting asynchronous work. Dispose the deferral when the work is finished;
the invocation completes after every deferral has been completed or disposed.

```csharp
private async void OnRefreshing(object? sender, RefreshingEventArgs eventArgs)
{
    using var deferral = eventArgs.GetDeferral();

    try
    {
        await _cache.RefreshAsync(deferral.CancellationToken);
    }
    catch (OperationCanceledException) when (deferral.CancellationToken.IsCancellationRequested)
    {
        // The event invocation was canceled.
    }
    catch (Exception exception)
    {
        _logger.LogError(exception, "Refreshing the cache failed.");
    }
}
```

Cancellation is cooperative: pass the deferral's `CancellationToken` to cancellable work. Canceling the publisher's
wait does not automatically complete outstanding deferrals. Because `EventHandler<TEventArgs>` handlers return
`void`, exceptions from an `async void` handler are not propagated to `InvokeAsync`; handle and report them in the
handler. Synchronous exceptions thrown directly by a non-async handler fault the returned task.

An event-args instance can participate in only one invocation at a time. Reuse it only after the invocation and all
its deferrals have completed.
