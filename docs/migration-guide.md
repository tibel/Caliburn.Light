# Migration Guide

This page covers the parts of Caliburn.Light that exist for migration from earlier versions: the deprecated meta-package and the APIs that were removed or obsoleted along the way. Nothing on this page is required for new applications — see [Basic Configuration](configuration.md) and [NuGet Packages](nuget.md) instead.

The [SimpleContainer](simple-container.md) reference lives on its own page, since the type itself only ships in the deprecated meta-package.

## The Caliburn.Light meta-package

`Caliburn.Light` is a deprecated compatibility package that forwards to `Caliburn.Light.WPF` and `Caliburn.Light.Coroutines`. New applications should reference the package for their platform instead:

| Platform | Package |
|----------|---------|
| WPF | `Caliburn.Light.WPF` |
| WinUI 3 | `Caliburn.Light.WinUI` |
| Avalonia | `Caliburn.Light.Avalonia` |

Migrating away from the meta-package is a package reference change; no code changes are required:

```diff
- <PackageReference Include="Caliburn.Light" Version="..." />
+ <PackageReference Include="Caliburn.Light.WPF" Version="..." />
```

The platform packages ship the same API surface as the meta-package, with one exception: [SimpleContainer](simple-container.md) only exists in the meta-package.

## Migrating removed and obsoleted APIs

The following APIs are no longer part of the framework. Members marked as obsoleted in the current release fail to compile, so they must be migrated rather than suppressed.

| Removed or obsoleted API | Since | Replacement |
|--------------------------|-------|-------------|
| `BootstrapperBase`, `CaliburnApplication` | 4.0.0 | Compose ViewModels directly and register framework services in your `IServiceProvider` |
| `NameBasedViewModelTypeResolver` | 4.0.0 | `ViewModelLocator` with explicit `ViewModelLocatorConfiguration` mappings |
| `UIContext` | 4.0.0 | `IDispatcher` |
| `ScreenHelper`, `PropertySupport` | 4.0.0 | `BindableObject` and `nameof()` |
| `IServiceLocator` | 4.0.0 | `IServiceProvider` |
| `IActivate`, `IDeactivate` | 4.0.0 | `IActivable` |
| `IParent`, `IConductActiveItem`, `IScreen` | 5.0.0 | `IParentAware`/`ParentAware`, `IConductor`, `Screen` |
| `dialogResult` on `TryClose()` | 5.0.0 | `CloseResult<T>` |
| In-framework coroutines | 5.0.0 | The `Caliburn.Light.Coroutines` package, documented in [Coroutines](coroutines.md) |
| Logging | 5.0.0 | Removed without replacement; use your preferred logging library |
| `ViewModelTypeResolver` | 6.0.0 | `IViewModelLocator`/`ViewModelLocator` |
| `IViewAware.ViewAttached`, `ViewAware.Views` | 6.0.0 | `ViewAware.OnViewAttached(view, context)`, `IViewAware.GetViews()` |
| WinUI `SuspensionManager`, `NavigationService`, `FrameAdapter` | 6.0.0 | `PageLifecycle` wrapping a `Frame` |
| WPF `IWindowManager.ShowPopup(viewModel, context)` | 6.1.0 | No direct replacement; host a `Popup` yourself and use `PopupLifecycle` for activation |
| `IChild` (obsoleted) | current release | Inherit from `Screen` or `ParentAware`, or implement `IParentAware` |
| `ConductorBase<T>.EnsureItem` (obsoleted) | current release | Override item association in the concrete conductor |
| `ConductorBaseWithActiveItem<T>.ChangeActiveItemAsync` (obsoleted) | current release | Override active-item transitions in the concrete conductor |

`SimpleContainer` was removed in 6.0.0 alongside `ViewModelTypeResolver` and restored to the meta-package afterwards. It is documented in [SimpleContainer](simple-container.md) for applications that still depend on it.

`EnsureItem` and `ChangeActiveItemAsync` were reduced to no-ops before being obsoleted. Applications that relied on overriding them should move the logic into the concrete conductor, as described in [Screens, Conductors and Composition](composition.md).

### Coroutine API changes

Coroutines moved out of the framework in 5.0.0 and are back as the `Caliburn.Light.Coroutines` package. Calling them is unchanged, but three details differ:

- `ICoTask.BeginExecute` takes a `CommandExecutionContext` instead of a `CoroutineExecutionContext`, so custom `ICoTask` implementations no longer compile.
- The public `CoTask` base class is gone and `CoTaskDecorator` is now internal, so there is no longer a public base class to derive from. Implement `ICoTask` or `ICoTask<T>` directly.
- `OverrideCancel<TResult>()` no longer defaults its result argument, so `coTask.OverrideCancel<string>()` must become `coTask.OverrideCancel(default(string))`. The non-generic `OverrideCancel()` is unchanged.
