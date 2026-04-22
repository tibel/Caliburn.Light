---
description: >-
  WinUI-specific conventions for WindowLifecycle, close guard, and DataContext access
  in Caliburn.Light.WinUI source and test files.
applyTo:
  globs:
    - "src/Caliburn.Light.WinUI/**/*.cs"
    - "tests/Caliburn.Light.WinUI.Tests/**/*.cs"
    - "samples/Caliburn.Light.Gallery.WinUI/**/*.cs"
---

# WinUI Platform Differences

WinUI differs from WPF/Avalonia in three important ways in `WindowLifecycle`:

1. **Close guard**: Uses `view.AppWindow.Closing` (not `Window.Closed`, which fires too late to cancel).
2. **ViewModel access**: Via `view.Content.DataContext` (WinUI `Window` is not a `FrameworkElement`), whereas WPF/Avalonia use `view.DataContext` directly.
3. **Activation**: Single `Activated` handler checks `WindowActivationState` enum (CodeActivated/PointerActivated/Deactivated), whereas WPF/Avalonia use separate `Activated`/`Deactivated` events.
