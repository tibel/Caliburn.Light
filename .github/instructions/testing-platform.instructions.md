---
description: >-
  Platform-specific test executors, async UI event coordination, and
  platform-specific testing notes for WPF, Avalonia, and WinUI test projects.
applyTo:
  globs:
    - "tests/Caliburn.Light.WPF.Tests/**/*.cs"
    - "tests/Caliburn.Light.Avalonia.Tests/**/*.cs"
    - "tests/Caliburn.Light.WinUI.Tests/**/*.cs"
---

# Platform Test Executors

Each UI platform has a custom `ITestExecutor` — **every new UI test class must declare `[TestExecutor<T>]`** at class level, otherwise tests run off the UI thread and will fail:

- **WpfTestExecutor** — New STA thread with `Dispatcher.Run()` per test.
- **AvaloniaTestExecutor** — Singleton headless app, dispatches to `Dispatcher.UIThread`.
- **WinUITestExecutor** — Singleton app on STA thread, `DispatcherQueue.TryEnqueue()`. `TestApp` implements `IXamlMetadataProvider` for `Frame.Navigate()`.

Both Avalonia and WinUI executors use `volatile` on the `_initialized` field for correct double-checked locking, and suppress TUnit0031 for intentional `async void` lambdas in dispatcher callbacks — the completion is bridged via `TaskCompletionSource`. Do not remove either.

## Async UI event coordination

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

## Avalonia headless limitations

Avalonia headless mode does not provide `IPopupImpl`. Popup open/close lifecycle tests cannot run — only constructor/property tests are possible. Full Popup lifecycle behavior is covered in the WPF and WinUI test projects instead.

## WinUI testing specifics

- `FrameworkElement.XamlRoot` is null until `Loaded` fires — await it before using `ContentDialog`, `Popup`, or `Frame`.
- `ContentDialog.ShowAsync()` is modal — use fire-and-forget (`_ = dialog.ShowAsync()`) and coordinate via `Opened`/`Closed` event handlers.
- **Close-guard tests**: `window.Close()` bypasses `AppWindow.Closing` — use `SendMessage(hwnd, WM_CLOSE, 0, 0)` to trigger the full OS close path.
