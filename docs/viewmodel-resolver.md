# View/ViewModel Resolution

Caliburn.Light supports both ViewModel-First and View-First workflows. View resolution is explicit: you register view/view-model mappings, then use `View.Create` or `IWindowManager` to request a view for a view model. `View.Bind` associates a view with its existing `DataContext`; it does not resolve a mapped view model.

## Why ViewModel-First?

In traditional "View-First" MVVM, you create a View and it instantiates its ViewModel. This has several drawbacks:

- Views need to know how to create ViewModels (tightly coupled)
- ViewModels can't easily communicate or compose with each other
- Testing becomes harder

With **ViewModel-First**:

- ViewModels are regular objects you can create, compose, and test easily
- Views can be created and attached for you after you register mappings and request resolution
- ViewModel composition feels natural (parents own children)
- View models can be composed independently of their views; navigation is handled by the platform navigation APIs

```csharp
// ViewModel-First: You work with ViewModels
var settings = new SettingsViewModel();
await ActivateItemAsync(settings);
// With a registered mapping, View.Create or IWindowManager can resolve SettingsView.
```

## Overview

The platform package provides two main types for resolution:

- `IViewModelLocator` - Resolves a view model for a given view
- `ViewModelLocatorConfiguration` - Configures the view-to-viewmodel and viewmodel-to-view mappings

## Registration

Register view/viewmodel pairs by configuring `ViewModelLocatorConfiguration`. There is no convention-based fallback: an unmapped type cannot be resolved. Register views with the service provider when they need constructor injection; otherwise the locator can create mapped views with a public parameterless constructor.

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

In the ViewModel-First approach, you work primarily with view models. Once a mapping is registered, the resolution triggers below can locate and create the corresponding view.

### View.Create Attached Property

Use `View.Create` to inject a view for a view model:

```xml
<ContentControl DataContext="{Binding ActiveItem}"
                cal:View.Create="True" />
```

When `View.Create` is enabled, the framework will:
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

In the View-First approach, you start with views that already have a view model as their `DataContext`, and the framework can attach the view to that model.

### View.Bind Attached Property

Use `View.Bind` inside DataTemplates where the view receives its DataContext from the template:

```xml
<DataTemplate>
    <local:ItemView cal:View.Bind="True" />
</DataTemplate>
```

`View.Bind` attaches or detaches the existing `DataContext` when it implements `IViewAware`. It does not look up or create a view model from the registered mappings.

## How Resolution Works

When you request a view for a view model:

1. The framework calls `IViewModelLocator.LocateForModel(model, context)`
2. It retrieves the registered view type for the view model
3. It asks the service provider for the view, then falls back to a public parameterless constructor
4. It sets the view model as the view's DataContext

When you request a view model for a view:

1. The framework calls `IViewModelLocator.LocateForView(view)`
2. It retrieves the registered view model type for the view
3. It asks the service provider for the view model; if it is not registered, resolution returns no model

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
