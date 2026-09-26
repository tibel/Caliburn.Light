# Avalonia Test Guidelines

## Avalonia Headless Limitations

Avalonia headless mode does not provide `IPopupImpl`. Popup open/close lifecycle tests cannot run — only constructor/property tests are possible. Full Popup lifecycle behavior is covered in the WPF and WinUI test projects instead.
