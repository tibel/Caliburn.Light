# WinUI Test Guidelines

## WinUI Testing Specifics

- `FrameworkElement.XamlRoot` is null until `Loaded` fires — await it before using `ContentDialog`, `Popup`, or `Frame`.
- `ContentDialog.ShowAsync()` is modal — use fire-and-forget (`_ = dialog.ShowAsync()`) and coordinate via `Opened`/`Closed` event handlers.
- **Close-guard tests**: `window.Close()` bypasses `AppWindow.Closing` — use `SendMessage(hwnd, WM_CLOSE, 0, 0)` to trigger the full OS close path.
