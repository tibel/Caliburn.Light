---
layout: page
title: Service Locator
---

Caliburn.Micro comes pre-bundled with a static Service Locator called IoC. For those unfamiliar, a Service Locator is an entity that can provide another entity with service instances, usually based on some type or key. Service Locator is actually a pattern and is related to Inversion of Control. Many consider Service Locator to be an anti-pattern but like all patterns it has its use cases. 

### Getting Started

IoC is the static entity used for Service Location in Caliburn.Micro, this enables IoC to work with static entites such as dependency properties with ease. The public definition of IoC is shown below.

``` csharp
public static class IoC {
	public static Func<Type, string, object> GetInstance;
        public static Func<Type, IEnumerable<object>> GetAllInstances;
        public static Action<object> BuildUp;

        public static T Get<T>(string key = null);
	public static IEnumerable<T> GetAll<T>();
}
```

As you can see above much of the functionality of IoC is dependant on the consumer providing it. In most cases the relevant methods required map directly to methods provided by all Dependency Injection containers (although the name and functionality may differ).

### Injecting IoC with functionality.

Caliburn.Micro requires IoC to work correctly because it takes advantage of it at a framework level. As a non optional service we provide an extensibility point directly on the Bootstrapper for the purposes of injecting functionality into IoC. Below, the sample uses Caliburn.Micro's own SimpleContainer to inject functionality into IoC.

``` csharp
public class CustomBootstrapper : Bootstrapper {
	private SimpleContainer _container = new SimpleContainer();
		
	//...

	protected override object GetInstance(Type service, string key) {
		return _container.GetInstance(service, key);
	}

	protected override IEnumerable<object> GetAllInstances(Type service) {
		return _container.GetAllInstances(service);
	}

	protected override void BuildUp(object instance) {
		_container.BuildUp(instance);
	}

	//...
}
```

By mapping your chosen dependency container to IoC, Calburn.Micro can take advantage of any service bindings made on the container via Service Location. 

### Using IoC in your application

As stated at the outset Service Location, apart from a few specific areas, is considered by many to be an anti pattern; in most cases you will want to make use of your dependency injection container. Many problems that Service Locator solves can be fixed without it by planning out your applications composition; refer to Screens, Conductors & Composition for more information on composition.

However, if you still require Service Location, IoC makes it easy. The code below shows how to use the service locator to retrieve or inject instances with services.

##### Getting a single service

IoC supports the retrieval of a single type by type or type and key. Key-based retrieval is not supported by all dependency injection containers, because of this the key param is optional.

``` csharp
var windowManager = IoC.Get<IWindowManager>();
var windowManager = IoC.Get<IWindowManager>("windowManager");
```

##### Getting a collection of services

Requesting a collection of services is also supported by IoC. The return type is IEnumerable T where T is the type of service requested. LINQ can be used to filter the final collection but be aware that at this point any entity in the collection will have already been instantiated.

``` csharp
var viewModelCollection = IoC.GetAll<IViewModel>();
var viewModel = IoC.GetAll<IViewModel>().FirstOrDefault(vm => vm.GetType() == typeof(ShellViewModel));
```

##### Injecting an instance with services

IoC supports the injection of services into a given instance. The mechanics of how this is done is left to the implementation injected into the Action<Instance> BuildUp field. There are various places in the framework were this is used to inject functionality into entities that were created externally to the dependency container mapped to IoC.

``` csharp
var viewModel = new LocallyCreatedViewModel();
IoC.BuildUp(viewModel);
```