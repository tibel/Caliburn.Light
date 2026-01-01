# Weak Event Handler

The `WeakEventHandler` static class provides extension methods to register weak event handlers for common .NET events. Weak event handlers prevent memory leaks by allowing the subscriber to be garbage collected even when the event source still holds a reference to the handler.

## The Problem with Standard Events

In .NET, when you subscribe to an event, the event source holds a strong reference to the subscriber. This can cause memory leaks when:

- The event source has a longer lifetime than the subscriber
- The subscriber forgets to unsubscribe from the event
- The subscriber cannot unsubscribe (e.g., no access to the source when the subscriber is disposed)

## The Solution

Caliburn.Light provides weak event handlers that use `WeakReference<T>` internally. This allows the garbage collector to reclaim the subscriber even if it hasn't explicitly unsubscribed from the event.

## Available Extension Methods

### RegisterPropertyChangingWeak

Registers a weak handler to `INotifyPropertyChanging.PropertyChanging`.

```csharp
public static IDisposable RegisterPropertyChangingWeak<TSubscriber>(
    this INotifyPropertyChanging source,
    TSubscriber subscriber,
    Action<TSubscriber, object?, PropertyChangingEventArgs> weakHandler)
    where TSubscriber : class
```

### RegisterPropertyChangedWeak

Registers a weak handler to `INotifyPropertyChanged.PropertyChanged`.

```csharp
public static IDisposable RegisterPropertyChangedWeak<TSubscriber>(
    this INotifyPropertyChanged source,
    TSubscriber subscriber,
    Action<TSubscriber, object?, PropertyChangedEventArgs> weakHandler)
    where TSubscriber : class
```

### RegisterCollectionChangedWeak

Registers a weak handler to `INotifyCollectionChanged.CollectionChanged`.

```csharp
public static IDisposable RegisterCollectionChangedWeak<TSubscriber>(
    this INotifyCollectionChanged source,
    TSubscriber subscriber,
    Action<TSubscriber, object?, NotifyCollectionChangedEventArgs> weakHandler)
    where TSubscriber : class
```

### RegisterCanExecuteChangedWeak

Registers a weak handler to `ICommand.CanExecuteChanged`.

```csharp
public static IDisposable RegisterCanExecuteChangedWeak<TSubscriber>(
    this ICommand source,
    TSubscriber subscriber,
    Action<TSubscriber, object?, EventArgs> weakHandler)
    where TSubscriber : class
```

## Usage

All methods return an `IDisposable` that can be used to explicitly unsubscribe from the event when needed.

### Basic Example

```csharp
public class MyViewModel : IDisposable
{
    private readonly IDisposable _subscription;
    private readonly SomeModel _model;

    public MyViewModel(SomeModel model)
    {
        _model = model;
        
        // Register a weak event handler
        _subscription = model.RegisterPropertyChangedWeak(this, 
            static (subscriber, sender, e) => subscriber.OnModelPropertyChanged(e));
    }

    private void OnModelPropertyChanged(PropertyChangedEventArgs e)
    {
        // Handle property change
        if (e.PropertyName == nameof(SomeModel.Name))
        {
            // React to change
        }
    }

    public void Dispose()
    {
        // Explicitly unsubscribe (optional but recommended)
        _subscription.Dispose();
    }
}
```

### Monitoring Collection Changes

```csharp
public class CollectionMonitor
{
    private readonly IDisposable _subscription;

    public CollectionMonitor(ObservableCollection<string> collection)
    {
        _subscription = collection.RegisterCollectionChangedWeak(this,
            static (subscriber, sender, e) => subscriber.OnCollectionChanged(e));
    }

    private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                // Handle items added
                break;
            case NotifyCollectionChangedAction.Remove:
                // Handle items removed
                break;
            // ... other cases
        }
    }
}
```

### Monitoring Command CanExecute

```csharp
public class CommandMonitor
{
    private readonly IDisposable _subscription;

    public CommandMonitor(ICommand command)
    {
        _subscription = command.RegisterCanExecuteChangedWeak(this,
            static (subscriber, sender, e) => subscriber.OnCanExecuteChanged());
    }

    private void OnCanExecuteChanged()
    {
        // React to CanExecute state change
    }
}
```

## How It Works

The weak event handler infrastructure consists of:

1. **`WeakEventHandlerBase<TSource, TSubscriber, TEventArgs>`** - An abstract base class that:
   - Stores weak references to both the event source and subscriber
   - Provides the `OnEvent` method that checks if the subscriber is still alive
   - Automatically unsubscribes when the subscriber has been garbage collected
   - Implements `IDisposable` for explicit unsubscription

2. **Concrete implementations** for each event type (e.g., `WeakNotifyPropertyChangedHandler<TSubscriber>`) that:
   - Subscribe to the specific event in the constructor
   - Override `RemoveEventHandler` to unsubscribe from the event

### Automatic Cleanup

When an event is raised and the subscriber has been garbage collected:
1. The `OnEvent` method detects the subscriber is no longer alive
2. It automatically calls `RemoveEventHandler` to unsubscribe from the event
3. This prevents the "dead" handler from continuing to receive events

## Best Practices

1. **Use static lambdas** - Use the `static` keyword on your lambda expressions to avoid capturing `this` and creating closures, which would defeat the purpose of weak references:

   ```csharp
   // Good - static lambda doesn't capture 'this'
   source.RegisterPropertyChangedWeak(this, 
       static (subscriber, sender, e) => subscriber.HandleEvent(e));

   // Bad - non-static lambda captures 'this' strongly
   source.RegisterPropertyChangedWeak(this, 
       (subscriber, sender, e) => this.HandleEvent(e));
   ```

2. **Store the disposable** - Keep a reference to the returned `IDisposable` if you need to unsubscribe explicitly.

3. **Dispose when possible** - While weak handlers prevent memory leaks, explicitly disposing is still recommended for deterministic cleanup.

## Extending for Custom Events

You can create your own weak event handlers by inheriting from `WeakEventHandlerBase<TSource, TSubscriber, TEventArgs>`:

```csharp
internal sealed class WeakCustomEventHandler<TSubscriber> :
    WeakEventHandlerBase<ICustomEventSource, TSubscriber, CustomEventArgs>
    where TSubscriber : class
{
    public WeakCustomEventHandler(
        ICustomEventSource source, 
        TSubscriber subscriber,
        Action<TSubscriber, object?, CustomEventArgs> weakHandler)
        : base(source, subscriber, weakHandler)
    {
        source.CustomEvent += OnEvent;
    }

    protected override void RemoveEventHandler(ICustomEventSource source)
    {
        source.CustomEvent -= OnEvent;
    }
}

// Extension method
public static class CustomWeakEventExtensions
{
    public static IDisposable RegisterCustomEventWeak<TSubscriber>(
        this ICustomEventSource source,
        TSubscriber subscriber,
        Action<TSubscriber, object?, CustomEventArgs> weakHandler)
        where TSubscriber : class
    {
        return new WeakCustomEventHandler<TSubscriber>(source, subscriber, weakHandler);
    }
}
```

For static events, use the two-type-parameter version `WeakEventHandlerBase<TSubscriber, TEventArgs>` and override the parameterless `RemoveEventHandler()` method.

## WeakEventSource

While `WeakEventHandler` helps subscribers avoid memory leaks when subscribing to events, `WeakEventSource` solves the problem from the publisher's side. It allows event publishers to hold only weak references to their subscribers, preventing the publisher from keeping subscribers alive.

### When to Use WeakEventSource

Use `WeakEventSource` when:

- You are implementing a custom event on a class
- The event source typically has a longer lifetime than subscribers
- You want to prevent memory leaks even if subscribers forget to unsubscribe

### Available Classes

#### WeakEventSource

A weak event source for standard `EventHandler` delegates with `EventArgs`.

```csharp
public sealed class WeakEventSource : WeakEventSourceBase<EventHandler>
{
    public void Add(EventHandler eventHandler);
    public void Remove(EventHandler eventHandler);
    public void Raise(object? sender, EventArgs e);
}
```

#### WeakEventSource&lt;TEventArgs&gt;

A generic weak event source for `EventHandler<TEventArgs>` delegates with custom event arguments.

```csharp
public sealed class WeakEventSource<TEventArgs> : WeakEventSourceBase<EventHandler<TEventArgs>>
    where TEventArgs : EventArgs
{
    public void Add(EventHandler<TEventArgs> eventHandler);
    public void Remove(EventHandler<TEventArgs> eventHandler);
    public void Raise(object? sender, TEventArgs e);
}
```

### Usage Example

```csharp
public class DataService
{
    // Use WeakEventSource instead of a regular event
    private readonly WeakEventSource _dataChanged = new();

    // Expose as a standard event pattern
    public event EventHandler DataChanged
    {
        add => _dataChanged.Add(value);
        remove => _dataChanged.Remove(value);
    }

    public void UpdateData()
    {
        // ... update data logic ...

        // Raise the event
        _dataChanged.Raise(this, EventArgs.Empty);
    }
}
```

### Example with Custom EventArgs

```csharp
public class ItemChangedEventArgs : EventArgs
{
    public ItemChangedEventArgs(string itemName, ChangeType changeType)
    {
        ItemName = itemName;
        ChangeType = changeType;
    }

    public string ItemName { get; }
    public ChangeType ChangeType { get; }
}

public enum ChangeType { Added, Removed, Modified }

public class ItemRepository
{
    private readonly WeakEventSource<ItemChangedEventArgs> _itemChanged = new();

    public event EventHandler<ItemChangedEventArgs> ItemChanged
    {
        add => _itemChanged.Add(value);
        remove => _itemChanged.Remove(value);
    }

    public void AddItem(string name)
    {
        // ... add item logic ...

        _itemChanged.Raise(this, new ItemChangedEventArgs(name, ChangeType.Added));
    }

    public void RemoveItem(string name)
    {
        // ... remove item logic ...

        _itemChanged.Raise(this, new ItemChangedEventArgs(name, ChangeType.Removed));
    }
}
```

### How It Works

`WeakEventSource` uses `WeakEventSourceBase<TEventHandler>` internally, which:

1. **Stores handlers weakly** - Event handlers are stored using weak references, allowing subscribers to be garbage collected even if they don't explicitly unsubscribe
2. **Thread-safe** - All operations are protected by a lock for safe concurrent access
3. **Automatic cleanup** - Dead (garbage collected) handlers are automatically removed when the event is raised
4. **Null-safe** - Adding or removing `null` handlers is safely ignored

### Key Behaviors

| Operation | Behavior |
|-----------|----------|
| `Add(null)` | Safely ignored, no exception thrown |
| `Remove(null)` | Safely ignored, no exception thrown |
| `Raise()` with no handlers | Works without error |
| Subscriber garbage collected | Handler automatically cleaned up on next `Raise()` |
| Multiple handlers from same target | All handlers invoked correctly |

### WeakEventSource vs WeakEventHandler

| Aspect | WeakEventSource | WeakEventHandler |
|--------|-----------------|------------------|
| **Implemented by** | Event publisher | Event subscriber |
| **Weak reference to** | Subscribers | Subscriber |
| **Use case** | Custom events you're defining | Existing events you're subscribing to |
| **Returns** | N/A | `IDisposable` for unsubscription |

Use `WeakEventSource` when you're creating a class that publishes events. Use `WeakEventHandler` extension methods when you're subscribing to events from other classes (especially framework events like `PropertyChanged`).
