# Commands

Caliburn.Light provides a robust command infrastructure for building MVVM applications with `ICommand` support.

## Overview

The framework includes several command implementations:

- `DelegateCommand` - A synchronous command that can be data-bound
- `DelegateCommand<T>` - A synchronous command with a typed parameter
- `AsyncDelegateCommand` - An asynchronous command that prevents re-entrancy
- `AsyncDelegateCommand<T>` - An asynchronous command with a typed parameter
- `DelegateCommandBuilder` - A fluent builder for creating commands

## DelegateCommandBuilder

The recommended way to create commands is using the `DelegateCommandBuilder`:

### Synchronous Commands

```csharp
public class MyViewModel : BindableObject
{
    public MyViewModel()
    {
        // Simple command
        SaveCommand = DelegateCommandBuilder.NoParameter()
            .OnExecute(() => Save())
            .Build();

        // Command with canExecute
        DeleteCommand = DelegateCommandBuilder.NoParameter()
            .OnExecute(() => Delete())
            .OnCanExecute(() => CanDelete)
            .Observe(this, nameof(CanDelete))
            .Build();
    }

    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }

    public bool CanDelete { get; set; }

    private void Save() { /* ... */ }
    private void Delete() { /* ... */ }
}
```

### Commands with Parameters

```csharp
public class MyViewModel : BindableObject
{
    public MyViewModel()
    {
        SelectItemCommand = DelegateCommandBuilder.WithParameter<ItemViewModel>()
            .OnExecute(item => SelectItem(item))
            .Build();
    }

    public ICommand SelectItemCommand { get; }

    private void SelectItem(ItemViewModel? item)
    {
        if (item is null) return;
        // Handle selection
    }
}
```

### Asynchronous Commands

```csharp
public class MyViewModel : BindableObject
{
    public MyViewModel()
    {
        LoadCommand = DelegateCommandBuilder.NoParameter()
            .OnExecute(() => LoadAsync())
            .Build();

        SaveCommand = DelegateCommandBuilder.NoParameter()
            .OnExecute(() => SaveAsync())
            .OnCanExecute(() => !HasErrors)
            .Observe(this, nameof(HasErrors))
            .Build();
    }

    public AsyncDelegateCommand LoadCommand { get; }
    public AsyncDelegateCommand SaveCommand { get; }

    public bool HasErrors { get; set; }

    private async Task LoadAsync()
    {
        // Async operation
        await Task.Delay(1000);
    }

    private async Task SaveAsync()
    {
        // Async operation
        await Task.Delay(1000);
    }
}
```

## AsyncCommand Features

The `AsyncCommand` base class provides:

- **Re-entrancy prevention**: While the command is executing, `CanExecute` returns `false`
- **IsExecuting property**: Track whether the command is currently running
- **Executing event**: Static event fired when an async command starts (useful for global loading indicators)

```csharp
// Check if command is running
if (myCommand.IsExecuting)
{
    // Show loading indicator
}

// Global handler for async command execution
AsyncCommand.Executing += (sender, args) =>
{
    // args.Task contains the executing task
};
```

`Executing` is a static event, so a plain subscription keeps any captured object alive for the lifetime of the process. For handlers that should not outlive the object they capture, register weakly instead:

```csharp
private readonly IDisposable _registration;

public LoadingIndicator()
{
    _registration = WeakEventHandler.RegisterAsyncCommandExecutingWeak(this,
        static (subscriber, sender, args) => subscriber.OnCommandExecuting(args.Task));
}

private void OnCommandExecuting(Task task) { }

// Dispose the registration to detach deterministically
_registration.Dispose();
```

See [Weak Event Handler](weak-event-handler.md) for details.

## Observing Property Changes

Commands can automatically re-evaluate `CanExecute` when properties change:

```csharp
SaveCommand = DelegateCommandBuilder.NoParameter()
    .OnExecute(() => Save())
    .OnCanExecute(() => IsValid && !IsBusy)
    .Observe(this, nameof(IsValid), nameof(IsBusy))
    .Build();
```

## XAML Binding

Bind commands to buttons and other controls:

```xml
<Button Content="Save" Command="{Binding SaveCommand}" />

<Button Content="Delete" 
        Command="{Binding DeleteCommand}" 
        CommandParameter="{Binding SelectedItem}" />
```

## CommandExecutionContext

A `CommandExecutionContext` describes a command invocation. The indexer `this[string key]` holds additional custom values:

```csharp
public sealed class CommandExecutionContext
{
    public object? this[string key] { get; set; }

    public object? Source { get; set; }
    public object? Target { get; set; }
    public object? EventArgs { get; set; }
}
```

- `Source` — the element that raised the event (populated on the `OnEvent` invocation path)
- `Target` — the intended target of the command (not populated by the framework)
- `EventArgs` — the event data of the triggering event (populated on the `OnEvent` invocation path)

When a bound `CommandParameter` is an `ISpecialValue` — such as the platform's `ExecutionContextResolver` — and the command is invoked through `OnEvent`, the framework creates a context populated with `Source` and `EventArgs` and calls `ISpecialValue.Resolve(context)`, which may override `Source`. When the parameter is coerced through `CoerceParameter<TParameter>.Default` instead, the context is created empty (`Source`/`EventArgs` are `null`).

## Direct Command Creation

You can also create commands directly without the builder:

```csharp
// Simple delegate command
var command = new DelegateCommand(
    execute: () => DoSomething(),
    canExecute: () => CanDoSomething);

// With property observation
var command = new DelegateCommand(
    execute: () => DoSomething(),
    canExecute: () => CanDoSomething,
    target: this,
    propertyNames: nameof(CanDoSomething));
```
