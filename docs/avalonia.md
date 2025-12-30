# Avalonia Specifics

Caliburn.Light supports Avalonia applications.

## Getting Started

### Configuration

Configure your Avalonia application by setting up dependency injection in your `App.axaml.cs`:

```csharp
using Avalonia;
using Avalonia.Markup.Xaml;
using Caliburn.Light;
using Caliburn.Light.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace YourNamespace;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private IServiceProvider Configure()
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

        return services.BuildServiceProvider();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var serviceProvider = Configure();

        serviceProvider.GetRequiredService<IWindowManager>()
            .ShowWindow(serviceProvider.GetRequiredService<ShellViewModel>());

        base.OnFrameworkInitializationCompleted();
    }
}
```

## Window Manager

The Avalonia `IWindowManager` provides the following functionality:

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
}
```

### File Pickers

```csharp
// Open file picker
Task<IReadOnlyList<IStorageFile>> ShowOpenFilePickerAsync(FilePickerOpenOptions options, object ownerViewModel);

// Save file picker
Task<IStorageFile?> ShowSaveFilePickerAsync(FilePickerSaveOptions options, object ownerViewModel);

// Folder picker
Task<IReadOnlyList<IStorageFolder>> ShowOpenFolderPickerAsync(FolderPickerOpenOptions options, object ownerViewModel);
```

The file picker options and return types use Avalonia's native `Avalonia.Platform.Storage` types.

## Lifecycle Classes

Avalonia provides lifecycle classes:

- `WindowLifecycle` - Manages the lifecycle of a Window
- `PopupLifecycle` - Manages the lifecycle of a Popup

These classes ensure that `IActivatable` view models are properly activated and deactivated as the associated view becomes visible or hidden.

## XAML Namespace

Import the Caliburn.Light namespace in your AXAML files:

```xml
xmlns:cal="clr-namespace:Caliburn.Light.Avalonia;assembly=Caliburn.Light.Avalonia"
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
