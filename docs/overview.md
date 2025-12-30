# Introduction

Caliburn.Light is a lightweight, portable MVVM framework for WPF, WinUI, and Avalonia. It is designed to be simple, flexible, and easy to understand.

## ViewModel-First Architecture

Caliburn.Light uses a **ViewModel-First** approach where you compose your application by working with ViewModels, and Views are automatically located and attached. This allows for cleaner architecture, easier testing, and more natural ViewModel composition. See [View/ViewModel Resolution](viewmodel-resolver.md) for details.

### Screens and Conductors
The Screen, ScreenConductor and ScreenCollection patterns enable model-based tracking of the active or current item, enforcing of screen lifecycles and elegant shutdown or shutdown cancellation in an application. Caliburn.Light's implementation of these patterns is an evolution of the one found in Caliburn and supports conducting any type of class. You'll find that Caliburn.Light's screen implementation is quite thorough and even handles asynchronous shutdown scenarios with ease. See [Screens, Conductors and Composition](composition.md) for details.

### Event Aggregator
The integrated EventAggregator is simple yet powerful. The aggregator follows a bus-style pub/sub model. You register a message handler with the aggregator, and it sends you any messages you are interested in. References to handlers are held weakly. See [The Event Aggregator](event-aggregator.md) for details.

### View Locator
For every ViewModel in your application, Caliburn.Light has a strategy for locating the View that should render it. The recommended approach is registration-based using `IViewModelLocator` which is explicit and refactoring safe. Additionally, multiple views over the same ViewModel are supported by specifying a context.

### View Model Locator
Though Caliburn.Light favors the ViewModel-First approach, View-First is also supported by providing a ViewModelLocator with the same mapping semantics as the ViewLocator.

### Window Manager
This service provides a View-Model-centric way of displaying Windows, dialogs, and file pickers. Simply pass it an instance of the View Model and it will locate the view, wrap it in a Window if necessary, and show the window. See [The Window Manager](window-manager.md) for details.

### BindableObject and BindableCollection
What self respecting XAML framework could go without a base implementation of INotifyPropertyChanged? The Caliburn.Light implementation enables string-based (use `nameof` operator) change notification. BindableCollection<T> is a simple collection that inherits from ObservableCollection<T> and allows to suspend change notifications. See [BindableObject](bindable-object.md) and [BindableCollection](bindable-collection.md) for details.

**Note**: It is not ensured that the events are raised on the UI thread. See [UI Thread Dispatching](dispatching.md) for details.

### MVVM
This framework enables MVVM. MVVM isn't hard on its own, but Caliburn.Light strives to go beyond simply getting it done. We want to write elegant, testable, maintainable and extensible presentation layer code...and we want it to be easy to do so. That's what this is about.

### Supported Platforms
Caliburn.Light supports the following platforms:
- **WPF** (.NET 8.0+)
- **WinUI** (Windows App SDK)
- **Avalonia**

### Dependency Injection
Caliburn.Light is designed to work with any dependency injection container. The samples demonstrate usage with `Microsoft.Extensions.DependencyInjection`. Register your services, view models, and the Caliburn.Light infrastructure using the provided extension methods.
