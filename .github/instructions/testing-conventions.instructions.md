---
description: >-
  Testing conventions, TUnit framework usage, assertion pitfalls, anti-patterns,
  and test patterns for Caliburn.Light test projects.
applyTo:
  globs:
    - "tests/**/*.cs"
---

# Testing

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
- **`[NotInParallel]` without a key** — always use named key like `[NotInParallel("ViewHelper")]`. Keyless only serializes with other keyless tests.
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

Static state shared across test classes requires `[NotInParallel("key")]` at class level to prevent race conditions. Named keys in use:

- **`"StaticExecutingEvent"`** — test classes touching static `Executing` events on `AsyncCommand` / `EventAggregator`
- **`"ViewHelper"`** — `ViewAdapterTests` (WPF, Avalonia, WinUI) and `ViewTests` (WPF, Avalonia): `ViewAdapterTests` mutates `ViewHelper` via `ViewHelper.Reset()`; `ViewTests` reads the same static state and must be serialized to prevent racing with the reset
- **`"ViewHelperTests"`** — Core test classes that mutate the same `ViewHelper` static state

Use `[Before(Test)]` / `[After(Test)]` for per-test setup/teardown (e.g. `ViewHelper.Reset()`).
