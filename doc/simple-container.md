# Simple IoC Container

Caliburn.Light comes pre-bundled with a Dependency Injection container called SimpleContainer. For those unfamiliar, a dependency injection container is an object that is used to hold dependency mappings for use later in an app via Dependency Injection. Dependency Injection is actually a pattern typically using the container element instead of manual service mapping.

### Getting Started

SimpleContainer is the main class used for Dependency Injection in Caliburn.Light. There are other classes which act as supporting infrastructure in the form of extension methods, which are discussed later. For now, the public definition of SimpleContainer is shown below.

``` csharp
public SimpleContainer()

void RegisterInstance(Type service, string key, object implementation)
void RegisterPerRequest(Type service, string key, Type implementation)
void RegisterSingleton(Type service, string key, Type implementation)
void RegisterPerRequest(Type service, string key Func<SimpleContainer, object> handler)
void UnregisterHandler(Type service, string key)

object GetInstance(Type service, string key)
IEnumerable<object> GetAllInstances(Type service)

SimpleContainer CreateChildContainer()
bool IsRegistered(Type service, string key)
```

As you can see above, the API is broken into service registration and retrieval with a couple of support methods.

### Creation and Configuration

Before adding any service bindings to SimpleContainer, it is important to remember that the container itself must be registered with Caliburn.Light for the framework to make use of the aforementioned bindings. This process will inject SimpleContainer into IoC which is Caliburn.Light's built in Service Locator. The code below details how to register SimpleContainer with Caliburn.Light.

``` csharp
public class CustomBootstrapper : BootstrapperBase {
	private SimpleContainer _container;

    protected override void Configure()
    {
      _container = new SimpleContainer();
      IoC.Initialize(_container);
	  
	  //...
    }
}
```

As you can see above there are 3 methods which need to be overridden to correctly register SimpleContainer with Caliburn.Light. You may refer to the Bootstrapper documentation for more information on the methods above.

### Registering Service Bindings

SimpleContainer provides many different ways to create service bindings based on lifecycle needs. On top of this, there are a number of extension methods included with Caliburn.Light that provide even more flexibility when choosing to register services.

##### Register Instance

The RegisterInstance method allows a pre-constructed instance to be registered with the container against a type, key or both.

``` csharp
void RegisterInstance(Type serviceType, string key, object instance);
void RegisterInstance<TService>(TService instance)
```

Registering a pre-constructed instance is useful when an entity is required to be in a specific state before the first request for it is made.

##### Register Per Request

SimpleContainer contains 2 methods to handle per request registration. RegisterPerRequest registers an implementation to be registered against a type, key or both.

``` csharp
void RegisterPerRequest(Type serviceType, string key, Type implementation);
void RegisterPerRequest<TService, TImplementation>()
void RegisterPerRequest<TImplementation>()
```

Per Request registration causes the creation of the returned entity once per request. This means that two different requests for the same entity will result in two distinct instances being created.

##### Register Singleton

Singleton registration, like PerRequest, has 2 different methods for registration. RegisterSingleton registers an implementation against a type, key or both.

``` csharp
void RegisterSingleton(Type serviceType, string key, Type implementation);
void RegisterSingleton<TImplementation>()
void RegisterSingleton<TService, TImplementation>()
```

Registering Singletons may look the same as registering instances but there is an important difference in lifecycle. Instances are pre constructed before registration while singleton registrations are only constructed when first requested.

##### Register Handler

Factories, or more specifically, factory methods can be registered with the Handler method. The Handler method takes a Func<SimpleContainer, object> as its parameter; This allows the factory method to take advantage of the container itself which is useful in complex construction scenarios.

``` csharp
void RegisterPerRequest<TService>(Func<SimpleContainer, object> handler)
```

Some registrations may require multiple context sensitive implementations to be registered. Handles provides a convenient way to wrap this logic up and make use of it at the time of requesting.

Note: All of the above registration methods actually use Handles under the covers.


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

### Advanced Features

The techniques discussed in the Getting Started section are more than enough for most applications but SimpleContainer also provides some advanced features that can help with complex registration or retrieval scenarios.

##### Utilizing Child Containers

Child Containers are useful in complex modular application scenarios. Requesting a child container will create a new instance of SimpleContainer with all of the currently registered services copied over. Any new instances registered with the parent will not be registered in the child container implicitly

``` csharp
var childContainer = _simpleContainer.CreateChildContainer();
```

Do remember that due to the nature of singleton and instance registrations that both containers will effectively be accessing the same instance. This allows complex registration scenarios to be achieved.
