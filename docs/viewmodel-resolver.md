# View/ViewModel Resolution

Caliburn.Light uses a **ViewModel-First** approach, where you compose your application by working with ViewModels, and the framework automatically locates and creates the appropriate Views.

## Why ViewModel-First?

In traditional "View-First" MVVM, you create a View and it instantiates its ViewModel. This has several drawbacks:

- Views need to know how to create ViewModels (tightly coupled)
- ViewModels can't easily communicate or compose with each other
- Testing becomes harder

With **ViewModel-First**:

- ViewModels are regular objects you can create, compose, and test easily
- Views are automatically attached—you never instantiate Views directly
- ViewModel composition feels natural (parents own children)
- Navigation is just creating a new ViewModel

```csharp
// ViewModel-First: You work with ViewModels
var settings = new SettingsViewModel();
await ActivateItemAsync(settings);
// The framework finds and displays SettingsView automatically!
```

## Overview

The framework provides two main interfaces for resolution:

- `IViewModelLocator` - Resolves a view model for a given view
- `ViewModelLocatorConfiguration` - Configures the view-to-viewmodel and viewmodel-to-view mappings

## Registration

Register view/viewmodel pairs by configuring `ViewModelLocatorConfiguration`:

```csharp
var services = new ServiceCollection();

// Register mappings
services.Configure<ViewModelLocatorConfiguration>(config =>
{
    config.AddMapping<ShellView, ShellViewModel>();
    config.AddMapping<HomeView, HomeViewModel>();
    config.AddMapping<SettingsView, SettingsViewModel>();
});

// Register the types with DI
services.AddTransient<ShellView>();
services.AddTransient<ShellViewModel>();
services.AddTransient<HomeView>();
services.AddTransient<HomeViewModel>();
// ... etc
```

### Multiple Views per ViewModel

You can register multiple views for the same view model using a context string:

```csharp
services.Configure<ViewModelLocatorConfiguration>(config =>
{
    // Default view
    config.AddMapping<DocumentView, DocumentViewModel>();

    // Alternative views with context
    config.AddMapping<CompactDocumentView, DocumentViewModel>("Compact");
    config.AddMapping<DetailDocumentView, DocumentViewModel>("Detail");
});
```

Use the context in XAML:

```xml
<ContentControl DataContext="{Binding Document}"
                cal:View.Create="True"
                cal:View.Context="Compact" />
```

## ViewModel-First Approach

In the ViewModel-First approach, you work primarily with view models, and the framework automatically locates and creates the appropriate views.

### View.Create Attached Property

Use `View.Create` to inject a view for a view model:

```xml
<ContentControl DataContext="{Binding ActiveItem}"
                cal:View.Create="True" />
```

The framework will:
1. Look up the registered view type for the view model
2. Create an instance of the view
3. Set the view model as the view's DataContext
4. Display the view in the ContentControl

### Window Manager

The `IWindowManager` also uses ViewModel-First:

```csharp
// The WindowManager finds the view for ShellViewModel automatically
_windowManager.ShowWindow(new ShellViewModel());
```

## View-First Approach

In the View-First approach, you start with views, and the framework attaches them to the corresponding view models.

### View.Bind Attached Property

Use `View.Bind` inside DataTemplates where the view receives its DataContext from the template:

```xml
<DataTemplate>
    <local:ItemView cal:View.Bind="True" />
</DataTemplate>
```

## How Resolution Works

When you request a view for a view model:

1. The framework calls `IViewModelLocator.LocateForModel(model, context)`
2. It retrieves the registered view type for the view model
3. It creates an instance of the view using the service provider
4. It sets the view model as the view's DataContext

When you request a view model for a view:

1. The framework calls `IViewModelLocator.LocateForView(view)`
2. It retrieves the registered view model type for the view
3. It creates an instance of the view model using the service provider

## Example

```csharp
public class ShellViewModel : Conductor<IHaveDisplayName>.Collection.OneActive
{
    public ShellViewModel()
    {
        Items.Add(new HomeViewModel());
        Items.Add(new SettingsViewModel());
    }
}
```

```xml
<Window>
    <DockPanel>
        <ListBox DockPanel.Dock="Left" 
                 ItemsSource="{Binding Items}" 
                 SelectedItem="{Binding ActiveItem}" />
        
        <!-- The view for ActiveItem is automatically resolved and displayed -->
        <ContentControl DataContext="{Binding ActiveItem}"
                        cal:View.Create="True" />
    </DockPanel>
</Window>
```
