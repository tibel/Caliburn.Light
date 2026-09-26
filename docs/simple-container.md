# SimpleContainer

`SimpleContainer` is a lightweight dependency injection container included in the `Caliburn.Light` package. It supports instance, per-request, and singleton registrations, constructor injection, keyed registrations, and child containers.

The current Caliburn.Light configuration examples use `Microsoft.Extensions.DependencyInjection`. `SimpleContainer` is an alternative when you want a smaller container; it is not automatically installed as a framework-wide service locator. Register the services your application needs and resolve them from the container.

## Creating and registering services

Create a container and register services by their service type and implementation:

```csharp
using Caliburn.Light;

var container = new SimpleContainer();

container.RegisterSingleton<IMessageService, MessageService>();
container.RegisterPerRequest<IPageViewModel, HomeViewModel>();
container.RegisterInstance<ISettings>(settings);
```

You can also register a concrete type as itself:

```csharp
container.RegisterPerRequest<ReportViewModel>();
```

The generic registration methods accept an optional key. A key lets you register different implementations for the same service:

```csharp
container.RegisterPerRequest<IPageViewModel, HomeViewModel>("home");
container.RegisterPerRequest<IPageViewModel, SearchViewModel>("search");

var home = container.GetRequiredInstance<IPageViewModel>("home");
```

The non-generic overloads accept `Type` values for the service and implementation. Singleton registrations create their instance on first resolution and reuse it; per-request registrations create a new instance for each resolution. Instance registrations use the object supplied at registration time.

## Constructor injection

When creating a registered implementation, the container selects its public constructor with the most parameters and resolves each parameter from the container:

```csharp
public class HomeViewModel
{
    public HomeViewModel(IMessageService messageService)
    {
        // Use the registered service.
    }
}
```

Register constructor dependencies that are not otherwise handled by the container. It can also supply `IEnumerable<T>` containing all registrations for `T`, and `Func<T>` when `T` is registered. The container does not automatically construct unregistered reference-type dependencies. If multiple public constructors have the same maximum number of parameters, the selected constructor is not guaranteed.

## Factory registrations

Use a handler when an instance needs custom construction logic. The handler receives the container so it can resolve other services:

```csharp
container.RegisterPerRequest<IReport>(
    c => new Report(c.GetRequiredInstance<IMessageService>()));
```

Handlers can also be registered as singletons:

```csharp
container.RegisterSingleton<IMessageService>(
    _ => new MessageService(settings));
```

## Resolving services

`GetInstance` returns `null` when an unregistered reference type is requested. Use `GetRequiredInstance` when a missing service should be an error:

```csharp
var optional = container.GetInstance<IMessageService>();
var required = container.GetRequiredInstance<IMessageService>();
```

`GetAllInstances<TService>()` returns all registrations for a service. `IEnumerable<TService>` can also be requested through `GetInstance`. Resolving a single service with multiple matching registrations throws; use `GetAllInstances` when multiple implementations are expected.

The container implements `IServiceProvider`. Its `GetService(Type)` implementation is explicit, so access it through the interface:

```csharp
IServiceProvider serviceProvider = container;
var service = serviceProvider.GetService(typeof(IMessageService));
```

## Removing and checking registrations

Use `IsRegistered` to check for a registration and `UnregisterHandler` to remove the handlers for a service and key:

```csharp
if (container.IsRegistered<IMessageService>())
{
    container.UnregisterHandler<IMessageService>();
}
```

## Child containers

`CreateChildContainer` creates another container initialized with the current registrations:

```csharp
var child = container.CreateChildContainer();
```

Both containers can resolve the registrations they inherited. New service/key registrations added to the child are local to it. Registering another handler for an inherited service/key, however, adds it to the inherited registration as well; child containers do not provide isolated overrides for an existing service/key.

## Limitations

`SimpleContainer` uses reflection to create registered types and is annotated as requiring dynamic code and unreferenced members. Take care when using it in trimmed or Native AOT deployments.
