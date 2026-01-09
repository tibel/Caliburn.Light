# Basic Configuration

Configuring Caliburn.Light is straightforward when using dependency injection.

## Attached Properties

### ViewModel-First
- `View.Create`  
  Set to `True` on a ContentControl to locate the view for the DataContext and inject it.
- `View.Context`  
  To support multiple views over the same ViewModel, set this property on the injection site.

### View-First
- `View.Bind`  
  Set to `True` on a view inside a DataTemplate to attach the view to the view-model.

## Samples

The gallery samples demonstrate the recommended configuration approach using `Microsoft.Extensions.DependencyInjection`:

- [WPF Gallery](https://github.com/tibel/Caliburn.Light/tree/main/samples/Caliburn.Light.Gallery.WPF)
- [WinUI Gallery](https://github.com/tibel/Caliburn.Light/tree/main/samples/Caliburn.Light.Gallery.WinUI)
- [Avalonia Gallery](https://github.com/tibel/Caliburn.Light/tree/main/samples/Caliburn.Light.Gallery.Avalonia)

### Basic Setup Pattern

All platforms follow a similar configuration pattern:

```csharp
using Caliburn.Light;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

var services = new ServiceCollection();

// Register core Caliburn.Light services
services.AddSingleton<IWindowManager, WindowManager>();
services.AddSingleton<IEventAggregator, EventAggregator>();
services.AddSingleton<IViewModelLocator, ViewModelLocator>();
services.AddTransient(sp => sp.GetRequiredService<IOptions<ViewModelLocatorConfiguration>>().Value);

// Register view-viewmodel mappings
services.Configure<ViewModelLocatorConfiguration>(config =>
{
    config.AddMapping<ShellView, ShellViewModel>();
    config.AddMapping<HomeView, HomeViewModel>();
});

// Register views and view models
services.AddTransient<ShellView>();
services.AddTransient<ShellViewModel>();
services.AddTransient<HomeView>();
services.AddTransient<HomeViewModel>();

var serviceProvider = services.BuildServiceProvider();

// Show the main window
serviceProvider.GetRequiredService<IWindowManager>()
    .ShowWindow(serviceProvider.GetRequiredService<ShellViewModel>());
```

> **Tip:** The gallery samples include helper extension methods like `AddCaliburnLight()` and `AddView<>()` 
> that wrap the registration pattern above. You can create similar helpers in your own application
> to reduce boilerplate. See `ServiceCollectionExtensions.cs` in the sample projects.
