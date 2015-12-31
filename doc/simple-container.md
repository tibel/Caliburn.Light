---
layout: page
title: Simple IoC Container
---

Caliburn.Micro comes pre-bundled with a Dependency Injection container called SimpleContainer. For those unfamiliar, a dependency injection container is an object that is used to hold dependency mappings for use later in an app via Dependency Injection. Dependency Injection is actually a pattern typically using the container element instead of manual service mapping.

### Getting Started

SimpleContainer is the main class used for Dependency Injection in Caliburn.Micro. There are other classes which act as supporting infrastructure in the form of extension methods, which are discussed later. For now, the public definition of SimpleContainer is shown below.

``` csharp
public SimpleContainer()

void RegisterInstance(Type service, string key, object implementation)
void RegisterPerRequest(Type service, string key, Type implementation)
void RegisterSingleton(Type service, string key, Type implementation)
void RegisterHandler(Type service, string key Func<SimpleContainer, object> handler)
void UnregisterHandler(Type service, string key)

object GetInstance(Type service, string key)
IEnumerable<object> GetAllInstances(Type service)
void BuildUp(object instance)

SimpleContainer CreateChildContainer()
bool HasHandler(Type service, string key)

event Action<object> Activated
```

As you can see above, the api is broken into service registration and retrieval with a couple of support methods and a single event.

### Creation and Configuration

Before adding any service bindings to SimpleContainer, it is important to remember that the container itself must be registered with Caliburn.Micro for the framework to make use of the aforementioned bindings. This process will inject SimpleContainer into IoC which is Caliburn.Micro's built in Service Locator. The code below details how to register SimpleContainer with Caliburn.Micro.

``` csharp
public class CustomBootstrapper : BootstrapperBase {
	private SimpleContainer _container = new SimpleContainer();

	...

	protected override object GetInstance(Type serviceType, string key) {
		return _container.GetInstance(serviceType, key);
	}

	protected override IEnumerable<object> GetAllInstances(Type serviceType) {
		return _container.GetAllInstances(serviceType);
	}

	protected override void BuildUp(object instance) {
		_container.BuildUp(instance);
	}

	...
}
```

As you can see above there are 3 methods which need to be overriden to correctly register SimpleContainer with Caliburn.Micro. You may refer to the Bootstrapper documentation for more information on the methods above.

### Registering Service Bindings

SimpleContainer provides many different ways to create service bindings based on lifecycle needs. On top of this, there are a number of extension methods included with Caliburn.Micro that provide even more flexibility when choosing to register services.

##### Register Instance

The RegisterInstance method allows a pre-constructed instance to be registered with the container against a type, key or both. Instance on the other hand registers a pre-constructed instance to be registered against a type only.

``` csharp
void RegisterInstance(Type serviceType, string key, object instance);
SimpleContainer Instance<TService>(TService instance)
```

Registering a pre-constructed instance is useful when an entity is required to be in a specific state before the first request for it is made.

##### Register Per Request

SimpleContainer contains 2 methods to handle per request registration. RegisterPerRequest registers an implementation to be registered against a type, key or both. PerRequest is overloaded to enable registration against the implementation's type or another type it implements or inherits from. PerRequest calls can be chained.

``` csharp
void RegisterPerRequest(Type serviceType, string key, Type implementation);
SimpleContainer PerRequest<TService, TImplementation>()
SimpleContainer PerRequest<TImplementation>()
```

Per Request registration causes the creation of the returned entity once per request. This means that two different requests for the same entity will result in two distinct instances being created.

##### Register Singleton

Singleton registration, like PerRequest, has 2 different methods for registration. RegisterSingleton registers an implementation against a type, key or both while Singleton is overloaded to enable registration against the implementation's type or another type it implements or inherits from. Singleton calls can be chained.

``` csharp
void RegisterSingleton(Type serviceType, string key, Type implementation);
SimpleContainer Singleton<TImplementation>()
SimpleContainer Singleton<TService, TImplementation>()
```

Registering Singletons may look the same as registering instances but there is an important difference in lifecycle. Instances are pre constructed before registration while singleton registrations are only constructed when first requested.

##### Register Handler

Factories, or more specifically, factory methods can be registered with the Handler method. The Handler method takes a Func<SimpleContainer, object> as its parameter; This allows the factory method to take advantage of the container itself which is useful in complex construction scenarios.

``` csharp
SimpleContainer Handler<TService>(Func<SimpleContainer, object> handler)
```

Some registrations may require multiple context sensitive implementations to be registered. Handles provides a convenient way to wrap this logic up and make use of it at the time of requesting.

Note: All of the above registration methods actually use Handles under the covers.

##### Register All Types Of

SimpleContainer provides basic assembly inspection. AllTypesOf allows an assembly inspected for any Implementation that implements or inherits the service type being registered. Optionally a filter can be provided to narrow the collection of implementations registered.


``` csharp
SimpleContainer AllTypesOf<TService>(Assembly assembly, Func<Type, bool> filter = null)
```

Assembly inspection is useful when dealing with modular systems where assemblies may not be available or known until run time.

### Injecting Services

The main benefit of Dependency injection is that any service requested will have it's dependencies resolved before it is returned to the caller. This is recursive so dependencies are satisfied for the whole object graph returned. This process can also be utilized on instances that did not originate from the dependency container in the form of property injection.

##### Constructor Injection

Constructor injection is the most widely used form of dependency injection and it denotes a required dependency between services and the class into which they are injected. Constructor injection should be used when you require the non optional use of a given service.

``` csharp
public class ShellViewModel {
	private readonly IWindowManager _windowManager;
	
	public ShellViewModel(IWindowManager windowManager) {
		_windowManager = windowManager;
	}
}
```

By specifying IWindowManager as a constructor parameter we are explicitly requesting it as a non optional service. If ShellViewModel gets constructed by the dependency container it will have an implementation of IWindowManager injected into it.

##### Property Injection

Property Injection provides the ability to inject services into an entity created outside of the dependency container. When an entity is passed into the BuildUp method its properties will be inspected and any available matching services will be injected using the same recursive logic as above.

``` csharp
...
		var shellViewModel = new ShellViewModel();
		_container.BuildUp(shellViewModel);
	}
}


public class ShellViewModel {
	public IEventAggregator EventAggregator { get; set; }
}
```

In most cases constructor injection is the best option because it makes service requirements explicit, however property injection has many use cases. It is important to note that property injection only works for Interface types. 

### Advanced Features

The techniques discussed in the Getting Started section are more than enough for most applications but SimpleContainer also provides some advanced features that can help with complex registration or retrieval scenarios.

##### Handling Instance Activation

SimpleContainer provides the Activate event that is raised when a service is requested from the container and its corresponding implementation is created thus allowing you to perform any custom initialization or operation you wish on the newly created instance. An Example of this can be seen below.

``` csharp
class CustomBootstrapper : BootstrapperBase {

	private SimpleContainer _container

	...
			
	protected override void Configure() {
		...
		_container.Activate += _OnInstanceActivation;
	}

	private void OnInstanceActivation(object instance) {
		// Perform any custom operations
	}
}
```

##### Utilizing Child Containers

Child Containers are useful in complex modular application scenarios. Requesting a child container will create a new instance of SimpleContainer with all of the currently registered services copied over. Any new instances registered with the parent will not be registered in the child container implicitly

``` csharp
var childContainer = _simpleContainer.CreateChildContainer();
```

Do remember that due to the nature of singleton and instance registrations that both containers will effectively be accessing the same instance. This allows complex registration scenarios to be achieved.