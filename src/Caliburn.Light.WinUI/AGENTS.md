# WinUI Platform Guidelines

WinUI differs from WPF/Avalonia in three important ways in `WindowLifecycle`:

1. **Close guard**: Uses `view.AppWindow.Closing` (not `Window.Closed`, which fires too late to cancel).
2. **ViewModel access**: Via `view.Content.DataContext` (WinUI `Window` is not a `FrameworkElement`), whereas WPF/Avalonia use `view.DataContext` directly.
3. **Activation**: Single `Activated` handler checks `WindowActivationState` enum (CodeActivated/PointerActivated/Deactivated), whereas WPF/Avalonia use separate `Activated`/`Deactivated` events.
