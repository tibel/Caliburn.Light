# Validation

Caliburn.Light provides validation infrastructure for view models.

## Overview

The validation framework is built on the standard `INotifyDataErrorInfo` interface, making it compatible with XAML binding validation. Caliburn.Light provides helper classes to simplify implementing validation:

- `ValidationAdapter` - Manages validation errors and integrates with `INotifyDataErrorInfo`
- `RuleValidator` - Defines validation rules for properties
- `RuleValidatorHelper` - Extension methods for common validation rules

## Sample

See the validation demos in the gallery samples:

- [WPF Validation Demo]({{site.github.repository_url}}/tree/main/samples/Caliburn.Light.Gallery.WPF/Validation)
- [WinUI Validation Demo]({{site.github.repository_url}}/tree/main/samples/Caliburn.Light.Gallery.WinUI/Validation)
- [Avalonia Validation Demo]({{site.github.repository_url}}/tree/main/samples/Caliburn.Light.Gallery.Avalonia/Validation)

## Using ValidationAdapter and RuleValidator

The recommended approach is to use `ValidationAdapter` with `RuleValidator`:

```csharp
public sealed class MyViewModel : BindableObject, INotifyDataErrorInfo
{
    private readonly ValidationAdapter _validation;
    private string _name = string.Empty;
    private string _email = string.Empty;

    public MyViewModel()
    {
        _validation = new ValidationAdapter(OnErrorsChanged);
        _validation.Validator = SetupValidator();
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    protected override bool SetProperty<T>(ref T field, T newValue, string? propertyName = null)
    {
        var result = base.SetProperty(ref field, newValue, propertyName);
        if (result)
            _validation.ValidateProperty(this, propertyName!);
        return result;
    }

    private static IValidator SetupValidator()
    {
        var validator = new RuleValidator();
        
        // String length validation
        validator.AddStringLengthRule<MyViewModel>(
            nameof(Name), 
            m => m.Name, 
            minimumLength: 1, 
            maximumLength: 100, 
            "Name is required.");
        
        // Regex validation
        validator.AddRegexRule<MyViewModel>(
            nameof(Email), 
            m => m.Email, 
            @"^[\w\.-]+@[\w\.-]+\.\w+$", 
            "Invalid email format.");
        
        return validator;
    }

    // INotifyDataErrorInfo implementation
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public bool HasErrors => _validation.HasErrors;

    public IEnumerable GetErrors(string? propertyName) => 
        string.IsNullOrEmpty(propertyName) 
            ? _validation.GetErrors() 
            : _validation.GetPropertyErrors(propertyName);

    private void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        RaisePropertyChanged(nameof(HasErrors));
    }
}
```

## Built-in Validation Rules

`RuleValidatorHelper` provides extension methods for common validation scenarios:

```csharp
var validator = new RuleValidator();

// String length validation
validator.AddStringLengthRule<T>(propertyName, getValue, minimumLength, maximumLength, errorMessage);

// Range validation (for comparable types)
validator.AddRangeRule<T, TProperty>(propertyName, getValue, minimum, maximum, errorMessage);

// Regex validation
validator.AddRegexRule<T>(propertyName, getValue, pattern, errorMessage);

// Custom delegate validation
validator.AddDelegateRule<T>(propertyName, validateFunc, errorMessage);
```

### Custom Validation Rules

Create custom rules using `AddDelegateRule`:

```csharp
validator.AddDelegateRule<MyViewModel>(
    nameof(StartDate),
    m => m.StartDate <= m.EndDate,
    "Start date must be before end date.");
```

## Manual Implementation

For simple cases, you can implement `INotifyDataErrorInfo` manually:

```csharp
public class MyViewModel : BindableObject, INotifyDataErrorInfo
{
    private string _name = string.Empty;
    private readonly Dictionary<string, List<string>> _errors = new();

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                ValidateName();
            }
        }
    }

    private void ValidateName()
    {
        ClearErrors(nameof(Name));
        
        if (string.IsNullOrWhiteSpace(Name))
            AddError(nameof(Name), "Name is required.");
        else if (Name.Length < 3)
            AddError(nameof(Name), "Name must be at least 3 characters.");
    }

    private void AddError(string propertyName, string error)
    {
        if (!_errors.ContainsKey(propertyName))
            _errors[propertyName] = new List<string>();
        _errors[propertyName].Add(error);
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    private void ClearErrors(string propertyName)
    {
        if (_errors.Remove(propertyName))
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    public bool HasErrors => _errors.Count > 0;
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName) =>
        string.IsNullOrEmpty(propertyName)
            ? _errors.Values.SelectMany(x => x)
            : _errors.TryGetValue(propertyName, out var errors) ? errors : Enumerable.Empty<string>();
}
```

## XAML Binding

Enable validation in your XAML bindings:

```xml
<!-- WPF -->
<TextBox Text="{Binding Name, ValidatesOnNotifyDataErrors=True}" />

<!-- WinUI / Avalonia -->
<TextBox Text="{Binding Name, Mode=TwoWay}" />
```

The XAML framework will automatically display validation errors based on the `INotifyDataErrorInfo` implementation.

## Disabling Save When Invalid

Combine validation with commands to disable actions when the form is invalid:

```csharp
SaveCommand = DelegateCommandBuilder.NoParameter()
    .OnExecute(() => Save())
    .OnCanExecute(() => !HasErrors)
    .Observe(this, nameof(HasErrors))
    .Build();
```

