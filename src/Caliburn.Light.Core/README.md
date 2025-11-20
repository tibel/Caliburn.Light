# Caliburn.Light.Core

[![NuGet](https://img.shields.io/nuget/v/Caliburn.Light.Core.svg)](https://www.nuget.org/packages/Caliburn.Light.Core/)

The core package of Caliburn.Light - a magic-free, powerful MVVM framework for building modern .NET applications.

## Overview

Caliburn.Light.Core provides the platform-agnostic foundation for MVVM applications, including:

- **ViewModels**: Base classes and interfaces for building reactive view models
- **Data Binding**: Two-way data binding with `BindableObject` and `IBindableObject`
- **Commands**: Full `ICommand` support with `DelegateCommand` and `AsyncCommand`
- **Validation**: Comprehensive validation framework with `IValidator` and rule-based validation
- **Event Aggregation**: Loosely-coupled communication via `IEventAggregator`
- **Screen Management**: Lifecycle management with conductors and activation/deactivation
- **Dependency Injection**: Simple built-in IoC container via `SimpleContainer`
- **Weak Events**: Memory-efficient event handling

## Installation

Install via NuGet Package Manager:

```
PM> Install-Package Caliburn.Light.Core
```

Or via .NET CLI:

```
dotnet add package Caliburn.Light.Core
```

## Key Features

### BindableObject

Base class for view models with `INotifyPropertyChanged` and `INotifyPropertyChanging` support:

```csharp
public class PersonViewModel : BindableObject
{
    private string _name;
    
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
}
```

### Commands

Create commands with automatic `CanExecute` tracking:

```csharp
public class MyViewModel : BindableObject
{
    public MyViewModel()
    {
        SaveCommand = DelegateCommandBuilder.FromAction(Save)
            .ObservesProperty(() => Name)
            .Build();
    }
    
    public DelegateCommand SaveCommand { get; }
    
    private void Save()
    {
        // Save logic
    }
}
```

### Async Commands

Support for async operations:

```csharp
public AsyncCommand LoadDataCommand { get; }

public MyViewModel()
{
    LoadDataCommand = AsyncCommand.Create(LoadDataAsync);
}

private async Task LoadDataAsync()
{
    // Async operation
}
```

### Validation

Rule-based validation:

```csharp
public class PersonViewModel : BindableObject
{
    private readonly RuleValidator<PersonViewModel> _validator;
    
    public PersonViewModel()
    {
        _validator = new RuleValidator<PersonViewModel>(this);
        _validator.AddRule(vm => !string.IsNullOrEmpty(vm.Name), "Name is required");
    }
    
    public IValidator Validator => _validator;
}
```

### Event Aggregator

Loosely-coupled event communication:

```csharp
public class MyEvent { }

// Subscribe
eventAggregator.Subscribe<MyEvent>(this, (sender, e) => 
{
    // Handle event
});

// Publish
await eventAggregator.PublishAsync(new MyEvent());
```

### Screen Management

Lifecycle management with activation/deactivation:

```csharp
public class ShellViewModel : Conductor<IActivatable>
{
    public async Task ShowDetailAsync(DetailViewModel detail)
    {
        await ActivateItemAsync(detail);
    }
}
```

### Simple Container

Built-in dependency injection:

```csharp
var container = new SimpleContainer();

// Register services
container.RegisterSingleton<IMyService, MyService>();
container.RegisterPerRequest<IViewModel, MyViewModel>();

// Resolve
var service = container.GetInstance<IMyService>();
```

## Target Framework

- .NET 8.0

## Features

- **AOT Compatible**: Supports ahead-of-time compilation
- **Nullable Enabled**: Full nullable reference type support
- **Magic-Free**: No conventions or reflection-based magic
- **Weak Events**: Prevents memory leaks with automatic cleanup
- **Thread-Safe**: Designed for multi-threaded environments

## Documentation

For complete documentation, visit the [Caliburn.Light documentation](https://github.com/tibel/Caliburn.Light/tree/main/docs).

## License

Caliburn.Light is licensed under the [MIT license](https://github.com/tibel/Caliburn.Light/blob/main/LICENSE).
