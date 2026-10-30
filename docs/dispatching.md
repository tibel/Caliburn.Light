# UI Thread Dispatching

In XAML applications, UI elements can only be accessed from the UI thread. Caliburn.Light provides the `IDispatcher` interface to help you marshal operations to the correct thread.

## Overview

When you perform work on a background thread (e.g., loading data from a database or calling a web service), you need to switch back to the UI thread before updating properties that are bound to UI elements. Caliburn.Light provides a simple and elegant solution using async/await patterns.

## IDispatcher Interface

```csharp
public interface IDispatcher
{
    bool CheckAccess();
    void BeginInvoke(Action action);
}
```

### Methods

| Method | Description |
|--------|-------------|
| `CheckAccess()` | Returns `true` if you're on the UI thread |
| `BeginInvoke(action)` | Queues the action to run on the UI thread (fire-and-forget) |

### Switching to a View Thread

Use `ViewHelper.SwitchToViewAsync(view)` to continue on the UI thread associated with a view:

```csharp
await ViewHelper.SwitchToViewAsync(view);
```

The view must be registered with a platform adapter. The awaitable resolves that adapter when it is
created, then schedules the continuation only when the caller is not already on the view thread.

## Getting the Dispatcher

The dispatcher is obtained through `ViewHelper.GetDispatcher(view)`. The recommended pattern is to use `ViewAware` as a base class:

```csharp
public class MyViewModel : ViewAware
{
    private IDispatcher _dispatcher = CurrentThreadDispatcher.Instance;

    protected override void OnViewAttached(object view, string? context)
    {
        base.OnViewAttached(view, context);
        _dispatcher = ViewHelper.GetDispatcher(view);
    }

    protected override void OnViewDetached(object view, string? context)
    {
        _dispatcher = CurrentThreadDispatcher.Instance;
        base.OnViewDetached(view, context);
    }
}
```

## Usage Examples

### Switching Back to the UI Thread

Capture the attached view before starting background work, then use it to switch back:

```csharp
public class DataViewModel : ViewAware
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

        // Start on UI thread
        Status = "Loading...";

        // Move to background thread
        await Task.Delay(10).ConfigureAwait(false);

        // Do work without accessing UI-bound state
        var data = LoadDataFromDatabase();

        // Switch back to UI thread
        await ViewHelper.SwitchToViewAsync(view);

        // Now on UI thread
        Debug.Assert(ViewHelper.CheckAccess(view));
        Items.AddRange(data);
        Status = "Loaded";
    }
}
```

### Using BeginInvoke

For fire-and-forget scenarios:

```csharp
public void HandleBackgroundEvent(object sender, DataEventArgs e)
{
    // Queue the update on UI thread and continue immediately
    ViewHelper.BeginInvoke(_view!, () =>
    {
        ProcessedCount++;
        LastItem = e.Data;
    });
}
```

### Checking Thread Access

```csharp
public void UpdateStatus(string message)
{
    if (ViewHelper.CheckAccess(_view!))
    {
        // Already on UI thread
        Status = message;
    }
    else
    {
        // Need to marshal to UI thread
        ViewHelper.BeginInvoke(_view!, () => Status = message);
    }
}
```

## ConfigureAwait Behavior

Understanding `ConfigureAwait` is important when working with dispatchers:

`ConfigureAwait(false)` avoids capturing the current synchronization context; it does not itself
guarantee a thread switch. Use `ViewHelper.SwitchToViewAsync(view)` when code must continue on a
specific view's UI thread.

## Complete Example

Here's a complete example from the gallery samples:

```csharp
public sealed class ThreadingViewModel : ViewAware, IHaveDisplayName
{
    private object? _view;
    private IDispatcher _dispatcher = CurrentThreadDispatcher.Instance;

    public string? DisplayName => "Threading";

    public ThreadingViewModel()
    {
        SwitchToCommand = DelegateCommandBuilder.NoParameter()
            .OnExecute(OnSwitchTo)
            .Build();
    }

    public ICommand SwitchToCommand { get; }

    protected override void OnViewAttached(object view, string? context)
    {
        base.OnViewAttached(view, context);
        _view = view;
        _dispatcher = ViewHelper.GetDispatcher(view);
    }

    protected override void OnViewDetached(object view, string? context)
    {
        _view = null;
        _dispatcher = CurrentThreadDispatcher.Instance;
        base.OnViewDetached(view, context);
    }

    private async Task OnSwitchTo()
    {
        var view = _view ?? throw new InvalidOperationException("A view must be attached.");

        // Move off UI thread
        await Task.Delay(10).ConfigureAwait(false);

        // Switch back to UI thread
        await ViewHelper.SwitchToViewAsync(view);
        Debug.Assert(ViewHelper.CheckAccess(view));

        Trace.TraceInformation("On UI thread after SwitchToViewAsync().");
    }
}
```

## Event Aggregator Integration

The EventAggregator supports dispatching handlers to the UI thread:

```csharp
// Handler will run on the UI thread
_subscription = eventAggregator.Subscribe<MyViewModel, DataUpdatedMessage>(
    this,
    (target, message) => target.HandleDataUpdated(message),
    dispatcher: _dispatcher);
```

## Best Practices

### 1. Use SwitchToViewAsync for Clean Async Code

The explicit view awaitable keeps async code readable:

```csharp
private async Task ProcessAsync()
{
    await Task.Run(() => HeavyComputation()).ConfigureAwait(false);
    await ViewHelper.SwitchToViewAsync(view);
    UpdateUI();
}
```

### 2. Get Dispatcher from View

Always obtain the dispatcher when the view is attached:

```csharp
protected override void OnViewAttached(object view, string? context)
{
    base.OnViewAttached(view, context);
    _dispatcher = ViewHelper.GetDispatcher(view);
}
```

### 3. Use CurrentThreadDispatcher as Fallback

Use `CurrentThreadDispatcher.Instance` before the view is attached:

```csharp
private IDispatcher _dispatcher = CurrentThreadDispatcher.Instance;
```

### 4. Avoid Blocking the UI Thread

Don't use synchronous waits on the UI thread:

```csharp
// Bad - blocks UI thread
Task.Run(() => LoadData()).Wait();

// Good - async all the way
await Task.Run(() => LoadData()).ConfigureAwait(false);
await ViewHelper.SwitchToViewAsync(view);
```

## Platform-Specific Notes

### WPF

Uses the view's WPF dispatcher through its `IViewAdapter`.

### WinUI

Uses the view's WinUI dispatcher through its `IViewAdapter`.

### Avalonia

Uses the view's Avalonia dispatcher through its `IViewAdapter`.

## See Also

- [Async (Task Support)](async.md) - Asynchronous programming patterns
- [BindableCollection](bindable-collection.md) - Observable collections
- [Event Aggregator](event-aggregator.md) - Pub/sub messaging with dispatcher support
