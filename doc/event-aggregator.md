---
layout: page
title: The Event Aggregator
---

Caliburn.Micro comes pre-bundled with an Event Aggregator, conveniently called EventAggregator. For those unfamiliar, an Event Aggregator is a service that provides the ability to publish an object from one entity to another in a loosely based fashion. Event Aggregator is actually a pattern and it's implementation can vary from framework to framework. For Caliburn.Micro we focused on making our Event Aggregator implementation simple to use without sacrificing features or flexibility.

### Getting Started

As previously mentioned we provide an implementation of Event Aggregator for you. This implementation implements the IEventAggregator interface, however, you can provide your own implementation if required. Please take a moment to familiarize yourself with the IEventAggregator signature.

``` csharp
public interface IEventAggregator {
    	bool HandlerExistsFor(Type messageType);
    	void Subscribe(object subscriber);
    	void Unsubscribe(object subscriber);
    	void Publish(object message, Action<Action> marshal);
    }
```

### Creation and Lifecycle

To use the EventAggregator correctly it must exist as an application level service. This is usually achieved by creating an instance of EventAggregator as a Singleton. We recommend that you use Dependency Injection to obtain a reference to the instance although we do not enforce this. The sample below details how to create an instance of EventAggregator, add it to the IoC container included with Caliburn.Micro (although you are free to use any container you wish) and request it in a ViewModel.

``` csharp
// Creating the EventAggregator as a singleton.
    public class Bootstrapper : BootstrapperBase {
        private readonly SimpleContainer _container =
            new SimpleContainer();

         // ... Other Bootstrapper Config

        protected override void Configure(){
            _container.Singleton<IEventAggregator, EventAggregator>();
        }

        // ... Other Bootstrapper Config
    }

    // Acquiring the EventAggregator in a viewModel.
    public class FooViewModel {
        private readonly IEventAggregator _eventAggregator;

        public FooViewModel(IEventAggregator eventAggregator) {
            _eventAggregator = eventAggregator;
        }
    }
```

Note that we are utilizing the Bootstrapper, and specifically the Configure method, in the code above. There is no requirement to wire up the EventAggregator in a specific location, simply ensure it is created before it is first requested.

### Publishing Events

Once you have obtained a reference to the EventAggregator instance you are free to begin publishing Events. An Event or message as we call it to distinguish between .Net events, can be any object you like. There is no requirement to build your Events in any specific fashion. As you can see in the sample below the Publish methods can accept any entity that derives from System.Object and will happily publish it to any interested subscribers.

``` csharp
public class FooViewModel {
        private readonly IEventAggregator _eventAggregator;

        public FooViewModel(IEventAggregator eventAggregator) {
            _eventAggregator = eventAggregator;

            _eventAggregator.PublishOnUIThread(new object());
            _eventAggregator.PublishOnUIThread("Hello World");
            _eventAggregator.PublishOnUIThread(22);
        }
    }
```

### Publishing using a custom thread Marshal

By convention, the EventAggregator publishes on the UI thread (using PublishOnUIThread() method). You can override this per publish. Consider the following code below which publishes the message supplied on a background thread.

``` csharp
_eventAggregator.Publish(new object(), action => {
                    Task.Factory.StartNew(action);
                });
```

### Subscribing To Events

Any entity can subscribe to any Event by providing itself to the EventAggregator via the Subscribe method. To keep this functionality easy to use we provide a special interface (IHandle<T>) which marks a subscriber as interested in an Event of a given type.

``` csharp
IHandle<TMessage> : IHandle {
	    void Handle<TMessage>(TMessage message);
    }
```

Notice that by implementing the interface above you are forced to implement the method Handle(T message) were T is the type of message you have specified your interest in. This method is what will be called when a matching Event type is published. 

``` csharp
public class FooViewModel : IHandle<object> {
        private readonly IEventAggregator _eventAggregator;

        public FooViewModel(IEventAggregator eventAggregator) {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
        }

        public void Handle(object message) {
            // Handle the message here.
        }
    }
```

### Subscribing To Many Events

It is not uncommon for a single entity to want to listen for multiple event types. Because of our use of generics this is as simple as adding a second IHandle<T> interface to the subscriber. Notice that Handle method is now overloaded with the new Event type.

``` csharp
public class FooViewModel : IHandle<string>, IHandle<bool> {
        private readonly IEventAggregator _eventAggregator;
    
        public FooViewModel(IEventAggregator eventAggregator) {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
        }
    
        public void Handle(string message) {
            // Handle the message here.
        }

        public void Handle(bool message) {
            // Handle the message here.
        }
    }
```

### Polymorphic Subscribers

Caliburn.Micro's EventAggregator honors polymorphism. When selecting handlers to call, the EventAggregator will fire any handler who's Event type is assignable from the Event being sent. This results in a lot of flexibility and helps reuse.

``` csharp
public class FooViewModel : IHandle<object>, IHandle<string> {
        private readonly IEventAggregator _eventAggregator;
    
        public FooViewModel(IEventAggregator eventAggregator) {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
            _eventAggregator.PublishOnUIThread("Hello");
        }
    
        public void Handle(object message) {
            // This will be called
        }

        public void Handle(string message) {
            // This also
        }
    }
```

In the example above, because String is derived from System.Object both handlers will be called when a String message is published.

### Querying Handlers

When a subscriber is passed to the EventAggregator it is broken down into a special object called a Handler and a weak reference is maintained. We provide a mechanism to query the EventAggregator to see if a given Event type has any handlers, this can be useful in specific scenarios were at least one handler is assumed to exist.

``` csharp
public class FooViewModel : IHandle<object> {
        private readonly IEventAggregator _eventAggregator;

        public FooViewModel(IEventAggregator eventAggregator) {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
        }

        public void Handle(object message){
            if (_eventAggregator.HandlerExistsFor(typeof(SpecialMessageEvent))){
                _eventAggregator.PublishOnUIThread(new SpecialEventMessage(message));
            }
        }
    }
```

### Coroutine Aware Subscribers

If you are using the EventAggregator with Caliburn.Micro as opposed to on it's own via Nuget, access to Coroutine support within the Event Aggregator becomes available. Coroutines are supported via the IHandleWithCoroutine<T> Interface.

``` csharp
public interface IHandleWithCoroutine<TMessage> : IHandle {
		IEnumerable<IResult> Handle(TMessage message);
    }
```

The code below utilizes Coroutines with the EventAggregator. In this instance Activate will be fired asynchronously, Do work however, will not be called until after Activate has completed.

``` csharp
public class FooViewModel : Screen, IHandleWithCoroutine<EventWithCoroutine> {
        private readonly IEventAggregator _eventAggregator;

        public FooViewModel(IEventAggregator eventAggregator) {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
        }

        public IEnumerable<IResult> Handle(EventWithCoroutine message) {
            yield return message.Activate();
            yield return message.DoWork();
        }
    }
```

``` csharp
public class EventWithCoroutine {
        public IResult Activate() {
            return new TaskResult(Task.Factory.StartNew(() => {
                    // Activate logic
                }));
        }

        public IResult DoWork() {
            return new TaskResult(Task.Factory.StartNew(() => {
                // Do work logic
            }));
        }
    }
```

### Task Aware Subscribers

Caliburn.Micro also provides support for task based subscribers where the asynchronous functionality of Coroutines is desired but in a more light weight fashion. To utilize this functionality implement the IHandleWithTask<T> Interface, seen below.

``` csharp
public interface IHandleWithTask<TMessage> : IHandle {
        Task Handle(TMessage message);
    }
```

Any subscribers that implement the above interface can then handle events in Task based manner.

``` csharp
public class FooViewModel : Screen, IHandleWithTask<object> {
        private readonly IEventAggregator _eventAggregator;

        public FooViewModel(IEventAggregator eventAggregator) {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
        }

        public Task Handle(object message) {
            return Task.Factory.StartNew(() => message);
        }
    }
```

### Unsubscribing and Leaks

The problem with standard .Net events is that they are prone to memory leaks. We avoid this situation by maintaining a weak reference to subscribers. If the only thing that references a subscriber is the EventAggregator then it will be allowed to go out of scope and ultimately be garbage collected. However, we still provide an explicit way to unsubscribe to allow for conditional handling as seen below.

``` csharp
public class FooViewModel : Screen, IHandle<object> {
        private readonly IEventAggregator _eventAggregator;

        public FooViewModel(IEventAggregator eventAggregator) {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
        }

        public void Handle(object message) {
            // Handle the message here.
        }

        protected override void OnActivate() {
            _eventAggregator.Subscribe(this);
            base.OnActivate();
        }

        protected override void OnDeactivate(bool close) {
            _eventAggregator.Unsubscribe(this);
            base.OnDeactivate(close);
        }
    }
```

In the code above Screen is used to expose lifecycle events on the ViewModel. More on this can be found on this in the Screens, Conductors and Composition article on this wiki.

### Custom Result Handling

In more complex scenarios it may be required to override the default handling of Handlers which have results. In this instance it is possible to swap out the existing implementation in favour of your own. First we create a new handler type.

``` csharp
IHandleAndReturnString<T> : IHandle {
	    string Handle<T>(T message);
    }
```

Next we create our new result processor. This can be configured in the bootstrapper.

``` csharp
var standardResultProcesser = EventAggregator.HandlerResultProcessing;
    EventAggregator.HandlerResultProcessing = (target, result) =>
    {
        var stringResult = result as string;
        if (stringResult != null)
            MessageBox.Show(stringResult);
        else
            standardResultProcesser(target, result);
    };
```

Now, any time an event is processed returns a string, it will be captured and displayed in a MessageBox. The new handler falls back to the default implementations in cases were the result is not assignable from string. It is extremely important to note however, this feature was not designed for request / response usage, treating it as such will most definitely create bottle necks on publish.
