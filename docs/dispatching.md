# UI Thread Dispatching

Caliburn.Light does not automatically switch to the UI thread. Use the attached view to check access,
schedule fire-and-forget work, or switch an async continuation to its UI thread.

## View-based dispatching

`ViewHelper` delegates thread access to the platform adapter for the specified view:

```csharp
if (ViewHelper.CheckAccess(view))
{
    UpdateUI();
}
else
{
    ViewHelper.BeginInvoke(view, UpdateUI);
}
```

`BeginInvoke` schedules the action and returns immediately. Exceptions from the action are raised on
the UI thread.

To switch an async continuation to the view's UI thread:

```csharp
await ViewHelper.SwitchToViewAsync(view);
UpdateUI();
```

The adapter is resolved when `SwitchToViewAsync` is called. If the caller already has access to the
view, the await completes synchronously.

## View-model example

Keep the attached view while it is available, and capture it before starting asynchronous work:

```csharp
public sealed class DataViewModel : ViewAware
{
    private object? _view;

    protected override void OnViewAttached(object view, string? context)
    {
        base.OnViewAttached(view, context);
        _view = view;
    }

    protected override void OnViewDetached(object view, string? context)
    {
        _view = null;
        base.OnViewDetached(view, context);
    }

    private async Task LoadDataAsync()
    {
        var view = _view ?? throw new InvalidOperationException("A view must be attached.");

        Status = "Loading...";
        var data = await LoadDataFromDatabaseAsync().ConfigureAwait(false);

        await ViewHelper.SwitchToViewAsync(view);
        Items.AddRange(data);
        Status = "Loaded";
    }
}
```

`ConfigureAwait(false)` avoids capturing the current synchronization context; it does not guarantee
a thread switch. Use `SwitchToViewAsync` when code must continue on a particular view's UI thread.

## Event aggregator

Select handler execution using `ThreadOption` when subscribing. See
[The Event Aggregator](event-aggregator.md) for its thread routing and batching behavior.

## Platform notes

WPF, Avalonia, and WinUI adapters use the dispatcher associated with the view. Their `CheckAccess`
and `BeginInvoke` operations are exposed through `ViewHelper`, so view models do not need a
platform-specific dispatcher abstraction.

## See also

- [Async (Task Support)](async.md) - Asynchronous programming patterns and lifecycle
- [BindableCollection](bindable-collection.md) - Observable collections
- [Event Aggregator](event-aggregator.md) - Pub/sub messaging and thread options
