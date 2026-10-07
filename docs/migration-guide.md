# Migration Guide

This page covers the parts of Caliburn.Light that exist for migration from earlier versions: the deprecated meta-package and the APIs that were removed or obsoleted along the way. Nothing on this page is required for new applications — see [Basic Configuration](configuration.md) and [NuGet Packages](nuget.md) instead.

## The Caliburn.Light meta-package

`Caliburn.Light` is a deprecated compatibility package that forwards to `Caliburn.Light.WPF` and `Caliburn.Light.Coroutines`. New applications should reference the package for their platform instead:

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

The following APIs are no longer part of the framework, or are obsoleted. Members obsoleted in the current release still compile (with a warning) and remain binary compatible, but should be migrated since they may be removed in a later release.

| Removed or obsoleted API | Since | Replacement |
|--------------------------|-------|-------------|
| `BootstrapperBase`, `CaliburnApplication` | 4.0.0 | Compose ViewModels directly and register framework services in your `IServiceProvider` |
| `NameBasedViewModelTypeResolver` | 4.0.0 | `ViewModelLocator` with explicit `ViewModelLocatorConfiguration` mappings |
| `UIContext` | 4.0.0 | `IDispatcher` |
| `ScreenHelper`, `PropertySupport` | 4.0.0 | `BindableObject` and `nameof()` |
| `IServiceLocator` | 4.0.0 | `IServiceProvider` |
| `IActivate`, `IDeactivate` | 4.0.0 | `IActivable` |
| `IParent`, `IConductActiveItem`, `IScreen` | 5.0.0 | `IParentAware`/`ParentAware`, `IConductor`, `Screen` |
| `dialogResult` on `TryClose()` | 5.0.0 | Removed without replacement |
| In-framework coroutines | 5.0.0 | The `Caliburn.Light.Coroutines` package, documented in [Coroutines](coroutines.md) |
| Logging | 5.0.0 | Removed without replacement; use your preferred logging library |
| `ViewModelTypeResolver` | 6.0.0 | `IViewModelLocator`/`ViewModelLocator` |
| `SimpleContainer` | 6.0.0 | Any `IServiceProvider`; [Migrating from the legacy container](simple-container-migration.md) maps the API |
| `IViewAware.ViewAttached`, `ViewAware.Views` | 6.0.0 | `ViewAware.OnViewAttached(view, context)`, `IViewAware.GetViews()` |
| WinUI `SuspensionManager`, `NavigationService`, `FrameAdapter` | 6.0.0 | `PageLifecycle` wrapping a `Frame` |
| WPF `IWindowManager.ShowPopup(viewModel, context)` | 6.1.0 | No direct replacement; host a `Popup` yourself and use `PopupLifecycle` for activation |
| `IChild` (obsoleted) | current release | Inherit from `Screen` or `ParentAware`, or implement `IParentAware` |
| `ConductorBase<T>.EnsureItem` (obsoleted) | current release | Override item association in the concrete conductor |
| `ConductorBaseWithActiveItem<T>.ChangeActiveItemAsync` (obsoleted) | current release | Override active-item transitions in the concrete conductor |
| `IConductor.ActivationProcessed`, `ActivationProcessedEventArgs`, `ConductorBase<T>.OnActivationProcessed` (obsoleted, warning only) | current release | No longer raised by the framework; the conductor no longer calls `OnActivationProcessed`, so the event does not fire during normal use. Observe `PropertyChanged(nameof(ActiveItem))` or `IActivatable.Activated`; `Conductor<T>` subclasses can override `OnActivationVetoed` to observe vetoed activations, or raise your own event from your `ICloseGuard` |

`SimpleContainer` was removed in 6.0.0 alongside `ViewModelTypeResolver` and ships in no package, including the meta-package. [Migrating from the legacy container to Microsoft.Extensions.DependencyInjection](simple-container-migration.md) covers moving off it.

`EnsureItem` was reduced to a no-op before being obsoleted, and the conductor no longer calls `OnActivationProcessed`, so `ActivationProcessed` does not fire during normal use (subclasses calling `OnActivationProcessed` directly still raise the event). Applications that relied on overriding the conductor methods should move the logic into the concrete conductor, as described in [Screens, Conductors and Composition](composition.md).

### Coroutine API changes

Coroutines moved out of the framework in 5.0.0 and are back as the `Caliburn.Light.Coroutines` package. Calling them is unchanged, but three details differ:

- `ICoTask.BeginExecute` takes a `CommandExecutionContext` instead of a `CoroutineExecutionContext`, so custom `ICoTask` implementations no longer compile.
- The public `CoTask` base class is gone and `CoTaskDecorator` is now internal, so there is no longer a public base class to derive from. Implement `ICoTask` or `ICoTask<T>` directly.
- `OverrideCancel<TResult>()` no longer defaults its result argument, so `coTask.OverrideCancel<string>()` must become `coTask.OverrideCancel(default(string))`. The non-generic `OverrideCancel()` is unchanged.

### UI dispatching migration (IDispatcher)

The framework no longer has `UIContext`. In modern Caliburn.Light, UI thread marshaling is handled by `IDispatcher`, which is defined in Caliburn.Light.Core and implemented by the platform-specific dispatchers.

To obtain an `IDispatcher`:
- When a view is attached, use `Caliburn.Light.ViewHelper.GetDispatcher(view)` (or platform-specific `View.GetDispatcherFrom` helpers). See [UI Thread Dispatching](dispatching.md) for more details.
- If no view is attached, do not use `Caliburn.Light.CurrentThreadDispatcher.Instance` as a fallback for UI thread marshaling—it always executes work on the current thread with no marshaling, and its `SwitchTo()`/`BeginInvoke` complete inline. Instead, use a dispatcher from your application shell/view, or resolve `IDispatcher` from the platform context your app already has (e.g. the main window/view's dispatcher). See [UI Thread Dispatching](dispatching.md) for guidance.

Notes about behavior:
- `IDispatcher.BeginInvoke(Action)` returns `void` (unlike `UIContext.Run`, which returned a `Task`). Exceptions thrown by the dispatched action surface on the dispatcher thread unless you explicitly await the work.
- Behavior differs by implementation: platform dispatchers (WPF, Avalonia, WinUI) always enqueue work; `CurrentThreadDispatcher` always executes work inline on the calling thread with no marshaling. WinUI enqueues can also be rejected (e.g. during shutdown), in which case the work may not execute. Always consider using `dispatcher.CheckAccess()` when appropriate.
- `UIContext.VerifyAccess()` maps to `dispatcher.CheckAccess()` (but `VerifyAccess()` threw if not on the UI thread, whereas `CheckAccess()` only returns a boolean; if you need the throwing behavior, check the result and throw explicitly).

Replacement patterns:
- **Continue on the UI thread (async continuation):** `await dispatcher.SwitchTo();` (or your platform's equivalent).
- **Fire-and-forget UI work:** `dispatcher.BeginInvoke(action);`. Be aware that exceptions from the action will not be propagated to the caller.
- **Await a dispatched callback that returns a value:** In an async method, switch to the UI thread and then compute the result:
  ```csharp
  static async Task<T> RunOnUiThreadAsync<T>(IDispatcher dispatcher, Func<T> func)
  {
      await dispatcher.SwitchTo();
      return func();
  }
  ```
  This propagates exceptions and results to the caller. If `func` is synchronous, you can also return it directly after switching. Use `ConfigureAwait(false)` when the continuation doesn't need to be on the UI thread.
- **Migrating from `UIContext.Run`:** There is no direct 1:1 mapping for all `UIContext.Run` overloads. For fire-and-forget cases, use `BeginInvoke`. For cases where you need the result or exception propagation, switch to the UI thread in an async method and execute the work. The replacement depends on whether you need fire-and-forget execution, return a value, or propagate exceptions/cancellation.

Additional notes:
- `UIContext.TaskScheduler` and `IUIContext` have no direct equivalent on `IDispatcher`. If your code relied on them, consider using the platform's dispatcher TaskScheduler or an app-owned abstraction tailored to your needs.

### Migration checklist (to the latest version)

Use the items relevant to your starting version and the APIs your application uses. The checklist highlights what is typically mechanical versus what requires behavior decisions or remains application-owned.

- [ ] **Package split** (mechanical) — Update package references. Use `Caliburn.Light.Core` plus the appropriate platform package (`Caliburn.Light.WPF`, `Caliburn.Light.WinUI`, or `Caliburn.Light.Avalonia`). Add `Caliburn.Light.Coroutines` only if you use the coroutine APIs.
- [ ] **Startup composition and view-model mappings** (decision) — Replace any removed bootstrapper model with explicit app startup. Verify view location/view-model mapping is configured via `ViewModelLocator` and platform conventions; no legacy bootstrapper types remain.
- [ ] **SimpleContainer and service-locator migration** (decision/app-owned) — `SimpleContainer` was removed. Port registrations and lifetimes to your chosen DI container. If you previously relied on service-locator patterns, refactor to constructor injection where feasible.
- [ ] **Dispatcher, lifecycle, and view-awareness** (mix) — Migrate from `UIContext` to `IDispatcher` patterns (see [UI dispatching migration](#ui-dispatching-migration-idispatcher)). Update lifecycle/view-awareness usage to current platform APIs. Behavior around activation, close guards, and navigation differs by platform (see platform-specific docs).
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
