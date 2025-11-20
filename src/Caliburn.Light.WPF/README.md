# Caliburn.Light.WPF

[![NuGet](https://img.shields.io/nuget/v/Caliburn.Light.WPF.svg)](https://www.nuget.org/packages/Caliburn.Light.WPF/)

WPF-specific extensions for Caliburn.Light - a magic-free, powerful MVVM framework for building WPF applications.

## Overview

Caliburn.Light.WPF extends the core framework with WPF-specific features:

- **View/ViewModel Resolution**: Automatic view location and binding
- **Window Management**: Window lifecycle and dialog services
- **View Adapters**: WPF-specific view integration
- **Design-Time Support**: Enhanced Visual Studio designer experience
- **Page Navigation**: Page lifecycle management
- **Lifecycle Management**: Window, popup, and page lifecycle handlers

## Installation

Install via NuGet Package Manager:

```
PM> Install-Package Caliburn.Light.WPF
```

Or via .NET CLI:

```
dotnet add package Caliburn.Light.WPF
```

This package automatically includes `Caliburn.Light.Core` as a dependency.

## Key Features

### View/ViewModel Binding

Automatic view location and data context binding:

```xaml
<Window x:Class="MyApp.Views.ShellView"
        xmlns:cal="http://schemas.caliburnproject.org"
        cal:View.Model="{Binding}">
    <!-- Your UI -->
</Window>
```

Register view/viewmodel mappings:

```csharp
var resolver = new ViewModelTypeResolver();
resolver.AddMapping<ShellView, ShellViewModel>();
```

### Window Manager

Display windows and dialogs:

```csharp
public class ShellViewModel
{
    private readonly IWindowManager _windowManager;
    
    public ShellViewModel(IWindowManager windowManager)
    {
        _windowManager = windowManager;
    }
    
    public async Task ShowDialogAsync()
    {
        var dialogViewModel = new DialogViewModel();
        await _windowManager.ShowDialogAsync(dialogViewModel);
    }
    
    public async Task ShowWindowAsync()
    {
        var windowViewModel = new ChildWindowViewModel();
        await _windowManager.ShowWindowAsync(windowViewModel);
    }
}
```

### Common Dialogs

Built-in support for common Windows dialogs:

```csharp
// Message box
var result = await _windowManager.ShowMessageBoxAsync(
    "Are you sure?", 
    "Confirmation",
    MessageBoxButton.YesNo);

// Open file dialog
var settings = new OpenFileDialogSettings
{
    Title = "Select a file",
    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
};
var result = await _windowManager.ShowOpenFileDialogAsync(settings);

// Save file dialog
var saveSettings = new SaveFileDialogSettings
{
    Title = "Save file",
    Filter = "Text files (*.txt)|*.txt"
};
var result = await _windowManager.ShowSaveFileDialogAsync(saveSettings);

// Open folder dialog
var folderSettings = new OpenFolderDialogSettings
{
    Title = "Select a folder"
};
var result = await _windowManager.ShowOpenFolderDialogAsync(folderSettings);
```

### Lifecycle Management

Manage window, popup, and page lifecycles:

```csharp
public class MyViewModel : Screen
{
    private readonly WindowLifecycle _windowLifecycle;
    
    public MyViewModel()
    {
        _windowLifecycle = new WindowLifecycle(this);
    }
    
    protected override void OnViewReady(object view)
    {
        base.OnViewReady(view);
        if (view is Window window)
        {
            _windowLifecycle.Attach(window);
        }
    }
}
```

### ViewModel Locator

Locate and bind view models to views:

```csharp
var locator = new ViewModelLocator(serviceProvider, viewModelTypeResolver);

// In XAML
<Window cal:View.Model="{Binding Source={x:Static cal:ViewModelLocator.Instance}}">
```

### View Context

Support for contextual view resolution:

```csharp
View.SetContext(element, "dialog");
```

## Basic Setup

1. **Configure Services** in `App.xaml.cs`:

```csharp
public partial class App : Application
{
    private SimpleContainer _container;
    
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Setup container
        _container = new SimpleContainer();
        
        // Register services
        _container.RegisterSingleton<IWindowManager, WindowManager>();
        _container.RegisterSingleton<IViewModelLocator, ViewModelLocator>();
        _container.RegisterSingleton<IViewModelTypeResolver, ViewModelTypeResolver>();
        
        // Register view models
        _container.RegisterPerRequest<ShellViewModel>();
        
        // Show main window
        var windowManager = _container.GetInstance<IWindowManager>();
        var shellViewModel = _container.GetInstance<ShellViewModel>();
        await windowManager.ShowWindowAsync(shellViewModel);
    }
}
```

2. **Create ViewModel**:

```csharp
public class ShellViewModel : Screen
{
    public string Title => "My Application";
}
```

3. **Create View** (`ShellView.xaml`):

```xaml
<Window x:Class="MyApp.Views.ShellView"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:cal="http://schemas.caliburnproject.org"
        Title="{Binding Title}"
        Height="450" Width="800">
    <Grid>
        <!-- Your UI -->
    </Grid>
</Window>
```

## Target Framework

- .NET 8.0 (Windows)

## Features

- **Window Lifecycle Management**: Automatic activation/deactivation
- **Dialog Services**: MessageBox, OpenFile, SaveFile, OpenFolder dialogs
- **Page Navigation**: PageLifecycle for navigation scenarios
- **Design-Time Support**: Enhanced designer experience
- **Context-Based Resolution**: Support for contextual view resolution
- **Popup Management**: PopupLifecycle for managing popup windows

## Documentation

For complete documentation, visit:
- [WPF Specifics](https://github.com/tibel/Caliburn.Light/tree/main/docs/wpf.md)
- [Window Manager](https://github.com/tibel/Caliburn.Light/tree/main/docs/window-manager.md)
- [Full Documentation](https://github.com/tibel/Caliburn.Light/tree/main/docs)

## License

Caliburn.Light is licensed under the [MIT license](https://github.com/tibel/Caliburn.Light/blob/main/LICENSE).
