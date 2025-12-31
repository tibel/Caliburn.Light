![Logo](images/logo.png)

# Caliburn.Light Documentation

Caliburn.Light is a lightweight, portable MVVM framework designed for building maintainable XAML applications. It supports **WPF**, **WinUI 3**, and **Avalonia**.

## Key Features

- **ViewModel-First** - Compose your application by instantiating ViewModels; Views are located and attached automatically
- **Screens & Conductors** - Lifecycle management with activation, deactivation, and graceful shutdown
- **Commands** - Type-safe, observable commands with async support
- **Event Aggregator** - Loosely-coupled messaging between components
- **Validation** - Built-in validation infrastructure using `INotifyDataErrorInfo`
- **Window Manager** - Show windows and dialogs in a ViewModel-centric way

## Quick Links

| New to Caliburn.Light? | Looking for something specific? |
|------------------------|--------------------------------|
| [Quick Start Guide](quick-start.md) | [API by Topic](#documentation-by-topic) |
| [Overview](overview.md) | [Platform Guides](#platform-guides) |
| [Sample Applications](#samples) | [Advanced Topics](#advanced-topics) |

---

## Documentation by Topic

### Getting Started
- [Quick Start](quick-start.md) - Get up and running in minutes
- [Overview](overview.md) - Introduction to Caliburn.Light concepts
- [NuGet Packages](nuget.md) - Available packages and installation
- [Basic Configuration](configuration.md) - Setting up dependency injection

### Core Concepts
- [BindableObject](bindable-object.md) - Base class for ViewModels with `INotifyPropertyChanged`
- [BindableCollection](bindable-collection.md) - Observable collection for data binding
- [Commands](commands.md) - DelegateCommand, AsyncCommand, and the builder pattern
- [ViewModel-First](viewmodel-resolver.md) - View/ViewModel resolution and composition

### Application Structure
- [Screens, Conductors and Composition](composition.md) - Building complex view hierarchies
- [The Window Manager](window-manager.md) - Displaying windows and dialogs
- [The Event Aggregator](event-aggregator.md) - Pub/sub messaging pattern
- [Validation](validation.md) - Input validation with `INotifyDataErrorInfo`

### Platform Guides
- [WPF](wpf.md) - WPF-specific configuration and features
- [WinUI](winui.md) - WinUI 3 / Windows App SDK support
- [Avalonia](avalonia.md) - Cross-platform UI with Avalonia

### Advanced Topics
- [UI Thread Dispatching](dispatching.md) - Working with the UI thread
- [Async (Task Support)](async.md) - Asynchronous patterns and lifecycle
- [Weak Event Handler](weak-event-handler.md) - Prevent memory leaks with weak event subscriptions
- [Design Time Support](design-time.md) - Visual Studio designer integration

### Reference
- [History](history.md) - Project history and evolution
- [Build the Code](build.md) - Building from source

---

## Samples

Explore the sample applications to see Caliburn.Light in action:

- [WPF Gallery](https://github.com/tibel/Caliburn.Light/tree/main/samples/Caliburn.Light.Gallery.WPF) - Comprehensive WPF examples
- [WinUI Gallery](https://github.com/tibel/Caliburn.Light/tree/main/samples/Caliburn.Light.Gallery.WinUI) - WinUI 3 demonstrations
- [Avalonia Gallery](https://github.com/tibel/Caliburn.Light/tree/main/samples/Caliburn.Light.Gallery.Avalonia) - Cross-platform Avalonia examples

---

## Contributing

Contributions are welcome! Please see the [GitHub repository](https://github.com/tibel/Caliburn.Light) for more information.
