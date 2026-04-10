# WPF Specifics

Caliburn.Light supports WPF applications on .NET 10.0.

## Getting Started

### Configuration

Configure your WPF application by setting up dependency injection in your `App.xaml.cs`:

```csharp
using Caliburn.Light;
using Caliburn.Light.WPF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Windows;

namespace YourNamespace;

public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    private void Configure()
    {
        var services = new ServiceCollection();

        // Register Caliburn.Light services
        services.AddSingleton<IWindowManager, WindowManager>();
        services.AddSingleton<IEventAggregator, EventAggregator>();
        services.AddSingleton<IViewModelLocator, ViewModelLocator>();
        services.AddTransient(sp => sp.GetRequiredService<IOptions<ViewModelLocatorConfiguration>>().Value);

        // Register view-viewmodel mappings
        services.Configure<ViewModelLocatorConfiguration>(config =>
            config.AddMapping<ShellView, ShellViewModel>());

        // Register view and viewmodel
        services.AddTransient<ShellView>();
        services.AddTransient<ShellViewModel>();

        _serviceProvider = services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        Configure();

        base.OnStartup(e);

        _serviceProvider!.GetRequiredService<IWindowManager>()
            .ShowWindow(_serviceProvider.GetRequiredService<ShellViewModel>());
    }
}
```

**Note**: Remove the `StartupUri` attribute from App.xaml as Caliburn.Light handles window creation.

## Window Manager

The WPF `IWindowManager` provides the following functionality:

### Windows and Dialogs

```csharp
public interface IWindowManager
{
    // Show a non-modal window
    void ShowWindow(object viewModel, string? context = null);

    // Show a modal dialog
    Task ShowDialog(object viewModel, object ownerViewModel, string? context = null);

    // Bring a window to the foreground
    bool Activate(object viewModel);

    // Show a message box
    Task<MessageBoxResult> ShowMessageBoxDialog(MessageBoxDialogOptions options, object ownerViewModel);
}
```

### File Dialogs

```csharp
// Open file dialog
Task<IReadOnlyList<string>> ShowOpenFileDialog(OpenFileDialogOptions options, object ownerViewModel);

// Save file dialog
Task<string> ShowSaveFileDialog(SaveFileDialogOptions options, object ownerViewModel);

// Open folder dialog
Task<IReadOnlyList<string>> ShowOpenFolderDialog(OpenFolderDialogOptions options, object ownerViewModel);
```

## Lifecycle Classes

WPF provides several lifecycle classes:

- `WindowLifecycle` - Manages the lifecycle of a Window
- `PopupLifecycle` - Manages the lifecycle of a Popup
- `PageLifecycle` - Manages the lifecycle of a Page

These classes ensure that `IActivatable` view models are properly activated and deactivated as the associated view becomes visible or hidden. The `WindowLifecycle` also respects `ICloseGuard` — see [Composition & Lifecycle](composition.md) for details.

## XAML Namespace

Import the Caliburn.Light namespace in your XAML files:

```xml
xmlns:cal="https://github.com/tibel/Caliburn.Light/"
```

or

```xml
xmlns:cal="clr-namespace:Caliburn.Light.WPF;assembly=Caliburn.Light.WPF"
```

## View-First vs ViewModel-First

Caliburn.Light supports both approaches:

### ViewModel-First (Recommended)

Use `View.Create` attached property to inject views:

```xml
<ContentControl DataContext="{Binding ActiveItem}" cal:View.Create="True" />
```

### View-First

Use `View.Bind` attached property inside DataTemplates:

```xml
<DataTemplate>
    <local:ItemView cal:View.Bind="True" />
</DataTemplate>
```

## Multiple Views per ViewModel

You can have multiple views for the same ViewModel by using the `View.Context` attached property:

```xml
<ContentControl DataContext="{Binding Item}" 
                cal:View.Create="True"
                cal:View.Context="Detail" />
```

Register the view with context:

```csharp
services.Configure<ViewModelLocatorConfiguration>(config =>
    config.AddMapping<DetailView, ItemViewModel>("Detail"));
```
