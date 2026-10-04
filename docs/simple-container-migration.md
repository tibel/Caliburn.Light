# Migrating from SimpleContainer to Microsoft.Extensions.DependencyInjection

`SimpleContainer` was removed from Caliburn.Light in 6.0.0, so applications upgrading from 5.x or earlier no longer have it. `Microsoft.Extensions.DependencyInjection` is the container the gallery samples use, and the mapping is mostly mechanical — but a few resolution rules differ in ways that only show up at runtime.

This page assumes you already know both APIs. For the target setup, see [Basic Configuration](configuration.md).

## Adding the package

```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="..." />
```

Add `Microsoft.Extensions.Options` as well if you register a `ViewModelLocatorConfiguration`, which is the usual case:

```xml
<PackageReference Include="Microsoft.Extensions.Options" Version="..." />
```

Match both to the .NET version your application targets.

Caliburn.Light does not require any particular container — everything it consumes is an `IServiceProvider`, as documented in [Basic Configuration](configuration.md). `Microsoft.Extensions.DependencyInjection` is the recommended choice, not a requirement.

## Building the provider

A `SimpleContainer` is both the registration list and the resolver. `Microsoft.Extensions.DependencyInjection` splits that into two steps: collect registrations in a `ServiceCollection`, then freeze them into an `IServiceProvider` with `BuildServiceProvider`.

```csharp
using Caliburn.Light;
using Caliburn.Light.WPF; // the platform namespace: Caliburn.Light.Avalonia or Caliburn.Light.WinUI
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

var services = new ServiceCollection();

// Register core Caliburn.Light services.
services.AddSingleton<IWindowManager, WindowManager>();
services.AddSingleton<IEventAggregator, EventAggregator>();
services.AddSingleton<IViewModelLocator, ViewModelLocator>();
services.AddTransient(sp => sp.GetRequiredService<IOptions<ViewModelLocatorConfiguration>>().Value);

// Register view-viewmodel mappings.
services.Configure<ViewModelLocatorConfiguration>(config => config.AddMapping<ShellView, ShellViewModel>());

// Register views and view models.
services.AddTransient<ShellView>();
services.AddTransient<ShellViewModel>();

var serviceProvider = services.BuildServiceProvider();
```

Keep the provider alive for the lifetime of the application, and dispose it on shutdown. The gallery samples hold an `IServiceProvider?` field and skip this step, which leaks the container's singletons for the process lifetime — harmless in a desktop app, but worth doing properly. The `Dispose` call is what releases the services the container created:

```csharp
protected override void OnExit(ExitEventArgs e)
{
    serviceProvider.Dispose();

    base.OnExit(e);
}
```

`BuildServiceProvider()` returns a `ServiceProvider`, which is what makes that `Dispose` call available. `IServiceProvider` does not implement `IDisposable`, so a field typed as `IServiceProvider` cannot be disposed directly — either store it as `ServiceProvider` or cast when shutting down:

```csharp
private readonly ServiceProvider _serviceProvider;
```

## Registration mapping

| SimpleContainer | Microsoft.Extensions.DependencyInjection |
|-----------------|-----------------------------------------|
| `new SimpleContainer()` | `new ServiceCollection()` plus `BuildServiceProvider()` |
| `RegisterSingleton<TService, TImplementation>()` | `AddSingleton<TService, TImplementation>()` |
| `RegisterPerRequest<TService, TImplementation>()` | `AddTransient<TService, TImplementation>()` |
| `RegisterSingleton<TImplementation>()` | `AddSingleton<TImplementation>()` |
| `RegisterPerRequest<TImplementation>()` | `AddTransient<TImplementation>()` |
| `RegisterInstance<TService>(instance)` | `AddSingleton<TService>(instance)` |
| `RegisterSingleton<TService>(handler)` | `AddSingleton<TService>(factory)` |
| `RegisterPerRequest<TService>(handler)` | `AddTransient<TService>(factory)` |
| `RegisterSingleton(service, implementation)` | `AddSingleton(service, implementation)` |
| `RegisterPerRequest(service, implementation)` | `AddTransient(service, implementation)` |

"Per request" in `SimpleContainer` means one instance per resolution, which is what `AddTransient` means in `Microsoft.Extensions.DependencyInjection`. Both containers create singletons lazily, on first resolution.

A factory delegate receives the provider rather than the container. Replace `c` with `sp` and use the `GetRequiredService` extension methods on it:

```diff
-container.RegisterPerRequest<IReport>(c => new Report(c.GetRequiredInstance<IMessageService>()));
+services.AddTransient<IReport>(sp => new Report(sp.GetRequiredService<IMessageService>()));
```

Unlike `SimpleContainer`, the one-argument `AddSingleton(instance)` overload infers the service type from the argument, so `container.RegisterInstance<ISettings>(settings)` becomes `services.AddSingleton(settings)` — which registers the concrete type of `settings`, not `ISettings`. Use `AddSingleton<ISettings>(settings)` whenever the instance is exposed through an interface.

## Resolution mapping

| SimpleContainer | Microsoft.Extensions.DependencyInjection |
|-----------------|-----------------------------------------|
| `GetRequiredInstance<TService>()` | `GetRequiredService<TService>()` |
| `GetInstance<TService>()` | `GetService<TService>()` |
| `GetAllInstances<TService>()` | `GetServices<TService>()` |
| `GetRequiredInstance(service)` | `GetRequiredService(service)` |
| `GetInstance(service)` | `GetService(service)` |
| `container` used as `IServiceProvider` | `serviceProvider` |
| `IsRegistered<TService>(key)` | `GetRequiredService<IServiceProviderIsService>().IsService(typeof(TService))` — unkeyed only |
| `UnregisterHandler<TService>()` | No equivalent; registrations are frozen once the provider is built |

`GetInstance` returning `null` for an unregistered service and `GetRequiredInstance` throwing match the behavior of `GetService` and `GetRequiredService`, so optional-dependency code translates directly.

Do not substitute `GetService<TService>() is not null` for `IsRegistered`. It returns non-null for `IEnumerable<TService>` even when nothing is registered, and for `IServiceProvider` itself, so it reports services that `IsRegistered` reports as absent. `IServiceProviderIsService` is registered automatically and its `IsService` is the closer match.

Two differences remain, and both are edge cases:

- `IsService(typeof(IEnumerable<TService>))` also returns `true` for an empty registration, where `IsRegistered<IEnumerable<TService>>` returned `false`.
- `IsService` takes no key, so the `IsRegistered<TService>(key)` overload has no equivalent. Check keyed registrations by resolving the key, or keep the key list yourself.

Separately, `GetInstance(serviceType)` returned `default(T)` for an unregistered value type, where MS DI returns `null` and `GetRequiredService` throws. Code that relied on getting `default` back has to register the type or handle the throw.

## Multiple registrations for the same service

This is the most common runtime break. `SimpleContainer` throws when a service is registered more than once under the same key, because it cannot choose between the handlers. `Microsoft.Extensions.DependencyInjection` does not throw: a single-service resolution returns the **last** registration, and `GetServices<TService>` returns all of them.

```csharp
services.AddTransient<IPageViewModel, HomeViewModel>();
services.AddTransient<IPageViewModel, SearchViewModel>();

serviceProvider.GetRequiredService<IPageViewModel>();    // SearchViewModel (last registration)
serviceProvider.GetServices<IPageViewModel>();          // HomeViewModel, SearchViewModel
```

Code that used to fail loudly now silently gets the wrong implementation. Audit every service you register more than once and either remove the duplicate or split it into distinct services.

## Keyed registrations

`SimpleContainer` keys are strings, and a keyless resolution falls back to the first registration for that service. `Microsoft.Extensions.DependencyInjection` treats keyed and unkeyed registrations as separate: a keyless resolution never sees a keyed registration, and a keyed resolution of an unregistered key fails.

```csharp
services.AddKeyedTransient<IPageViewModel, HomeViewModel>("home");
services.AddKeyedTransient<IPageViewModel, SearchViewModel>("search");

serviceProvider.GetRequiredService<IPageViewModel>();                    // throws - no unkeyed registration
serviceProvider.GetRequiredKeyedService<IPageViewModel>("home");         // HomeViewModel
serviceProvider.GetKeyedServices<IPageViewModel>(KeyedService.AnyKey);   // HomeViewModel, SearchViewModel
```

Any keyless call that relied on the `SimpleContainer` fallback needs to become an explicit keyed call, or the service needs an unkeyed registration alongside the keyed ones. To inject a keyed service by constructor, use `[FromKeyedServices]`, or provide the argument through a factory delegate:

```csharp
public HomeViewModel([FromKeyedServices("home")] IReport homeReport)
{
    // Use the report registered under the "home" key.
}
```

Where a key only selects between a fixed set of implementations at startup, the options pattern or a dedicated marker interface is often clearer than keyed services.

## Child containers become scopes

`CreateChildContainer()` maps to a scope, not to another `IServiceCollection`:

| SimpleContainer | Microsoft.Extensions.DependencyInjection |
|-----------------|-----------------------------------------|
| `container.CreateChildContainer()` | `serviceProvider.CreateScope()` or `CreateAsyncScope()` |
| `child.GetRequiredInstance<T>()` | `scope.ServiceProvider.GetRequiredService<T>()` |

```csharp
using var scope = serviceProvider.CreateScope();
var report = scope.ServiceProvider.GetRequiredService<IReport>();
```

Three differences matter:

- **A scope resolves scoped services, it does not accept new ones.** `AddScoped<T>` registrations live in the provider. `SimpleContainer` let a child register a new service locally; there is no equivalent, so such registrations have to move into the `ServiceCollection`.
- **A scoped service is one instance per scope**, disposed when the scope is disposed. `SimpleContainer` had no scoped lifetime and no disposal, so a `SimpleContainer` child shared the parent's singletons and created a fresh per-request instance on every resolution.
- **Resolving a scoped service from the root provider works by default**, which makes an accidental scope leak quiet. Enable `ValidateScopes` and `ValidateOnBuild` during development so the provider reports missing and mis-scoped registrations at startup:

```csharp
var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
{
    ValidateOnBuild = true,
    ValidateScopes = true
});
```

## Disposal

`SimpleContainer` never disposes anything it resolves. The `Microsoft.Extensions.DependencyInjection` provider disposes the singleton and scoped services it created, and leaves instances it did not construct alone.

```csharp
// The provider disposes this, because it created it.
services.AddSingleton<IMessageService, MessageService>();

// The provider leaves this alone, because you created it.
services.AddSingleton<ISettings>(settings);
```

Two consequences to plan for:

- **Services that were never disposed now are.** If a singleton implements `IDisposable` and holds a file handle, a socket, or an event subscription, it will be torn down when the provider is disposed. That is usually what you want; make sure the shutdown path actually disposes the provider.
- **Do not double-dispose.** Objects you own elsewhere in the application should not also be owned by the provider. Register an instance you created yourself rather than a container registration of the same object, so exactly one owner disposes it.

`IServiceScope` is `IDisposable`, and `IAsyncScope` from `CreateAsyncScope()` is `IAsyncDisposable`. Prefer `await using` when the scoped services themselves implement `IAsyncDisposable`.

## Constructor selection

`SimpleContainer` picks the public constructor with the most parameters and then resolves them, so a constructor with more parameters fails at resolution time if any dependency is missing. `Microsoft.Extensions.DependencyInjection` picks the constructor with the most parameters **whose dependencies it can all resolve**, and falls back to a shorter one when the greedy choice is unsatisfiable.

Two cases throw `InvalidOperationException` rather than picking something:

- No public constructor can be satisfied at all.
- Two or more constructors are satisfiable and none is a superset of the others — most often two constructors of the same arity taking different parameter types. `SimpleContainer` picked the first of the tied constructors that `Type.GetConstructors()` returned, which reflection does not order by declaration, so a type that happened to work can now fail to activate.

There is one case where the fallback does not save you, and it breaks in the opposite direction: a constructor whose extra parameters all have default values counts as satisfiable even when those types are not registered. `Microsoft.Extensions.DependencyInjection` takes that greedy constructor and passes the defaults, whereas `SimpleContainer` chose the same constructor and then called `GetRequiredInstance` on every parameter, so an unregistered optional dependency threw. A view model with an optional constructor dependency that activated before can now activate with that dependency silently left at its default — the failure moves from loud to quiet rather than the other way round.

Where a type has several viable constructors, the reliable fix is to remove the ambiguity rather than rely on either container's rule. A single public constructor is unambiguous:

```csharp
public class HomeViewModel
{
    public HomeViewModel(IMessageService messageService)
    {
        // The only constructor; the container has no choice to make.
    }
}
```

`[ActivatorUtilitiesConstructor]` does **not** help here. The built-in provider never reads that attribute — only `ActivatorUtilities.CreateInstance` does — so a marked constructor is ignored during normal resolution and the greedy rule applies anyway. If the extra dependency is genuinely test-only, register through a factory instead:

```csharp
services.AddTransient<HomeViewModel>(sp => new HomeViewModel(sp.GetRequiredService<IMessageService>()));
```

Alternatively, keep the two constructors and activate test instances through `ActivatorUtilities.CreateInstance<HomeViewModel>(sp)`, which honors the attribute.

## Func<T> factories

`SimpleContainer` resolves `Func<T>` out of the box as long as `T` is registered. `Microsoft.Extensions.DependencyInjection` does not — `Func<T>` is an ordinary type there, so it has to be registered like any other service. This matters wherever a view model was handed a factory instead of a dependency.

```csharp
services.AddTransient<Func<HomeViewModel>>(sp => () => sp.GetRequiredService<HomeViewModel>());
```

The gallery samples wrap exactly this in an `AddFunc<TService>()` helper; see `ServiceCollectionExtensions.cs` in the sample projects.

The delegate captures the provider it was created from, so match the lifetime to what it resolves. A singleton `Func<T>` over a scoped `T` resolves the scoped service from the root provider and throws `Cannot resolve scoped service ... from root provider` once `ValidateScopes` is on during development. Use `AddTransient` for the common case of a factory resolving a transient, and `AddScoped` for a factory resolving a scoped service.

## Injecting the provider

`SimpleContainer` registers itself, so view models could take a `SimpleContainer` parameter. The equivalent is `IServiceProvider`, which the container injects as well — but injecting it makes a component resolve its own dependencies at call time, which hides them from the constructor and defeats the point of constructor injection. Prefer injecting the service itself, or `IServiceScopeFactory` where a component genuinely needs to open a scope of its own.

## Before and after

A composition root that uses `SimpleContainer`:

```csharp
public class Bootstrapper
{
    private readonly SimpleContainer _container = new();

    public Bootstrapper()
    {
        _container.RegisterSingleton<IWindowManager, WindowManager>();
        _container.RegisterSingleton<IEventAggregator, EventAggregator>();
        _container.RegisterInstance<ISettings>(LoadSettings());
        _container.RegisterPerRequest<IPageViewModel, HomeViewModel>();
        _container.RegisterPerRequest<ShellViewModel>();
    }

    public void Start()
    {
        _container.GetRequiredInstance<IWindowManager>()
            .ShowWindow(_container.GetRequiredInstance<ShellViewModel>());
    }
}
```

The same bootstrapper on `Microsoft.Extensions.DependencyInjection`:

```csharp
public class Bootstrapper
{
    private readonly ServiceProvider _serviceProvider;

    public Bootstrapper()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IWindowManager, WindowManager>();
        services.AddSingleton<IEventAggregator, EventAggregator>();
        services.AddSingleton<ISettings>(LoadSettings());
        services.AddTransient<IPageViewModel, HomeViewModel>();
        services.AddTransient<ShellViewModel>();

        _serviceProvider = services.BuildServiceProvider();
    }

    public void Start()
    {
        _serviceProvider.GetRequiredService<IWindowManager>()
            .ShowWindow(_serviceProvider.GetRequiredService<ShellViewModel>());
    }

    public void Stop() => _serviceProvider.Dispose();
}
```

Neither composition root registers a `Func<T>`, because neither view model takes one. If yours did, that registration has to be carried over explicitly — `SimpleContainer` supplied it for free, MS DI does not.

The bootstrapper itself usually disappears: register the services from `App.OnStartup`, `App.OnLaunched`, or `App.OnFrameworkInitializationCompleted`, as shown in [NuGet Packages](nuget.md).

## Migration checklist

- [ ] Add `Microsoft.Extensions.DependencyInjection` (and `Microsoft.Extensions.Options`)
- [ ] Move every registration into a single `ServiceCollection` and build the provider once, at startup
- [ ] Replace `CreateChildContainer()` with `CreateScope()` and move child-local registrations into the `ServiceCollection`
- [ ] Convert `string` keys to `AddKeyed*` registrations, and replace every keyless call that relied on the fallback
- [ ] Find services registered more than once and split or remove them, since resolution no longer throws
- [ ] Check types with several public constructors; equally greedy ones now throw as ambiguous, and unregistered optional parameters are now filled with defaults instead of throwing
- [ ] Register every `Func<T>` explicitly, matching its lifetime to what it resolves
- [ ] Dispose the provider on shutdown, and check that no service is disposed twice
- [ ] Enable `ValidateOnBuild` and `ValidateScopes` during development

## Trimming and Native AOT

`SimpleContainer` creates types through reflection and is annotated as requiring dynamic code and unreferenced members, which makes it a poor fit for trimmed or Native AOT deployments. `Microsoft.Extensions.DependencyInjection` compiles registrations into a call-site graph and has no such requirement, so removing `SimpleContainer` also removes one of the last reasons your application could not be trimmed.
