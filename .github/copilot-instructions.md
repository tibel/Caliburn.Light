# Copilot Instructions for Caliburn.Light

Caliburn.Light is a magic-free MVVM framework supporting WPF, Avalonia, and WinUI. It uses explicit wiring (no automatic conventions), weak events to prevent memory leaks, and does not auto-switch to the UI thread.

## Build & Test

```powershell
# Build
dotnet build Caliburn.Light.slnx

# Run all tests (from solution root)
dotnet test

# Test each project individually (WinUI requires runtime identifier)
dotnet test --project tests/Caliburn.Light.Core.Tests
dotnet test --project tests/Caliburn.Light.WPF.Tests
dotnet test --project tests/Caliburn.Light.Avalonia.Tests
dotnet test --project tests/Caliburn.Light.WinUI.Tests -r win-x64

# Single test class (TUnit uses --treenode-filter, NOT --filter)
dotnet test --project tests/Caliburn.Light.Core.Tests -- --treenode-filter "/*/*/ScreenTests/*"

# Single test method
dotnet test --project tests/Caliburn.Light.Core.Tests -- --treenode-filter "/*/*/ScreenTests/ActivateAsync_SetsIsActive"

# With coverage
dotnet test --project tests/Caliburn.Light.Core.Tests --coverage --coverage-output-format cobertura
```

CI packs NuGet packages with `dotnet pack Caliburn.Light.slnx --configuration Release -p:ContinuousIntegrationBuild=True`. CI also runs Core and Avalonia tests on `ubuntu-latest`. Solution uses `.slnx` format. Central package management via `Directory.Packages.props`.

## Architecture

Four NuGet packages with a shared core:

- **Caliburn.Light.Core** — Platform-agnostic MVVM: `Screen`, `Conductor`, `EventAggregator`, `DelegateCommand`, validation, weak events. Targets `net8.0;net10.0`. AOT-compatible.
- **Caliburn.Light.WPF** — WPF integration: `WindowManager`, lifecycle classes, view location. Targets `net8.0-windows7.0;net10.0-windows7.0`.
- **Caliburn.Light.Avalonia** — Avalonia integration: same API surface as WPF. Targets `net8.0;net10.0`. AOT-compatible.
- **Caliburn.Light.WinUI** — WinUI integration: adds `ContentDialogLifecycle`, uses `AppWindow.Closing` for close guard support. Targets `net8.0-windows10.0.19041.0;net10.0-windows10.0.19041.0`. AOT-compatible.

Each platform project references Core and mirrors the same patterns: `WindowLifecycle`, `PopupLifecycle`, `WindowManager`, `ViewModelLocator`, `View`, `ViewAdapter`, `BindingHelper`.

## Key Conventions

### Lifecycle event cleanup

All events subscribed in lifecycle constructors **must** be unsubscribed when the lifecycle ends. In `WindowLifecycle`, `OnViewClosed` is terminal — unsubscribe everything (`Closed`, `Activated`, `Deactivated`, `Closing`) there. In `PopupLifecycle` and `ContentDialogLifecycle`, `Closed` is **not** terminal (controls can reopen), so events stay wired.

### Platform differences in WindowLifecycle

WinUI differs from WPF/Avalonia in three important ways:

1. **Close guard**: Uses `view.AppWindow.Closing` (not `Window.Closed`, which fires too late to cancel). `window.Close()` bypasses `AppWindow.Closing` — use `SendMessage(hwnd, WM_CLOSE, 0, 0)` to trigger the full OS close path.
2. **ViewModel access**: Via `view.Content.DataContext` (WinUI `Window` is not a `FrameworkElement`), whereas WPF/Avalonia use `view.DataContext` directly.
3. **Activation**: Single `Activated` handler checks `WindowActivationState` enum (CodeActivated/PointerActivated/Deactivated), whereas WPF/Avalonia use separate `Activated`/`Deactivated` events.

### Fire-and-forget async

Use `.Observe()` extension (from `TaskHelper.cs`) for fire-and-forget async calls:
```csharp
activatable.ActivateAsync().Observe();
```
This suppresses unobserved task warnings while still propagating exceptions.

### Weak events

External event subscriptions use `WeakEventHandler`/`WeakEventSource` to prevent memory leaks. This is a core design principle.

## Testing

Tests use **TUnit** (not xUnit/NUnit). Key differences from other frameworks:

- Assertions are async: `await Assert.That(value).IsEqualTo(expected)`
- Use `.IsTrue()`/`.IsFalse()` for booleans, not `.IsEqualTo(true)` (analyzer TUnitAssertions0015)
- Use `.IsNull()` for null checks, not `.IsEqualTo(null)` (analyzer TUnitAssertions0014)
- Test runner uses `Microsoft.Testing.Platform` (configured in `global.json`)

### Test conventions

- **Naming**: `MethodName_Condition_ExpectedResult` (e.g. `ActivateAsync_SetsIsActive`, `Constructor_NullConfig_Throws`)
- **Test doubles**: Hand-written stubs and fakes (e.g. `TestScreen : Screen`, `StubConductor : IConductor`, `SimpleServiceProvider : IServiceProvider`). No mocking library is used.
- **One concept per test**: Each test verifies a single expectation with clear Arrange/Act/Assert structure.
- **Test class separation**: Separate classes for distinct responsibilities. Example: `ViewModelLocatorTests` tests runtime locator behavior, `ViewModelLocatorConfigurationTests` tests the mapping configuration API.

### TUnit assertion pitfalls

- **`IsEquivalentTo` is order-insensitive** — do not use it to verify event ordering or sequence. Use explicit indexed assertions instead:
  ```csharp
  // WRONG: does not verify order
  await Assert.That(events).IsEquivalentTo(["PropertyChanging", "PropertyChanged"]);

  // CORRECT: verifies exact order
  await Assert.That(events).Count().IsEqualTo(2);
  await Assert.That(events[0]).IsEqualTo("PropertyChanging");
  await Assert.That(events[1]).IsEqualTo("PropertyChanged");
  ```
- **`IsEqualTo` fails on mismatched collection types** (e.g. `List<string>` vs `string[]`). Use explicit indexed assertions or ensure types match.

### Test anti-patterns

- **`Task.Delay` for synchronization** — Never use `await Task.Delay()` to wait for UI events. Use event-driven `TaskCompletionSource` with `.WaitAsync()` timeout instead.
- **`[NotInParallel]` without a key** — `[NotInParallel]` (no argument) only serializes tests sharing the same empty key. Always use a named key like `[NotInParallel("ViewHelper")]` so all classes touching that state are serialized together.
- **Testing concurrency on non-thread-safe types** — `Conductor<T>` and other MVVM types are not thread-safe. Don't write concurrent stress tests for them — test sequential behavior instead.
- **Test name doesn't match assertion** — If a test asserts that `screen.IsActive` is true, name it `_ActivatesViewModel`, not `_HooksClosingEvent`. The name must describe what is verified.
- **GC tests without a positive case** — When testing weak-reference cleanup, verify both that dead handlers are removed AND that live handlers still work after cleanup.
- **Duplicate tests across files** — Each test class owns a clear responsibility. Don't place the same behavioral test in two files (e.g. PopupLifecycle tests should only be in `PopupLifecycleTests.cs`, not also in `WindowLifecycleTests.cs`).

### Test patterns

- **Static helpers over base classes** — Use private static helper methods (e.g. `CreateDialogWithXamlRoot`, `OpenPopupAsync`) for shared test setup rather than inheritance hierarchies.
- **Tuple returns for fixtures needing cleanup** — When a helper creates multiple objects the test must dispose, return a tuple: `(ContentDialog dialog, Window window)`. This makes cleanup explicit and visible.
- **Cross-platform test alignment** — The same behavioral tests should exist across WPF, Avalonia, and WinUI. When a platform can't run a test (e.g. Avalonia Popup in headless mode), document the limitation and ensure coverage on the other platforms.
- **Verify both paths in edge-case tests** — GC tests, error-path tests, and cleanup tests should verify the positive case too. Example: after removing a dead weak-event handler, also verify that a live subscriber still receives events.

### Parallel execution and `[NotInParallel]`

Static state shared across test classes requires `[NotInParallel("key")]` at class level to prevent race conditions:

- **`"StaticExecutingEvent"`**: `AsyncDelegateCommandTests` and `EventAggregatorTests` — both test static events (`AsyncCommand.Executing`, `EventAggregator.Executing`)
- **`"ViewHelper"`**: WPF/Avalonia `ViewAdapterTests` and `ViewTests` — all mutate `ViewHelper` static state via `ViewHelper.Reset()`
- **`"ViewHelperTests"`**: Core `ViewHelperTests` and `ScreenTests` — same `ViewHelper` static state

Use `[Before(Test)]` / `[After(Test)]` for per-test setup/teardown (e.g. `ViewHelper.Reset()`).

### Platform test executors

Each UI platform has a custom `ITestExecutor` (from `TUnit.Core.Interfaces`) applied via `[TestExecutor<T>]` at class level:

- **WpfTestExecutor** — Creates a new STA thread with `Dispatcher.Run()` per test.
- **AvaloniaTestExecutor** — Boots a singleton headless Avalonia app once, dispatches to `Dispatcher.UIThread`.
- **WinUITestExecutor** — Boots a singleton WinUI app on a dedicated STA thread, dispatches via `DispatcherQueue.TryEnqueue()`. The `TestApp` implements `IXamlMetadataProvider` to enable `Frame.Navigate()` for page lifecycle tests.

Both Avalonia and WinUI executors use `volatile` on the `_initialized` field for correct double-checked locking, and suppress TUnit0031 for intentional `async void` lambdas in dispatcher callbacks — the completion is bridged via `TaskCompletionSource`.

### Async UI event coordination

For UI events that fire asynchronously (Opened, Closed, Loaded), use this pattern:
```csharp
var tcs = new TaskCompletionSource();
TypedEventHandler<ContentDialog, ContentDialogOpenedEventArgs> handler = null!;
handler = (_, _) =>
{
    dialog.Opened -= handler;  // one-shot: unsubscribe immediately
    tcs.TrySetResult();
};
dialog.Opened += handler;
_ = dialog.ShowAsync();  // fire-and-forget for modal dialogs
await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));  // always add timeout
```

Key rules:
- **One-shot handlers**: Unsubscribe inside the handler to avoid stacked handlers on controls that reopen.
- **5-second timeout**: Always use `.WaitAsync(TimeSpan.FromSeconds(5))` to prevent test hangs.
- **Inline cleanup**: Close windows/dialogs at the end of each test method (`window.Close()`, `dialog.Hide()`).

### Avalonia headless limitations

Avalonia headless mode does not provide `IPopupImpl`. Popup open/close lifecycle tests cannot run — only constructor/property tests are possible. Full Popup lifecycle behavior is covered in the WPF and WinUI test projects instead.

### WinUI testing specifics

- `FrameworkElement.XamlRoot` is null until `Loaded` fires — await it before using `ContentDialog`, `Popup`, or `Frame`.
- `ContentDialog.ShowAsync()` is modal — use fire-and-forget (`_ = dialog.ShowAsync()`) and coordinate via `Opened`/`Closed` event handlers.
- `Popup.Opened`/`Closed` events fire asynchronously — use one-shot handlers that unsubscribe themselves.
- WinUI tests require `-r win-x64` runtime identifier.
- `Conductor<T>` is **not thread-safe** — do not write concurrent tests for it; use sequential assertions.
