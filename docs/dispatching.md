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

### Extension Method

The `DispatcherHelper` class provides an extension method:

```csharp
// Enables await to switch to the UI thread
await dispatcher.SwitchTo();
```

## Getting the Dispatcher

The dispatcher is obtained through `ViewHelper.GetDispatcher(view)`. The recommended pattern is to use `ViewAware` as a base class:

```csharp
public class MyViewModel : ViewAware
{
    private IDispatcher _dispatcher = CurrentThreadDispatcher.Instance;

    protected override void OnViewAttached(object view, string context)
    {
        base.OnViewAttached(view, context);
        _dispatcher = ViewHelper.GetDispatcher(view);
    }

    protected override void OnViewDetached(object view, string context)
    {
        _dispatcher = CurrentThreadDispatcher.Instance;
        base.OnViewDetached(view, context);
    }
}
```

## Usage Examples

### Using SwitchTo() - Recommended Pattern

The `SwitchTo()` extension method provides a clean way to switch to the UI thread:

```csharp
public class DataViewModel : ViewAware
{
    private IDispatcher _dispatcher = CurrentThreadDispatcher.Instance;

    protected override void OnViewAttached(object view, string context)
    {
        base.OnViewAttached(view, context);
        _dispatcher = ViewHelper.GetDispatcher(view);
    }

    private async Task LoadDataAsync()
    {
        // Start on UI thread
        Status = "Loading...";

        // Move to background thread
        await Task.Delay(10).ConfigureAwait(false);
        
        // Now on background thread
        Debug.Assert(!_dispatcher.CheckAccess());
        var data = LoadDataFromDatabase();

        // Switch back to UI thread
        await _dispatcher.SwitchTo();
        
        // Now on UI thread
        Debug.Assert(_dispatcher.CheckAccess());
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
    _dispatcher.BeginInvoke(() =>
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
    if (_dispatcher.CheckAccess())
    {
        // Already on UI thread
        Status = message;
    }
    else
    {
        // Need to marshal to UI thread
        _dispatcher.BeginInvoke(() => Status = message);
    }
}
```

## ConfigureAwait Behavior

Understanding `ConfigureAwait` is important when working with dispatchers:

```csharp
private async Task OnConfigureAwaitFalse()
{
    await Task.Delay(10).ConfigureAwait(false);
    // Now on ThreadPool thread, NOT UI thread
    Debug.Assert(!_dispatcher.CheckAccess());
}

private async Task OnConfigureAwaitTrue()
{
    await Task.Delay(10).ConfigureAwait(true);
    // Back on UI thread (captured context)
    Debug.Assert(_dispatcher.CheckAccess());
}
```

## Complete Example

Here's a complete example from the gallery samples:

```csharp
public sealed class ThreadingViewModel : ViewAware, IHaveDisplayName
{
    private IDispatcher _dispatcher = CurrentThreadDispatcher.Instance;

    public string? DisplayName => "Threading";

    public ThreadingViewModel()
    {
        SwitchToCommand = DelegateCommandBuilder.NoParameter()
            .OnExecute(OnSwitchTo)
            .Build();
    }

    public ICommand SwitchToCommand { get; }

    protected override void OnViewAttached(object view, string context)
    {
        base.OnViewAttached(view, context);
        _dispatcher = ViewHelper.GetDispatcher(view);
    }

    protected override void OnViewDetached(object view, string context)
    {
        _dispatcher = CurrentThreadDispatcher.Instance;
        base.OnViewDetached(view, context);
    }

    private async Task OnSwitchTo()
    {
        // Move off UI thread
        await Task.Delay(10).ConfigureAwait(false);
        Debug.Assert(!_dispatcher.CheckAccess());

        // Switch back to UI thread
        await _dispatcher.SwitchTo();
        Debug.Assert(_dispatcher.CheckAccess());

        Trace.TraceInformation("On UI thread after SwitchTo().");
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

### 1. Use SwitchTo() for Clean Async Code

The `SwitchTo()` pattern keeps your async code readable:

```csharp
private async Task ProcessAsync()
{
    await Task.Run(() => HeavyComputation()).ConfigureAwait(false);
    await _dispatcher.SwitchTo();
    UpdateUI();
}
```

### 2. Get Dispatcher from View

Always obtain the dispatcher when the view is attached:

```csharp
protected override void OnViewAttached(object view, string context)
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
await _dispatcher.SwitchTo();
```

## Platform-Specific Notes

### WPF

Uses `System.Windows.Threading.Dispatcher` internally via `View.GetDispatcherFrom()`.

### WinUI

Uses `Microsoft.UI.Dispatching.DispatcherQueue` internally via `View.GetDispatcherFrom()`.

### Avalonia

Uses `Avalonia.Threading.Dispatcher.UIThread` internally via `View.GetDispatcherFrom()`.

## See Also

- [Async (Task Support)](async.md) - Asynchronous programming patterns
- [BindableCollection](bindable-collection.md) - Observable collections
- [Event Aggregator](event-aggregator.md) - Pub/sub messaging with dispatcher support
