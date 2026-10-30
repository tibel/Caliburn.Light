# Migration Guide

This page covers the parts of Caliburn.Light that exist for migration from earlier versions: the deprecated meta-package and APIs that were removed along the way. Nothing on this page is required for new applications — see [Basic Configuration](configuration.md) and [NuGet Packages](nuget.md) instead.

## The Caliburn.Light meta-package

`Caliburn.Light` remains as a deprecated compatibility package that forwards to `Caliburn.Light.WPF` and `Caliburn.Light.Coroutines`. New applications should reference the package for their platform instead:

| Platform | Package |
|----------|---------|
| WPF | `Caliburn.Light.WPF` |
| WinUI 3 | `Caliburn.Light.WinUI` |
| Avalonia | `Caliburn.Light.Avalonia` |

Migrating away from the meta-package is a package reference change; the framework code itself needs no changes:

```diff
- <PackageReference Include="Caliburn.Light" Version="..." />
+ <PackageReference Include="Caliburn.Light.WPF" Version="..." />
```

The three platform packages share a common API surface, alongside platform-specific APIs and behavior. The meta-package forwards to `Caliburn.Light.WPF` **and** `Caliburn.Light.Coroutines`, so an application that used coroutines must add that reference explicitly:

```diff
  <PackageReference Include="Caliburn.Light.WPF" Version="..." />
+ <PackageReference Include="Caliburn.Light.Coroutines" Version="..." />
```

## Migrating removed and obsoleted APIs

The following APIs have been removed from the framework:

| Removed or obsoleted API | Since | Replacement |
|--------------------------|-------|-------------|
| `BootstrapperBase`, `CaliburnApplication` | 4.0.0 | Compose ViewModels directly and register framework services in your `IServiceProvider` |
| `NameBasedViewModelTypeResolver` | 4.0.0 | `ViewModelLocator` with explicit `ViewModelLocatorConfiguration` mappings |
| `UIContext` | 4.0.0 | View-based `ViewHelper.CheckAccess`, `BeginInvoke`, and `SwitchToViewAsync` |
| `ScreenHelper`, `PropertySupport` | 4.0.0 | `BindableObject` and `nameof()` |
| `IServiceLocator` | 4.0.0 | `IServiceProvider` |
| `IActivate`, `IDeactivate` | 4.0.0 | `IActivatable` |
| `IParent`, `IConductActiveItem`, `IScreen` | 5.0.0 | `IParentAware`/`ParentAware`, `IConductor`, `Screen` |
| `dialogResult` on `TryClose()` | 5.0.0 | Removed without replacement |
| In-framework coroutines | 5.0.0 | The `Caliburn.Light.Coroutines` package, documented in [Coroutines](coroutines.md) |
| Logging | 5.0.0 | Removed without replacement; use your preferred logging library |
| `ViewModelTypeResolver` | 6.0.0 | `IViewModelLocator`/`ViewModelLocator` |
| `SimpleContainer` | 6.0.0 | Any `IServiceProvider`; [Migrating from the legacy container](simple-container-migration.md) maps the API |
| `IViewAware.ViewAttached`, `ViewAware.Views` | 6.0.0 | `ViewAware.OnViewAttached(view, context)`, `IViewAware.GetViews()` |
| WinUI `SuspensionManager`, `NavigationService`, `FrameAdapter` | 6.0.0 | `PageLifecycle` wrapping a `Frame` |
| WPF `IWindowManager.ShowPopup(viewModel, context)` | 6.1.0 | No direct replacement; host a `Popup` yourself and use `PopupLifecycle` for activation |
| `IChild` | 8.0.0 | Inherit from `Screen` or `ParentAware`, or implement `IParentAware` |
| `ConductorBase<T>.EnsureItem` | 8.0.0 | Override item association in the concrete conductor |
| `IConductor.ActivationProcessed`, `ActivationProcessedEventArgs`, `ConductorBase<T>.OnActivationProcessed` | 8.0.0 | Observe `PropertyChanged(nameof(ActiveItem))` or `IActivatable.Activated` |
| `DispatcherHelper` | 8.0.0 | Use `ViewHelper.SwitchToViewAsync(view)` |
| `View.GetDispatcherFrom`, `IDispatcher`, `CurrentThreadDispatcher`, `ViewHelper.GetDispatcher` | 8.0.0 | Use view-based `ViewHelper.CheckAccess`, `BeginInvoke`, and `SwitchToViewAsync` |
| EventAggregator `dispatcher` subscription argument | 8.0.0 | Specify `ThreadOption.PublisherThread`, `UIThread`, or `BackgroundThread` |

`SimpleContainer` was removed in 6.0.0 alongside `ViewModelTypeResolver` and ships in no package, including the meta-package. [Migrating from the legacy container to Microsoft.Extensions.DependencyInjection](simple-container-migration.md) covers moving off it.

`EnsureItem` was reduced to a no-op before removal. Applications that relied on overriding it should move the logic into the concrete conductor, as described in [Screens, Conductors and Composition](composition.md).

### Coroutine API changes

Coroutines moved out of the framework in 5.0.0 and are back as the `Caliburn.Light.Coroutines` package. Calling them is unchanged, but three details differ:

- `ICoTask.BeginExecute` takes a `CommandExecutionContext` instead of a `CoroutineExecutionContext`, so custom `ICoTask` implementations no longer compile.
- The public `CoTask` base class is gone and `CoTaskDecorator` is now internal, so there is no longer a public base class to derive from. Implement `ICoTask` or `ICoTask<T>` directly.
- `OverrideCancel<TResult>()` no longer defaults its result argument, so `coTask.OverrideCancel<string>()` must become `coTask.OverrideCancel(default(string))`. The non-generic `OverrideCancel()` is unchanged.

### UI dispatching migration

The framework no longer has `UIContext`, `IDispatcher`, or `CurrentThreadDispatcher`. For a known view, use `ViewHelper` directly:
- **Check access:** `ViewHelper.CheckAccess(view)`.
- **Fire-and-forget UI work:** `ViewHelper.BeginInvoke(view, action)`. Exceptions from the action are raised on the UI thread.
- **Continue on the UI thread (async continuation):** `await ViewHelper.SwitchToViewAsync(view)`.
- **Await a callback that returns a value:** switch to the UI thread, then execute the callback:
  ```csharp
  static async Task<T> RunOnUiThreadAsync<T>(object view, Func<T> func)
  {
      await ViewHelper.SwitchToViewAsync(view);
      return func();
  }
  ```
- If code requires a platform dispatcher when no view is available, use an application-owned or platform API; Caliburn.Light no longer exposes a dispatcher abstraction.
- For EventAggregator subscriptions, choose `ThreadOption` instead of supplying a dispatcher. `UIThread` locates a view through the target, `IViewAware`, or `IParentAware` ancestry; with no view it runs on the publishing thread.
- `BackgroundThread` runs inline when publishing is already on a thread-pool thread; otherwise the EventAggregator queues a thread-pool continuation.
- **Migrating from `UIContext.Run`:** There is no direct 1:1 mapping for every overload. Use `BeginInvoke` for fire-and-forget, or switch to a view's UI thread when a result, exception propagation, or cancellation is needed.

### Migration checklist (to the latest version)

Use the items relevant to your starting version and the APIs your application uses. The checklist highlights what is typically mechanical versus what requires behavior decisions or remains application-owned.

- [ ] **Package split** (mechanical) — Update package references. Use `Caliburn.Light.Core` plus the appropriate platform package (`Caliburn.Light.WPF`, `Caliburn.Light.WinUI`, or `Caliburn.Light.Avalonia`). Add `Caliburn.Light.Coroutines` only if you use the coroutine APIs.
- [ ] **Startup composition and view-model mappings** (decision) — Replace any removed bootstrapper model with explicit app startup. Verify view location/view-model mapping is configured through explicit `ViewModelLocator` mappings; no legacy bootstrapper types remain.
- [ ] **SimpleContainer and service-locator migration** (decision/app-owned) — `SimpleContainer` was removed. Port registrations and lifetimes to your chosen DI container. If you previously relied on service-locator patterns, refactor to constructor injection where feasible.
- [ ] **UI dispatching, lifecycle, and view-awareness** (mix) — Migrate from `UIContext`/`IDispatcher` to view-based `ViewHelper` operations (see [UI dispatching migration](#ui-dispatching-migration)). Update lifecycle/view-awareness usage to current platform APIs. Behavior around activation, close guards, and navigation differs by platform (see platform-specific docs).
- [ ] **Coroutines** (mix) — Replace `Coroutine.FromTask(...)`/`AsCoTask(...)` legacy patterns with `Coroutine.From(...)` overloads where applicable. If you have custom co-tasks derived from removed base classes, implement `ICoTask` or `ICoTask<T>` directly (see [Coroutines](coroutines.md)). Ensure completion is raised exactly once with correct cancellation/error precedence.
- [ ] **File dialogs responsibilities** (app/platform-owned) — Replace any old coroutine-based file dialog helpers with Task-based `IWindowManager` methods on your platform (see [File dialogs migration](#file-dialogs-migration) and [Window Manager](window-manager.md)). File dialogs are platform-specific, not part of Core.
- [ ] **Configuration and assembly-name checks** (mechanical) — Update namespaces, using directives, and any assembly references to match current package names. Verify target frameworks align with your app (`net10.0` / platform-specific TFMs). Remove references to deprecated types (e.g. removed bootstrapper/container/dispatcher types).

> **Notes:**
> - **Mechanical** items are typically rename/reference updates.
> - **Decision** items require choosing how to structure startup/DI/lifetimes.
> - **App-owned** behavior (e.g. container lifetimes, view mapping, dialog ownership) is intentionally not auto-upgraded; test these areas explicitly after migration.

### File dialogs migration

Older versions of Caliburn.Light provided coroutine-based file dialog helpers. In current versions, file dialogs are exposed as Task-based methods on `IWindowManager` (platform-specific implementations), not as coroutines in the core framework.

- **WPF**: Replace any old open/save file dialog coroutines with `IWindowManager.ShowOpenFileDialog(OpenFileDialogOptions, ownerViewModel)`, `ShowSaveFileDialog(SaveFileDialogOptions, ownerViewModel)`, and `ShowOpenFolderDialog(OpenFolderDialogOptions, ownerViewModel)`. All return `Task` results (not `ICoTask`).
- **WinUI**: Use `ShowFileOpenPickerAsync(FileOpenPickerOptions, ownerViewModel)`, `ShowFileSavePickerAsync(FileSavePickerOptions, ownerViewModel)`, and `ShowFolderPickerAsync(FolderPickerOptions, ownerViewModel)`.
- **Avalonia**: Use `ShowOpenFilePickerAsync(FilePickerOpenOptions, ownerViewModel)`, `ShowSaveFilePickerAsync(FilePickerSaveOptions, ownerViewModel)`, and `ShowOpenFolderPickerAsync(FolderPickerOpenOptions, ownerViewModel)`.

Configure dialogs via the platform-specific options classes. File dialog APIs remain platform-specific (not in Core), aligning with UI platform capabilities. See [Window Manager](window-manager.md) for examples and API details.
