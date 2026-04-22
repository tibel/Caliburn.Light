---
description: >-
  Project overview, architecture, build/test commands, and key coding conventions
  for Caliburn.Light — a magic-free MVVM framework supporting WPF, Avalonia, and WinUI.
applyTo:
  globs:
    - "**/*.cs"
    - "**/*.csproj"
    - "**/*.slnx"
    - "**/*.props"
    - "global.json"
    - ".github/workflows/*.yml"
---

# Caliburn.Light

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

# Pack NuGet packages (CI — required for deterministic builds)
dotnet pack Caliburn.Light.slnx --configuration Release -p:ContinuousIntegrationBuild=True
```

- `.slnx` solution format. Central package management via `Directory.Packages.props`.
- CI runs Core and Avalonia tests on `ubuntu-latest`.

## Architecture

One shared core, three platform packages, plus a deprecated meta-package:

- **Caliburn.Light.Core** — Platform-agnostic MVVM: `Screen`, `Conductor`, `EventAggregator`, `DelegateCommand`, validation, weak events (`WeakEventHandler`/`WeakEventSource`). Targets `net10.0`. AOT-compatible.
- **Caliburn.Light.WPF** — WPF integration: `WindowManager`, lifecycle classes, view location. Targets `net10.0-windows7.0`.
- **Caliburn.Light.Avalonia** — Avalonia integration: same API surface as WPF. Targets `net10.0`. AOT-compatible.
- **Caliburn.Light.WinUI** — WinUI integration: adds `ContentDialogLifecycle`, uses `AppWindow.Closing` for close guard support. Targets `net10.0-windows10.0.19041.0`. AOT-compatible.
- **Caliburn.Light** — Meta-package; re-exports WPF. No independent source.

Each platform project references Core and mirrors the same patterns: `WindowLifecycle`, `PopupLifecycle`, `WindowManager`, `ViewModelLocator`, `View`, `ViewAdapter`, `BindingHelper`.

## Key Conventions

### Lifecycle event cleanup

All events subscribed in lifecycle constructors **must** be unsubscribed when the lifecycle ends. In `WindowLifecycle`, `OnViewClosed` is terminal — unsubscribe everything (`Closed`, `Activated`, `Deactivated`, `Closing`) there. In `PopupLifecycle` and `ContentDialogLifecycle`, `Closed` is **not** terminal (controls can reopen), so events stay wired.

### Weak events

External event subscriptions use `WeakEventHandler`/`WeakEventSource` to prevent memory leaks.

### Fire-and-forget async

Use `.Observe()` extension (from `TaskHelper.cs`) for fire-and-forget async calls:
```csharp
activatable.ActivateAsync().Observe();
```
This suppresses unobserved task warnings while still propagating exceptions.
