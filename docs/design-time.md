# Design Time Support

Enabling design-time support in Visual Studio (or Blend) is straightforward with Caliburn.Light.

## Setting a Design-Time DataContext

To enable design-time data binding, set a design-time DataContext on your view:

### WPF

```xml
<Window 
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:vm="clr-namespace:YourNamespace.ViewModels"
    mc:Ignorable="d" 
    d:DataContext="{d:DesignInstance Type=vm:MainViewModel, IsDesignTimeCreatable=True}">
    
    <!-- Your view content here -->
    
</Window>
```

### WinUI

```xml
<Page
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:vm="using:YourNamespace.ViewModels"
    mc:Ignorable="d"
    d:DataContext="{d:DesignInstance Type=vm:MainViewModel, IsDesignTimeCreatable=True}">
    
    <!-- Your view content here -->
    
</Page>
```

### Avalonia

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:vm="clr-namespace:YourNamespace.ViewModels"
             mc:Ignorable="d"
             d:DesignWidth="800" d:DesignHeight="450"
             x:DataType="vm:MainViewModel">
    
    <!-- Your view content here -->
    
</UserControl>
```

## Creating Design-Time ViewModels

For the design-time experience to work well, your view model should have a parameterless constructor or you should create a design-time specific version:

```csharp
public class MainViewModel : Screen
{
    // Parameterless constructor for design-time
    public MainViewModel()
    {
        // Initialize with sample data for design-time
        if (DesignMode.DesignModeEnabled)
        {
            Title = "Sample Title";
            Items = new[] { "Item 1", "Item 2", "Item 3" };
        }
    }

    // Constructor for runtime with DI
    public MainViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    public string Title { get; set; } = string.Empty;
    public IEnumerable<string> Items { get; set; } = Array.Empty<string>();
}
```

## Using d:DesignInstance

The `d:DesignInstance` markup extension has several useful properties:

- **Type** - The type of the design-time DataContext
- **IsDesignTimeCreatable** - Set to `True` if the type has a parameterless constructor
- **CreateList** - Set to `True` to create a collection for ItemsSource scenarios

Example for a list:
```xml
<ListBox d:ItemsSource="{d:DesignInstance Type=vm:ItemViewModel, CreateList=True}" />
```

## Checking for Design Mode

You can check if your code is running in design mode:

### WPF
```csharp
if (DesignMode.DesignModeEnabled)
{
    // Design-time code
}
```

### WinUI
```csharp
if (DesignMode.DesignModeEnabled)
{
    // Design-time code
}
```

### Avalonia
```csharp
using Avalonia.Controls;

if (Design.IsDesignMode)
{
    // Design-time code
}
```
