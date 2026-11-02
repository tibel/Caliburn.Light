# Testing Guidelines

Tests use **TUnit** with `Microsoft.Testing.Platform` (configured in `global.json`). Use async assertions, including `.IsTrue()`/`.IsFalse()` for booleans and `.IsNull()` for null checks.

## Test conventions

- Name tests `MethodName_Condition_ExpectedResult` and make the name match the assertion.
- Prefer one behavior per test, with clear Arrange/Act/Assert sections.
- Use hand-written stubs and fakes; this repository does not use a mocking library.
- Keep test classes focused on one responsibility.
- `IsEquivalentTo` ignores collection order. Use indexed assertions when order matters, and avoid comparing different collection types with `IsEqualTo`.
- Test sequential behavior for framework types such as `Conductor<T>`; they are not thread-safe.
- For cleanup and GC tests, verify both that cleanup occurred and that live objects still work.
- Shared UI setup should use helper methods rather than test base classes. Return tuples when a helper creates multiple disposable objects.
- Keep shared behavioral coverage aligned across WPF, Avalonia, and WinUI where the platforms support it. Record platform limitations and test platform-specific features on their owning platform.

## Async and shared state

- Do not use `Task.Delay` to wait for UI events. Subscribe before triggering the event and await a `TaskCompletionSource` with a timeout; remove one-shot handlers when they fire.
- Close windows and dialogs at the end of each test to prevent state leaking between tests.
- Use `[NotInParallel("SharedStaticEventAndViewHelper")]` for Core's `ViewHelperTests` and `EventAggregatorTests`; both reset the shared `ViewHelper` state. Use literal keys consistently for any other shared state that needs serialization. Avoid keyless `[NotInParallel]` unless the entire test process must be isolated.
- Reset shared state only when a test class actually depends on it.
- Do not make tests depend on platform `[ModuleInitializer]` side effects; module loading order is not guaranteed by the test host. Test the component directly where possible.

## UI test executors

Each UI platform has a custom executor. Add the matching `[TestExecutor<T>]` at class level to any test class that creates or accesses UI elements. Pure logic tests do not need one.

- WPF: `WpfTestExecutor`
- Avalonia: `AvaloniaTestExecutor`
- WinUI: `WinUITestExecutor`

The executors intentionally use `volatile` initialization flags and suppress TUnit0031 for dispatcher callbacks whose completion is bridged with `TaskCompletionSource`; preserve these safeguards when changing executor code.
