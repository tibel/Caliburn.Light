# BindableObject

`BindableObject` is the base class for all view models in Caliburn.Light. It provides a clean implementation of `INotifyPropertyChanged` and `INotifyPropertyChanging` for data binding.

## Overview

The `BindableObject` class provides:

- Implementation of `INotifyPropertyChanged` and `INotifyPropertyChanging`
- Helper methods for setting properties and raising change notifications
- Support for suspending notifications during bulk updates

## Basic Usage

Inherit from `BindableObject` and use `SetProperty` to implement property setters:

```csharp
using Caliburn.Light;

public class PersonViewModel : BindableObject
{
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;

    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }

    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }
}
```

## Key Methods

### SetProperty

The `SetProperty` method is the recommended way to update properties:

```csharp
protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
```

- Updates the backing field if the value has changed
- Raises `PropertyChanged` automatically
- Returns `true` if the value was changed, `false` otherwise
- Uses `[CallerMemberName]` so you don't need to specify the property name

Example:

```csharp
private int _count;

public int Count
{
    get => _count;
    set => SetProperty(ref _count, value);
}
```

### SetProperty with Callback

You can also execute code after the property changes:

```csharp
private string _searchText = string.Empty;

public string SearchText
{
    get => _searchText;
    set
    {
        if (SetProperty(ref _searchText, value))
        {
            // Value changed - perform search
            PerformSearch();
        }
    }
}
```

### RaisePropertyChanged

Use `RaisePropertyChanged` when you need to manually notify that a property has changed:

```csharp
protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
```

### RaisePropertyChanging

Similarly, use `RaisePropertyChanging` to notify that a property is about to change:

```csharp
protected void RaisePropertyChanging([CallerMemberName] string? propertyName = null)
```

This is called automatically by `SetProperty` before the value is updated.

### Computed Properties

`RaisePropertyChanged` is useful for computed properties:

```csharp
private string _firstName = string.Empty;
private string _lastName = string.Empty;

public string FirstName
{
    get => _firstName;
    set
    {
        if (SetProperty(ref _firstName, value))
        {
            RaisePropertyChanged(nameof(FullName));
        }
    }
}

public string LastName
{
    get => _lastName;
    set
    {
        if (SetProperty(ref _lastName, value))
        {
            RaisePropertyChanged(nameof(FullName));
        }
    }
}

// Computed property - no setter
public string FullName => $"{FirstName} {LastName}";
```

## IBindableObject Interface

`BindableObject` implements the `IBindableObject` interface:

```csharp
public interface IBindableObject : INotifyPropertyChanged
{
    IDisposable SuspendNotifications();
    void Refresh();
}
```

### Refresh

The `Refresh` method raises `PropertyChanged` with an empty string, indicating that all properties should be re-read:

```csharp
public void ReloadData()
{
    _firstName = "John";
    _lastName = "Doe";
    _age = 30;
    
    // Notify that all properties have changed
    Refresh();
}
```

## Example: Complete ViewModel

Here's a complete example showing various features:

```csharp
using Caliburn.Light;
using System.Windows.Input;

public class CustomerViewModel : BindableObject
{
    private string _name = string.Empty;
    private string _email = string.Empty;
    private bool _isActive;

    public CustomerViewModel()
    {
        SaveCommand = DelegateCommandBuilder.NoParameter()
            .OnExecute(() => Save())
            .OnCanExecute(() => CanSave)
            .Observe(this, nameof(CanSave))
            .Build();
    }

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                RaisePropertyChanged(nameof(CanSave));
            }
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            if (SetProperty(ref _email, value))
            {
                RaisePropertyChanged(nameof(CanSave));
            }
        }
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    // Computed property
    public bool CanSave => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Email);

    public ICommand SaveCommand { get; }

    private void Save()
    {
        // Save logic here
    }
}
```

## Thread Safety

`BindableObject` does not automatically marshal property change notifications to the UI thread. If you update properties from a background thread, you should use the UI dispatcher. See [UI Thread Dispatching](dispatching.md) for more information.

## See Also

- [BindableCollection](bindable-collection.md) - Observable collection for data binding
- [Screen](composition.md) - Extended base class with lifecycle support
- [Commands](commands.md) - Command infrastructure
