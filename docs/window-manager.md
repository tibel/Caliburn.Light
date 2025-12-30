# The Window Manager

The Window Manager provides a View-Model-centric way of displaying Windows, dialogs, and file pickers. It locates the view for a given view model, wraps it in a Window if necessary, and shows it.

## Overview

Each platform has its own `IWindowManager` implementation:

- **WPF**: `Caliburn.Light.WPF.IWindowManager`
- **WinUI**: `Caliburn.Light.WinUI.IWindowManager`
- **Avalonia**: `Caliburn.Light.Avalonia.IWindowManager`

## Common Methods

All platforms support these core methods:

```csharp
// Show a non-modal window
void ShowWindow(object viewModel, string? context = null);

// Show a modal dialog
Task ShowDialog(object viewModel, object ownerViewModel, string? context = null);

// Bring a window to the foreground
bool Activate(object viewModel);
```

## WPF-Specific Features

### Message Box

```csharp
Task<MessageBoxResult> ShowMessageBoxDialog(MessageBoxDialogOptions options, object ownerViewModel);
```

Example:

```csharp
var options = new MessageBoxDialogOptions
{
    Text = "Are you sure you want to delete this item?",
    Caption = "Confirm Delete",
    Button = MessageBoxButton.YesNo,
    Icon = MessageBoxImage.Question
};

var result = await _windowManager.ShowMessageBoxDialog(options, this);
if (result == MessageBoxResult.Yes)
{
    // Delete the item
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

## WinUI-Specific Features

### Content Dialog

```csharp
Task<ContentDialogResult> ShowContentDialog(object viewModel, object ownerViewModel, string? context = null);
```

### File Pickers

```csharp
Task<IReadOnlyList<PickFileResult>> ShowFileOpenPickerAsync(FileOpenPickerOptions options, object ownerViewModel);
Task<PickFileResult?> ShowFileSavePickerAsync(FileSavePickerOptions options, object ownerViewModel);
Task<IReadOnlyList<PickFolderResult>> ShowFolderPickerAsync(FolderPickerOptions options, object ownerViewModel);
```

## Avalonia-Specific Features

### File Pickers

```csharp
Task<IReadOnlyList<IStorageFile>> ShowOpenFilePickerAsync(FilePickerOpenOptions options, object ownerViewModel);
Task<IStorageFile?> ShowSaveFilePickerAsync(FilePickerSaveOptions options, object ownerViewModel);
Task<IReadOnlyList<IStorageFolder>> ShowOpenFolderPickerAsync(FolderPickerOpenOptions options, object ownerViewModel);
```

## Usage Example

```csharp
public class ShellViewModel : Conductor<IScreen>.Collection.OneActive
{
    private readonly IWindowManager _windowManager;

    public ShellViewModel(IWindowManager windowManager)
    {
        _windowManager = windowManager;
    }

    public async Task OpenSettingsAsync()
    {
        var settings = new SettingsViewModel();
        await _windowManager.ShowDialog(settings, this);
    }

    public void OpenNewWindow()
    {
        var document = new DocumentViewModel();
        _windowManager.ShowWindow(document);
    }
}
```

## View Resolution

The Window Manager uses the registered `IViewModelLocator` to find the appropriate view for a view model. Make sure to register your views:

```csharp
services.Configure<ViewModelLocatorConfiguration>(config =>
{
    config.AddMapping<SettingsView, SettingsViewModel>();
    config.AddMapping<DocumentView, DocumentViewModel>();
});
```

## Context Parameter

The optional `context` parameter allows you to use different views for the same view model:

```csharp
// Use default view
_windowManager.ShowWindow(viewModel);

// Use a specific view registered with context "Compact"
_windowManager.ShowWindow(viewModel, context: "Compact");
```

Register views with context:

```csharp
services.Configure<ViewModelLocatorConfiguration>(config =>
    config.AddMapping<CompactDocumentView, DocumentViewModel>("Compact"));
