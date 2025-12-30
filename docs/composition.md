# Screens, Conductors and Composition

The Screens and Conductors piece of Caliburn.Light is essential for engineering well-structured UIs, particularly when leveraging composition. The terms Screen, Screen Conductor and Screen Collection have been codified by Jeremy Miller during his work on the book "Presentation Patterns" for Addison Wesley. While these patterns are primarily used in Caliburn.Light by inheriting ViewModels from particular base classes, it's important to think of them as roles rather than as View-Models.

## Theory

### Screen
A Screen is a stateful unit of work existing within the presentation tier of an application. It's independent from the application shell. The shell may display many different screens, some even at the same time. Some screen examples might be a modal dialog for application settings, a code editor window in Visual Studio, or a page in a browser.

Often a screen has a lifecycle associated with it which allows the screen to perform custom activation and deactivation logic. For example, in Visual Studio, switching between tabs changes the toolbar icons because each screen has custom activation/deactivation logic.

### Screen Conductor
Once you introduce a Screen Activation Lifecycle into your application, you need some way to enforce it. This is the role of the ScreenConductor. When you show a screen, the conductor makes sure it is properly activated. If you are transitioning away from a screen, it makes sure it gets deactivated. The conductor can also implement Graceful Shutdown by asking screens "Can you close?" before closing them.

### Screen Collection
In an application like Visual Studio, you would have both a ScreenConductor managing activation/deactivation and a ScreenCollection maintaining the list of currently opened screens. Anything in the ScreenCollection remains open, but only one item is active at a time.

## Caliburn.Light Implementations

### Interfaces

Caliburn.Light breaks down the notion of screen activation into several interfaces:

- **IActivatable** – Combines activation and deactivation. Provides `ActivateAsync()` and `DeactivateAsync(bool close)` methods, `IsActive` property, and `Activated`, `Deactivating`, and `Deactivated` events.
- **ICloseGuard** – Indicates that the implementer may need to cancel a close operation. Has one method: `CanCloseAsync()` which returns a `Task<bool>`.

Additional helper interfaces:

- **IHaveDisplayName** – Has a single property called `DisplayName`
- **IBindableObject** – Inherits from `INotifyPropertyChanged` with additional behaviors
- **IBindableCollection&lt;T&gt;** – Composes `IList<T>`, `IBindableObject`, and `INotifyCollectionChanged`
- **IChild** – Implemented by elements that are part of a hierarchy. Has one property named `Parent`.
- **IViewAware** – Implemented by classes which need to be made aware of the view they are bound to

### Base Classes

- **BindableObject** – Implements `IBindableObject` (and thus `INotifyPropertyChanged`)
- **BindableCollection&lt;T&gt;** – Implements `IBindableCollection<T>` by inheriting from `ObservableCollection<T>`
- **Screen** – Inherits from `BindableObject` and implements `IActivatable`, `ICloseGuard`, and `IViewAware`

### Screen Lifecycle Methods

The `Screen` class provides these virtual methods to override:

```csharp
public class Screen : ViewAware, IActivatable, ICloseGuard
{
    // Called once when the screen is first activated
    protected virtual Task OnInitializeAsync() => Task.CompletedTask;

    // Called every time the screen is activated
    protected virtual Task OnActivateAsync() => Task.CompletedTask;

    // Called every time the screen is deactivated
    protected virtual Task OnDeactivateAsync(bool close) => Task.CompletedTask;

    // Override to add custom guard logic
    public virtual Task<bool> CanCloseAsync() => Task.FromResult(true);
}
```

### Conductors

The `IConductor` interface defines:

```csharp
public interface IConductor
{
    Task ActivateItemAsync(object? item);
    Task DeactivateItemAsync(object item, bool close);
    event EventHandler<ActivationProcessedEventArgs> ActivationProcessed;
    IEnumerable GetChildren();
}
```

Caliburn.Light provides three conductor implementations:

#### Conductor&lt;T&gt;
Holds and activates only one item at a time. Deactivation and closing are treated synonymously - activating a new item closes the previous one.

```csharp
public class ShellViewModel : Conductor<object>
{
    public async Task ShowHomeAsync()
    {
        await ActivateItemAsync(new HomeViewModel());
    }

    public async Task ShowSettingsAsync()
    {
        await ActivateItemAsync(new SettingsViewModel());
    }
}
```

#### Conductor&lt;T&gt;.Collection.OneActive
Maintains a collection of items but only one is active at a time. Items remain in the collection when deactivated.

```csharp
public class TabsViewModel : Conductor<IScreen>.Collection.OneActive
{
    public async Task OpenTabAsync()
    {
        var tab = new TabViewModel { DisplayName = "New Tab" };
        await ActivateItemAsync(tab);
    }

    public async Task CloseTabAsync(IScreen tab)
    {
        await DeactivateItemAsync(tab, close: true);
    }
}
```

#### Conductor&lt;T&gt;.Collection.AllActive
Maintains a collection where all items can be active simultaneously.

```csharp
public class DashboardViewModel : Conductor<IWidget>.Collection.AllActive
{
    public async Task AddWidgetAsync(IWidget widget)
    {
        await ActivateItemAsync(widget);
    }
}
```

### Conductor Hierarchy

All conductor implementations inherit from `Screen`, creating a composite pattern. This means:
- Conductors can be conducted by other conductors
- Lifecycle events cascade through the hierarchy
- If a conductor is deactivated, its `ActiveItem` is also deactivated
- If you try to close a conductor, it can only close if all its children can close

## Simple Navigation Example

```csharp
public class ShellViewModel : Conductor<object>
{
    public ShellViewModel()
    {
        ShowPageOneAsync();
    }

    public async Task ShowPageOneAsync()
    {
        await ActivateItemAsync(new PageOneViewModel());
    }

    public async Task ShowPageTwoAsync()
    {
        await ActivateItemAsync(new PageTwoViewModel());
    }
}
```

View:
```xml
<UserControl>
    <DockPanel>
        <StackPanel Orientation="Horizontal" DockPanel.Dock="Top">
            <Button Content="Page One" Command="{Binding ShowPageOneCommand}" />
            <Button Content="Page Two" Command="{Binding ShowPageTwoCommand}" />
        </StackPanel>
        <ContentControl DataContext="{Binding ActiveItem}" cal:View.Create="True" />
    </DockPanel>
</UserControl>
```

## Simple MDI Example

```csharp
public class ShellViewModel : Conductor<IScreen>.Collection.OneActive
{
    private int _count = 1;

    public async Task OpenTabAsync()
    {
        await ActivateItemAsync(new TabViewModel { DisplayName = $"Tab {_count++}" });
    }
}
```

View:
```xml
<Window>
    <DockPanel>
        <Button DockPanel.Dock="Top" Content="Open Tab" Command="{Binding OpenTabCommand}" />
        <TabControl ItemsSource="{Binding Items}" SelectedItem="{Binding ActiveItem}">
            <TabControl.ItemTemplate>
                <DataTemplate>
                    <TextBlock Text="{Binding DisplayName}" />
                </DataTemplate>
            </TabControl.ItemTemplate>
            <TabControl.ContentTemplate>
                <DataTemplate>
                    <local:TabView cal:View.Bind="True" />
                </DataTemplate>
            </TabControl.ContentTemplate>
        </TabControl>
    </DockPanel>
</Window>
```

## View Composition

Use the `View.Create` attached property to compose views:

```xml
<ContentControl DataContext="{Binding ActiveItem}" cal:View.Create="True" />
```

The framework will:
1. Look up the registered view for the view model
2. Create the view instance
3. Set the view model as DataContext
4. Display the view

## Multiple Views per ViewModel

Support multiple views by using the `View.Context` property:

```xml
<ContentControl DataContext="{Binding Item}" cal:View.Create="True" cal:View.Context="Master" />
<ContentControl DataContext="{Binding Item}" cal:View.Create="True" cal:View.Context="Detail" />
```

Register multiple views:
```csharp
services.Configure<ViewModelLocatorConfiguration>(config =>
{
    config.AddMapping<ItemMasterView, ItemViewModel>("Master");
    config.AddMapping<ItemDetailView, ItemViewModel>("Detail");
});
```

## Custom Close Strategy

You can provide a custom close strategy:

```csharp
public class MyCloseStrategy : ICloseStrategy<IScreen>
{
    public async Task<CloseResult<IScreen>> ExecuteAsync(IReadOnlyList<IScreen> toClose)
    {
        // Custom logic to determine which items can close
    }
}
```
