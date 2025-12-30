# The Event Aggregator

Caliburn.Light comes pre-bundled with an Event Aggregator, conveniently called EventAggregator. For those unfamiliar, an Event Aggregator is a service that provides the ability to publish an object from one entity to another in a loosely based fashion. Event Aggregator is actually a pattern and its implementation can vary from framework to framework. For Caliburn.Light we focused on making our Event Aggregator implementation simple to use without sacrificing features or flexibility.

## Getting Started

The `IEventAggregator` interface defines the contract:

```csharp
public interface IEventAggregator
{
    IEventAggregatorHandler Subscribe<TTarget, TMessage>(
        TTarget target, 
        Action<TTarget, TMessage> handler, 
        IDispatcher? dispatcher = default)
        where TTarget : class;

    IEventAggregatorHandler Subscribe<TTarget, TMessage>(
        TTarget target, 
        Func<TTarget, TMessage, Task> handler, 
        IDispatcher? dispatcher = default)
        where TTarget : class;

    void Unsubscribe(IEventAggregatorHandler handler);

    void Publish(object message);
}
```

## Registration

Register the EventAggregator with your dependency injection container:

```csharp
// Using Microsoft.Extensions.DependencyInjection
services.AddSingleton<IEventAggregator, EventAggregator>();
```

## Publishing Events

Once you have obtained a reference to the EventAggregator instance you are free to begin publishing Events. An Event or message can be any object you like. There is no requirement to build your Events in any specific fashion.

```csharp
public class PublisherViewModel
{
    private readonly IEventAggregator _eventAggregator;

    public PublisherViewModel(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
    }

    public void SendMessage()
    {
        _eventAggregator.Publish(new MyMessage("Hello World"));
        _eventAggregator.Publish("Simple string message");
        _eventAggregator.Publish(42);
    }
}
```

## Subscribing to Events

Any entity can subscribe to events by calling the Subscribe method on the EventAggregator:

```csharp
public class SubscriberViewModel
{
    private readonly IEventAggregatorHandler _subscription;

    public SubscriberViewModel(IEventAggregator eventAggregator)
    {
        // Subscribe with a handler that receives the target and message
        _subscription = eventAggregator.Subscribe<SubscriberViewModel, MyMessage>(
            this,
            (target, message) => target.HandleMessage(message));
    }

    private void HandleMessage(MyMessage message)
    {
        // Handle the message here
    }
}
```

### Async Handlers

You can also use async handlers:

```csharp
_subscription = eventAggregator.Subscribe<SubscriberViewModel, MyMessage>(
    this,
    async (target, message) => await target.HandleMessageAsync(message));
```

### Thread Dispatching

You can specify an `IDispatcher` to control which thread the handler runs on:

```csharp
_subscription = eventAggregator.Subscribe<SubscriberViewModel, MyMessage>(
    this,
    (target, message) => target.HandleMessage(message),
    dispatcher: uiDispatcher);
```

## Unsubscribing

The Subscribe method returns an `IEventAggregatorHandler` that can be used to unsubscribe:

```csharp
public class SubscriberViewModel : IDisposable
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IEventAggregatorHandler _subscription;

    public SubscriberViewModel(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        _subscription = eventAggregator.Subscribe<SubscriberViewModel, MyMessage>(
            this,
            (target, message) => target.HandleMessage(message));
    }

    public void Dispose()
    {
        _eventAggregator.Unsubscribe(_subscription);
    }

    private void HandleMessage(MyMessage message)
    {
        // Handle the message
    }
}
```

## Weak References

The EventAggregator uses weak references to hold handlers. This means that if your subscriber is garbage collected, it will automatically be removed from the subscription list. However, it's still good practice to explicitly unsubscribe when you're done with a subscription.

## Subscribing to Multiple Events

You can subscribe to multiple event types by calling Subscribe multiple times:

```csharp
public class MultiSubscriberViewModel
{
    private readonly List<IEventAggregatorHandler> _subscriptions = new();

    public MultiSubscriberViewModel(IEventAggregator eventAggregator)
    {
        _subscriptions.Add(eventAggregator.Subscribe<MultiSubscriberViewModel, StringMessage>(
            this, (t, m) => t.HandleString(m)));
        
        _subscriptions.Add(eventAggregator.Subscribe<MultiSubscriberViewModel, NumberMessage>(
            this, (t, m) => t.HandleNumber(m)));
    }

    private void HandleString(StringMessage message) { }
    private void HandleNumber(NumberMessage message) { }
}
```
