# Avalonia Test Guidelines

## Avalonia Headless Limitations

Avalonia headless mode does not provide `IPopupImpl`. Popup open/close lifecycle tests cannot run, so Avalonia tests are limited to behavior that does not require the popup implementation. Full Popup lifecycle behavior is covered in the WPF and WinUI test projects instead.
