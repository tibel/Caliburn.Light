![Logo](logo.png)

# Caliburn.Light

[![NuGet](https://img.shields.io/nuget/v/Caliburn.Light.Core.svg)](https://www.nuget.org/packages/Caliburn.Light.Core/)
[![CI](https://github.com/tibel/Caliburn.Light/actions/workflows/ci.yml/badge.svg)](https://github.com/tibel/Caliburn.Light/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A magic-free MVVM framework for building modern .NET applications with **WPF**, **WinUI 3**, and **Avalonia**.

## Features

- **Magic-free** — no conventions, no surprises; explicit configuration only
- **Screens & Conductors** — lifecycle management with activation, deactivation, and close guards
- **Commands** — type-safe `DelegateCommand` and `AsyncCommand` with builder pattern
- **Event Aggregator** — loosely-coupled pub/sub messaging with weak references
- **Validation** — built-in `INotifyDataErrorInfo` support with rule-based validation
- **Window Manager** — show windows, dialogs, and file pickers in a ViewModel-centric way
- **Weak Events** — memory-efficient event handling to prevent leaks

Inspired by [Caliburn.Micro](https://github.com/caliburn-micro/caliburn.micro), [Prism](https://github.com/PrismLibrary/Prism), and [MVVMLight](https://github.com/lbugnion/mvvmlight).

## Install

Caliburn.Light is available on [NuGet](https://www.nuget.org/profiles/tibel):

```
dotnet add package Caliburn.Light.WPF
dotnet add package Caliburn.Light.WinUI
dotnet add package Caliburn.Light.Avalonia
```

The platform packages include `Caliburn.Light.Core` automatically.

## Documentation

All Caliburn.Light [documentation](docs/README.md) is included, with a [Quick Start](docs/quick-start.md) guide to get you up and running.

## License

Caliburn.Light is licensed under the [MIT license](LICENSE).
