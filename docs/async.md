# Async (Task Support)

Caliburn.Light uses Task and async/await for all asynchronous operations.

## Screen Lifecycle

All lifecycle methods in Caliburn.Light are async:

```csharp
public class Screen : ViewAware, IActivatable, ICloseGuard
{
    // Called once when the screen is first activated
    protected virtual Task OnInitializeAsync() => Task.CompletedTask;

    // Called every time the screen is activated
    protected virtual Task OnActivateAsync() => Task.CompletedTask;

    // Called every time the screen is deactivated
    protected virtual Task OnDeactivateAsync(bool close) => Task.CompletedTask;

    // Called to determine if the screen can be closed
    public virtual Task<bool> CanCloseAsync() => Task.FromResult(true);
}
```

## Example Usage

```csharp
public class MyViewModel : Screen
{
    private readonly IDataService _dataService;

    public MyViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    protected override async Task OnInitializeAsync()
    {
        // Load data when the screen is first activated
        Data = await _dataService.LoadDataAsync();
    }

    protected override async Task OnActivateAsync()
    {
        // Refresh data every time the screen is activated
        await _dataService.RefreshAsync();
    }

    protected override async Task OnDeactivateAsync(bool close)
    {
        if (close)
        {
            // Save data when the screen is closed
            await _dataService.SaveAsync();
        }
    }

    public override async Task<bool> CanCloseAsync()
    {
        if (HasUnsavedChanges)
        {
            // Ask user if they want to save
            return await PromptSaveChangesAsync();
        }
        return true;
    }
}
```

## Conductor Operations

All conductor operations are also async:

```csharp
public interface IConductor
{
    Task ActivateItemAsync(object item);
    Task DeactivateItemAsync(object item, bool close);
}
```

Example:

```csharp
public class ShellViewModel : Conductor<TabViewModel>.Collection.OneActive
{
    public async Task OpenTabAsync()
    {
        var newTab = new TabViewModel();
        await ActivateItemAsync(newTab);
    }

    public async Task CloseActiveTabAsync()
    {
        if (ActiveItem != null)
        {
            await DeactivateItemAsync(ActiveItem, close: true);
        }
    }
}
```

## Window Manager

The Window Manager also uses async for modal dialogs:

```csharp
// Show a modal dialog and wait for it to close
await windowManager.ShowDialog(viewModel, ownerViewModel);

// Show a message box and get the result
var result = await windowManager.ShowMessageBoxDialog(options, ownerViewModel);

// Show file dialogs
var files = await windowManager.ShowOpenFileDialog(options, ownerViewModel);
```
