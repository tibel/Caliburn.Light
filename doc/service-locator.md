# Service Locator

Caliburn.Light comes pre-bundled with a static Service Locator called IoC. For those unfamiliar, a Service Locator is an entity that can provide another entity with service instances, usually based on some type or key. Service Locator is actually a pattern and is related to Inversion of Control. Many consider Service Locator to be an anti-pattern but like all patterns it has its use cases. 

### Getting Started

IoC is the static entity used for Service Location in Caliburn.Light, this enables IoC to work with static entities such as dependency properties with ease. The public definition of IoC is shown below.

``` csharp
public static class IoC
{
  public static void Initialize(IServiceLocator serviceLocator) { }

  public static object GetInstance(Type service, string key = null) { }
  public static T GetInstance<T>(string key = null) { }

  public static IEnumerable<object> GetAllInstances(Type service) { }
  public static IEnumerable<T> GetAllInstances<T>() { }
}
```

As you can see above much of the functionality of IoC is dependent on the consumer providing it. In most cases the relevant methods required map directly to methods provided by all Dependency Injection containers (although the name and functionality may differ).

### Injecting IoC with functionality.

Caliburn.Light requires IoC to work correctly because it takes advantage of it at a framework level. As a non optional service we provide an extensibility point `IServiceLocator` for the purposes of injecting functionality into IoC. Caliburn.Light's own SimpleContainer implements this interface to inject functionality into IoC.

By mapping your chosen dependency container to IoC, Calburn.Light can take advantage of any service bindings made on the container via Service Location. 

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
