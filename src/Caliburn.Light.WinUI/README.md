# Caliburn.Light.WinUI

[![NuGet](https://img.shields.io/nuget/v/Caliburn.Light.WinUI.svg)](https://www.nuget.org/packages/Caliburn.Light.WinUI/)

WinUI 3-specific extensions for Caliburn.Light - a magic-free, powerful MVVM framework for building modern Windows applications with WinUI.

## Overview

Caliburn.Light.WinUI extends the core framework with WinUI 3-specific features:

- **View/ViewModel Resolution**: Automatic view location and binding for WinUI
- **Window Management**: Window lifecycle and modern Windows UI services
- **View Adapters**: WinUI-specific view integration
- **Page Navigation**: Page lifecycle management for NavigationView scenarios
- **Lifecycle Management**: Window, popup, and page lifecycle handlers
- **Modern Windows UI**: Full support for Windows App SDK features

## Installation

Install via NuGet Package Manager:

```
PM> Install-Package Caliburn.Light.WinUI
```

Or via .NET CLI:

```
dotnet add package Caliburn.Light.WinUI
```

This package automatically includes `Caliburn.Light.Core` as a dependency.

## Requirements

- Windows App SDK 1.6.0 or later
- .NET 8.0
- Windows 10.0.19041.0 or later
- Minimum supported: Windows 10.0.17763.0

## Key Features

### View/ViewModel Binding

Automatic view location and data context binding:

```xml
<Window x:Class="MyApp.Views.ShellView"
        xmlns:cal="using:Caliburn.Light.WinUI">
    <cal:View.Model>
        <Binding />
    </cal:View.Model>
    <!-- Your UI -->
</Window>
```

Register view/viewmodel mappings:

```csharp
var resolver = new ViewModelTypeResolver();
resolver.AddMapping<ShellView, ShellViewModel>();
```

### Window Manager

Display windows with modern WinUI features:

```csharp
public class ShellViewModel
{
    private readonly IWindowManager _windowManager;
    
    public ShellViewModel(IWindowManager windowManager)
    {
        _windowManager = windowManager;
    }
    
    public async Task ShowWindowAsync()
    {
        var childViewModel = new ChildWindowViewModel();
        await _windowManager.ShowWindowAsync(childViewModel);
    }
}
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

### Page Lifecycle

Support for NavigationView and Frame navigation:

```csharp
public class MyPageViewModel : Screen
{
    private readonly PageLifecycle _pageLifecycle;
    
    public MyPageViewModel()
    {
        _pageLifecycle = new PageLifecycle(this);
    }
    
    protected override void OnViewReady(object view)
    {
        base.OnViewReady(view);
        if (view is Page page)
        {
            _pageLifecycle.Attach(page);
        }
    }
}
```

### Popup Lifecycle

Manage popup windows:

```csharp
public class PopupViewModel : Screen
{
    private readonly PopupLifecycle _popupLifecycle;
    
    public PopupViewModel()
    {
        _popupLifecycle = new PopupLifecycle(this);
    }
    
    protected override void OnViewReady(object view)
    {
        base.OnViewReady(view);
        if (view is Popup popup)
        {
            _popupLifecycle.Attach(popup);
        }
    }
}
```

### ViewModel Locator

Locate and bind view models to views:

```csharp
var locator = new ViewModelLocator(serviceProvider, viewModelTypeResolver);

// Use in your views to automatically bind view models
```

### Boolean to Visibility Converter

Built-in converter for WinUI:

```xml
<Page.Resources>
    <cal:BooleanToVisibilityConverter x:Key="BoolToVis" />
</Page.Resources>

<TextBlock Visibility="{Binding IsVisible, Converter={StaticResource BoolToVis}}" />
```

## Basic Setup

1. **Configure Services** in `App.xaml.cs`:

```csharp
public partial class App : Application
{
    private SimpleContainer _container;
    private Window _mainWindow;
    
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Setup container
        _container = new SimpleContainer();
        
        // Register services
        _container.RegisterSingleton<IWindowManager, WindowManager>();
        _container.RegisterSingleton<IViewModelLocator, ViewModelLocator>();
        _container.RegisterSingleton<IViewModelTypeResolver, ViewModelTypeResolver>();
        
        // Register view models
        _container.RegisterPerRequest<ShellViewModel>();
        
        // Create and show main window
        _mainWindow = new Window();
        var windowManager = _container.GetInstance<IWindowManager>();
        var shellViewModel = _container.GetInstance<ShellViewModel>();
        
        windowManager.ShowWindowAsync(shellViewModel, null, _mainWindow).GetAwaiter().GetResult();
        _mainWindow.Activate();
    }
}
```

2. **Create ViewModel**:

```csharp
public class ShellViewModel : Screen
{
    public string Title => "My WinUI Application";
}
```

3. **Create View** (`ShellView.xaml`):

```xml
<Page x:Class="MyApp.Views.ShellView"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:cal="using:Caliburn.Light.WinUI">
    <Grid>
        <TextBlock Text="{Binding Title}" />
    </Grid>
</Page>
```

## Target Framework

- .NET 8.0 (Windows 10.0.19041.0 or later)

## Features

- **AOT Compatible**: Supports ahead-of-time compilation
- **Window Lifecycle Management**: Automatic activation/deactivation
- **Modern Windows UI**: Full Windows App SDK integration
- **Page Navigation**: Built-in support for navigation scenarios
- **Popup Management**: PopupLifecycle for managing popups
- **Context-Based Resolution**: Support for contextual view resolution
- **DispatcherQueue Integration**: Uses modern WinUI threading model

## WinUI 3 Migration Notes

If you're migrating from UWP or WinUI 2.x, note that:

- `SuspensionManager` has been removed (no longer needed with modern Windows App SDK)
- `NavigationService` and `FrameAdapter` have been removed (use `PageLifecycle` instead)
- Uses `DispatcherQueue` instead of the legacy `Dispatcher`

## Documentation

For complete documentation, visit:
- [Windows Runtime Specifics](https://github.com/tibel/Caliburn.Light/tree/main/docs/windows-runtime.md)
- [Window Manager](https://github.com/tibel/Caliburn.Light/tree/main/docs/window-manager.md)
- [Full Documentation](https://github.com/tibel/Caliburn.Light/tree/main/docs)

## License

Caliburn.Light is licensed under the [MIT license](https://github.com/tibel/Caliburn.Light/blob/main/LICENSE).
