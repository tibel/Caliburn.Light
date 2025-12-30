# BindableCollection

`BindableCollection<T>` is an observable collection optimized for MVVM applications. It extends `ObservableCollection<T>` with additional features useful for data binding scenarios.

## Overview

`BindableCollection<T>` provides:

- All functionality of `ObservableCollection<T>`
- Ability to suspend change notifications for bulk operations
- Implementation of `IBindableCollection<T>`
- Helper methods for common collection operations

## Basic Usage

Use `BindableCollection<T>` whenever you need a collection that notifies the UI of changes:

```csharp
using Caliburn.Light;

public class TaskListViewModel : BindableObject
{
    public BindableCollection<TaskViewModel> Tasks { get; } = new();

    public void AddTask()
    {
        Tasks.Add(new TaskViewModel { Title = "New Task" });
    }

    public void RemoveTask(TaskViewModel task)
    {
        Tasks.Remove(task);
    }

    public void ClearTasks()
    {
        Tasks.Clear();
    }
}
```

## Suspending Notifications

When performing bulk operations, you can suspend change notifications to improve performance:

```csharp
public class ProductListViewModel : BindableObject
{
    public BindableCollection<ProductViewModel> Products { get; } = new();

    public async Task LoadProductsAsync()
    {
        var products = await _productService.GetAllAsync();

        // Suspend notifications during bulk add
        using (Products.SuspendNotifications())
        {
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(new ProductViewModel(product));
            }
        }
        // Notifications resume here - UI gets one update instead of many
    }
}
```

### How SuspendNotifications Works

- When suspended, `CollectionChanged` and `PropertyChanged` events are not raised
- When the suspension ends (via `Dispose`), notifications resume but no automatic `Reset` is raised
- Multiple levels of suspension are supported (reference counted)
- Always use a `using` statement to ensure notifications are properly resumed
- Use `AddRange()` or `RemoveRange()` which automatically raise a `Reset` notification, or call `Refresh()` manually after bulk operations

## IBindableCollection Interface

`BindableCollection<T>` implements `IBindableCollection<T>`:

```csharp
public interface IBindableCollection<T> : IList<T>, INotifyCollectionChanged, IBindableObject
{
    void Move(int oldIndex, int newIndex);
    void AddRange(IEnumerable<T> items);
    void RemoveRange(IEnumerable<T> items);
}
```

Note: `SuspendNotifications()` comes from `IBindableObject`, which `IBindableCollection<T>` inherits.

### AddRange

Add multiple items efficiently:

```csharp
var newItems = new List<OrderViewModel>
{
    new OrderViewModel { Id = 1 },
    new OrderViewModel { Id = 2 },
    new OrderViewModel { Id = 3 }
};

Orders.AddRange(newItems);
```

### RemoveRange

Remove multiple items efficiently:

```csharp
var completedOrders = Orders.Where(o => o.IsComplete).ToList();
Orders.RemoveRange(completedOrders);
```

## Example: Master-Detail Pattern

A common pattern using `BindableCollection<T>`:

```csharp
public class ContactsViewModel : BindableObject
{
    private ContactViewModel? _selectedContact;

    public BindableCollection<ContactViewModel> Contacts { get; } = new();

    public ContactViewModel? SelectedContact
    {
        get => _selectedContact;
        set => SetProperty(ref _selectedContact, value);
    }

    public async Task InitializeAsync()
    {
        var contacts = await _contactService.GetAllAsync();
        
        using (Contacts.SuspendNotifications())
        {
            Contacts.Clear();
            foreach (var contact in contacts)
            {
                Contacts.Add(new ContactViewModel(contact));
            }
        }

        // Select first contact
        SelectedContact = Contacts.FirstOrDefault();
    }

    public void DeleteSelected()
    {
        if (SelectedContact is not null)
        {
            var index = Contacts.IndexOf(SelectedContact);
            Contacts.Remove(SelectedContact);
            
            // Select adjacent item
            if (Contacts.Count > 0)
            {
                SelectedContact = Contacts[Math.Min(index, Contacts.Count - 1)];
            }
        }
    }
}
```

## XAML Binding

Bind to `BindableCollection<T>` like any other collection:

### WPF

```xml
<ListBox ItemsSource="{Binding Contacts}" 
         SelectedItem="{Binding SelectedContact}">
    <ListBox.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Name}"/>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

### Avalonia

```xml
<ListBox ItemsSource="{Binding Contacts}" 
         SelectedItem="{Binding SelectedContact}">
    <ListBox.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Name}"/>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

### WinUI

```xml
<ListView ItemsSource="{x:Bind ViewModel.Contacts}" 
          SelectedItem="{x:Bind ViewModel.SelectedContact, Mode=TwoWay}">
    <ListView.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Name}"/>
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>
```

## Thread Safety

Like `ObservableCollection<T>`, `BindableCollection<T>` is not thread-safe. If you need to modify the collection from a background thread, marshal the call to the UI thread:

```csharp
// Using the dispatcher
await _dispatcher.InvokeAsync(() => Items.Add(newItem));
```

See [UI Thread Dispatching](dispatching.md) for more information.

## Performance Tips

1. **Use SuspendNotifications for bulk operations** - Prevents UI updates for each individual change
2. **Use AddRange/RemoveRange** - More efficient than multiple Add/Remove calls
3. **Consider virtualization** - For large collections, use virtualized list controls
4. **Clear before repopulating** - When replacing all items, clear first within suspended notifications

## See Also

- [BindableObject](bindable-object.md) - Base class for view models
- [Screens, Conductors and Composition](composition.md) - Managing collections of screens
- [Commands](commands.md) - Command infrastructure
