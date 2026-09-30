# Testing Guidelines

Tests use **TUnit** (not xUnit/NUnit). Key differences from other frameworks:

- Assertions are async: `await Assert.That(value).IsEqualTo(expected)`
- Use `.IsTrue()`/`.IsFalse()` for booleans, not `.IsEqualTo(true)` (analyzer TUnitAssertions0015)
- Use `.IsNull()` for null checks, not `.IsEqualTo(null)` (analyzer TUnitAssertions0014)
- Test runner uses `Microsoft.Testing.Platform` (configured in `global.json`)

## Test conventions

- **Naming**: `MethodName_Condition_ExpectedResult` (e.g. `ActivateAsync_SetsIsActive`, `Constructor_NullConfig_Throws`)
- **Test doubles**: Hand-written stubs and fakes (e.g. `TestScreen : Screen`, `StubConductor : IConductor`, `SimpleServiceProvider : IServiceProvider`). No mocking library is used.
- **One concept per test**: Each test verifies a single expectation with clear Arrange/Act/Assert structure.
- **Test class separation**: Separate classes for distinct responsibilities. Example: `ViewModelLocatorTests` tests runtime locator behavior, `ViewModelLocatorConfigurationTests` tests the mapping configuration API.

## TUnit assertion pitfalls

- **`IsEquivalentTo` is order-insensitive** — do not use it to verify event ordering. Use indexed assertions instead:
  ```csharp
  // WRONG: does not verify order
  await Assert.That(events).IsEquivalentTo(["PropertyChanging", "PropertyChanged"]);

  // CORRECT: verifies exact order
  await Assert.That(events).Count().IsEqualTo(2);
  await Assert.That(events[0]).IsEqualTo("PropertyChanging");
  await Assert.That(events[1]).IsEqualTo("PropertyChanged");
  ```
- **`IsEqualTo` fails on mismatched collection types** (e.g. `List<string>` vs `string[]`). Use indexed assertions or ensure types match.

## Test anti-patterns

- **`Task.Delay` for synchronization** — Never use `await Task.Delay()` to wait for UI events. Use event-driven `TaskCompletionSource` with `.WaitAsync()` timeout instead.
- **Relying on `[ModuleInitializer]` side effects** — module initializers are lazy: they run on first load of the platform module, and the TUnit host does not guarantee that load has happened before a test body. Do not write a test whose setup depends on registration having occurred, and do not "fix" that with a `[Before(Test)]` guard that silently re-registers, which makes the test pass even if the production `[ModuleInitializer]` is deleted. Test the component directly instead, via `InternalsVisibleTo`.
- **Keyless `[NotInParallel]`** — a keyless constraint makes the test run completely alone, so it cannot overlap anything, including classes that share no key. A **class-level** keyed `[NotInParallel("key")]` is different: it serializes that class's own test methods and also holds it apart from other classes carrying the same key, but it does nothing against classes that do not carry the key. Prefer a class-level key when several classes must stay in step, and keyless on a single test when one test alone must be isolated. Per the TUnit docs, keyless is the most restrictive option, so reach for it only when a key is genuinely insufficient.
- **Testing concurrency on non-thread-safe types** — `Conductor<T>` and MVVM types are not thread-safe. Test sequential behavior, not concurrency.
- **Test name doesn't match assertion** — Name must describe what is verified, not what is set up.
- **GC tests without a positive case** — Verify both dead handlers are removed AND live handlers still work. Applies to all edge-case/cleanup tests.
- **Duplicate tests across files** — Each test class owns a clear responsibility. Don't place the same behavioral test in two files (e.g. PopupLifecycle tests should only be in `PopupLifecycleTests.cs`, not also in `WindowLifecycleTests.cs`).

## Test patterns

- **Static helpers over base classes** — Use private static helper methods (e.g. `CreateDialogWithXamlRoot`, `OpenPopupAsync`) for shared test setup rather than inheritance hierarchies.
- **Tuple returns for fixtures needing cleanup** — When a helper creates multiple objects the test must dispose, return a tuple: `(ContentDialog dialog, Window window)`. This makes cleanup explicit and visible.
- **Cross-platform test alignment** — The same behavioral tests should exist across WPF, Avalonia, and WinUI for shared features. When a platform can't run a test (e.g. Avalonia Popup in headless mode), document the limitation and ensure coverage on the other platforms. Platform-specific features (e.g. `ContentDialogLifecycle`) only need tests on the platform that owns them.
- **Verify both paths in edge-case tests** — For cleanup, error-path, and GC tests, verify both the failure/cleanup path and the normal/live path.

## Parallel execution and `[NotInParallel]`

Static state shared across test classes requires `[NotInParallel("key")]` at class level to prevent race conditions. Always write the key as a **string literal, never `nameof(...)`**: a key is a coordination token shared by every class that touches the same state, and `nameof` binds it to one class's name. `"StaticExecutingEvent"` spans three classes (`AsyncDelegateCommandTests`, `EventAggregatorTests`, `WeakStaticEventHandlerTests`), so `nameof` there would have produced three different keys and silently disabled the isolation. Named keys in use:

- **`"StaticExecutingEvent"`** — test classes touching static `Executing` events on `AsyncCommand` / `EventAggregator`
- **`"ViewHelperTests"`** — the Core test class that mutates the `ViewHelper` static state

The platform `ViewAdapterTests` (WPF, Avalonia, WinUI) construct their own `ViewAdapter` instance per test and call it directly, so they hold no test-local state and need no `[NotInParallel]` at all. No test in those assemblies calls `ViewHelper.Reset()` or otherwise mutates the registry, so no isolation is required. Note that `new ViewAdapter()` does indirectly populate the registry once, because it is what triggers the platform module's `[ModuleInitializer]`.

There is deliberately **no** test asserting that the platform `[ModuleInitializer]` registered the adapter. In this test host a test that touches no platform type runs before that module is loaded, so the registration has not happened yet; asserting on it is order-dependent. Test the adapter as a unit instead, and leave the start-up wiring uncovered by choice rather than with a test that cannot be trusted.

`ViewHelper`'s own registry semantics — `Initialize`, `Reset`, adapter resolution and the no-adapter-throws paths — are covered once in Core's `ViewHelperTests` using a `TestViewAdapter`. Do not re-test them per platform. Two caveats: duplicate suppression is **exercised but not asserted** — `Initialize_SameAdapterTwice_DoesNotDuplicate` can only check `IsInitialized`, which is true whether or not the adapter was added twice, and `ViewHelper` exposes no count or collection member, so duplication is unobservable through the public surface. The no-adapter-throws path is tested for `GetFirstNonGeneratedView`, `TryCloseAsync`, `GetCommandParameter` and `GetDispatcher`, but not for `ExecuteOnFirstLoad` or `ExecuteOnLayoutUpdated`. Finally, `ViewHelper` stores adapters in a plain `List<IViewAdapter>`, so concurrent `Initialize` (which `Add`s) racing reads (which use `Exists`/`Find`) is not thread-safe.

`ViewHelperTests` in Core keeps `[NotInParallel("ViewHelperTests")]` because its `[Before(Test)]`/`[After(Test)]` hooks call `ViewHelper.Reset()`; the class-level key serializes the class's own methods so a reset cannot be observed by a sibling. No other class needs that key.

`ScreenTests` previously carried the same key and reset `ViewHelper` before and after all of its tests, but none of them reach `ViewHelper`: `Screen.TryCloseAsync` only consults it for a parentless screen that has registered views, and those tests register none. The hooks were dead boilerplate from the first test commit and have been removed. Do not re-add a `ViewHelper` reset to a class that does not touch `ViewHelper` — it mutates global state, silently couples the class to the `ViewHelperTests` key, and buys nothing.

Use `[Before(Test)]` / `[After(Test)]` for per-test setup/teardown. Only reset shared state the class's tests actually depend on — a reset hook on a class that never reads the state is a global side effect with no benefit (see the `ScreenTests` note above).

## Platform Test Executors

Each UI platform has a custom `ITestExecutor` — **any test class that creates or touches UI elements must declare `[TestExecutor<T>]`** at class level, otherwise tests run off the UI thread and will fail. Pure-logic classes need no executor; `BooleanToVisibilityConverterTests` and `PickerOptionsTests` in the WinUI project are the current examples, as neither instantiates a control. There is no assembly-level default.

- **WpfTestExecutor** — New STA thread with `Dispatcher.Run()` per test.
- **AvaloniaTestExecutor** — Singleton headless app, dispatches to `Dispatcher.UIThread`.
- **WinUITestExecutor** — Singleton app on STA thread, `DispatcherQueue.TryEnqueue()`. `TestApp` implements `IXamlMetadataProvider` for `Frame.Navigate()`.

Both Avalonia and WinUI executors use `volatile` on the `_initialized` field for correct double-checked locking, and suppress TUnit0031 for intentional `async void` lambdas in dispatcher callbacks — the completion is bridged via `TaskCompletionSource`. Do not remove either.

### Async UI event coordination

For UI events that fire asynchronously (Opened, Closed, Loaded), use this pattern — adapt the handler delegate type to the actual event signature (`RoutedEventHandler`, `TypedEventHandler<T,TArgs>`, etc.):
```csharp
var tcs = new TaskCompletionSource();
RoutedEventHandler handler = null!;
handler = (_, _) =>
{
    element.Loaded -= handler;  // one-shot: unsubscribe immediately
    tcs.TrySetResult();
};
element.Loaded += handler;
// trigger the UI action that causes the event before awaiting
await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));  // always add timeout
```

Close windows/dialogs at the end of each test method (`window.Close()`, `dialog.Hide()`) to prevent cross-test state pollution.
