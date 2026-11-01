# BindableCollection

`BindableCollection<T>` is an observable collection for MVVM applications. It implements `IBindableCollection<T>` and `IReadOnlyBindableCollection<T>`.

## Overview

`BindableCollection<T>` provides:

- The generic list operations and collection change notifications
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
        // Nothing was raised while suspended - refresh once so the UI catches up
        Products.Refresh();
    }
}
```

### How SuspendNotifications Works

- When suspended, `CollectionChanged` and `PropertyChanged` events are not raised
- Nothing is replayed when the suspension ends (via `Dispose`): notifications resume, but no event is raised for the changes made while suspended
- Multiple levels of suspension are supported (reference counted)
- Always use a `using` statement to ensure notifications are properly resumed
- Use `AddRange()`, `InsertRange()`, or either `RemoveRange()` overload for bulk changes; each raises a single `Reset` notification when it changes the collection

## IBindableCollection Interface

`BindableCollection<T>` implements `IBindableCollection<T>`:

```csharp
public interface IBindableCollection<T> : IList<T>, INotifyCollectionChanged, IBindableObject
{
    void Move(int oldIndex, int newIndex);
    void AddRange(IEnumerable<T> items);
    void InsertRange(int index, IEnumerable<T> items);
    int RemoveAll(Predicate<T> match);
    int RemoveRange(IEnumerable<T> items);
    void RemoveRange(int index, int count);
}
```

Note: `SuspendNotifications()` comes from `IBindableObject`, which `IBindableCollection<T>` inherits.

The collection also implements `IReadOnlyBindableCollection<T>`.

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

Insert multiple items at a specific position:

```csharp
Orders.InsertRange(0, newItems);
```

### RemoveRange

Remove multiple items efficiently:

```csharp
var completedOrders = Orders.Where(o => o.IsComplete).ToList();
Orders.RemoveRange(completedOrders);
```

Remove every item that matches a predicate:

```csharp
var removedCount = Orders.RemoveAll(order => order.IsComplete);
```

Range operations and `RemoveAll` raise a single `Reset` notification when they change the collection. Empty additions and removals with no matching items do not raise change notifications. Clearing an already empty collection or moving an item to its current index is also a no-op. Call `Refresh()` when bindings should be notified even though the collection contents have not changed.

If a range source is a lazy query over the same collection, materialize it before calling `AddRange`, `InsertRange`, or `RemoveRange` (for example, with `ToList()`). This prevents the operation from modifying the collection while that query is being enumerated.

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

        // Raise a single Reset now that the bulk update is done
        Contacts.Refresh();

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

`BindableCollection<T>` is not thread-safe. If you need to modify the collection from a background thread, marshal the call to the UI thread:

```csharp
// Switch to the UI thread associated with the attached view
await ViewHelper.SwitchToViewAsync(view);
Items.Add(newItem);
```

See [UI Thread Dispatching](dispatching.md) for more information.

## Performance Tips

1. **Use SuspendNotifications for bulk operations** - Prevents UI updates for each individual change
2. **Use AddRange/RemoveRange** - More efficient than multiple Add/Remove calls
3. **Consider virtualization** - For large collections, use virtualized list controls
4. **Clear before repopulating** - When replacing all items, clear first within suspended notifications, then call `Refresh()` after resuming (or use `AddRange()` outside suspension) so the UI sees the result

## See Also

- [BindableObject](bindable-object.md) - Base class for view models
- [Screens, Conductors and Composition](composition.md) - Managing collections of screens
- [Commands](commands.md) - Command infrastructure
