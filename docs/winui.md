# WinUI Specifics

Caliburn.Light supports WinUI 3 (Windows App SDK) applications.

## Getting Started

### Configuration

Configure your WinUI application by setting up dependency injection in your `App.xaml.cs`:

```csharp
using Caliburn.Light;
using Caliburn.Light.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml;

namespace YourNamespace;

public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    public App()
    {
        InitializeComponent();
    }

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

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Configure();

        _serviceProvider!.GetRequiredService<IWindowManager>()
            .ShowWindow(_serviceProvider.GetRequiredService<ShellViewModel>());
    }
}
```

## Window Manager

The WinUI `IWindowManager` provides the following functionality:

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

    // Show a ContentDialog
    Task<ContentDialogResult> ShowContentDialog(object viewModel, object ownerViewModel, string? context = null);
}
```

### File Pickers

```csharp
// Open file picker
Task<IReadOnlyList<PickFileResult>> ShowFileOpenPickerAsync(FileOpenPickerOptions options, object ownerViewModel);

// Save file picker
Task<PickFileResult?> ShowFileSavePickerAsync(FileSavePickerOptions options, object ownerViewModel);

// Folder picker
Task<IReadOnlyList<PickFolderResult>> ShowFolderPickerAsync(FolderPickerOptions options, object ownerViewModel);
```

## Lifecycle Classes

WinUI provides several lifecycle classes:

- `WindowLifecycle` - Manages the lifecycle of a Window
- `PopupLifecycle` - Manages the lifecycle of a Popup
- `PageLifecycle` - Manages the lifecycle of a Page
- `ContentDialogLifecycle` - Manages the lifecycle of a ContentDialog

These classes ensure that `IActivatable` view models are properly activated and deactivated as the associated view becomes visible or hidden.

## View-First vs ViewModel-First

Caliburn.Light supports both approaches:

### ViewModel-First (Recommended)

Use `View.Create` attached property to inject views:

```xml
<ContentControl DataContext="{x:Bind ViewModel.ActiveItem, Mode=OneWay}" cal:View.Create="True" />
```

### View-First

Use `View.Bind` attached property inside DataTemplates:

```xml
<DataTemplate>
    <local:ItemView cal:View.Bind="True" />
</DataTemplate>
```

## Special Values (Event to Command)

WinUI supports binding UI events to commands using `ISpecialValue`. This is useful for events like `ItemClick` that pass event-specific data.

### Usage

1. Create a class implementing `ISpecialValue`:

```csharp
public sealed class ClickedItem : ISpecialValue
{
    public object? Resolve(CommandExecutionContext context)
    {
        return context.EventArgs is ItemClickEventArgs args
            ? args.ClickedItem
            : null;
    }
}
```

2. Use `View.CommandParameter` in XAML:

```xml
<GridView ItemsSource="{x:Bind ViewModel.Items}"
          IsItemClickEnabled="True"
          ItemClick="{x:Bind ViewModel.ItemClickCommand.OnEvent}">
    <cal:View.CommandParameter>
        <local:ClickedItem />
    </cal:View.CommandParameter>
</GridView>
```

3. Define the command in your ViewModel:

```csharp
public AsyncCommand ItemClickCommand { get; }

public MyViewModel()
{
    ItemClickCommand = DelegateCommandBuilder.WithParameter<ItemViewModel>()
        .OnExecute(item => HandleItemClick(item))
        .Build();
}
```

The `ISpecialValue.Resolve` method receives a `CommandExecutionContext` containing:
- `Source` - The UI element that raised the event
- `EventArgs` - The event arguments
- `Parameter` - The command parameter (if any)
