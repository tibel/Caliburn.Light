# NuGet Package Installation

[NuGet](http://www.nuget.org/) is a Visual Studio extension that makes it easy to add, remove, and update libraries and tools in Visual Studio projects that use the .NET Framework.

### Installing the packages

Caliburn.Light is modular. Install the package for your target platform:

**For WPF:**
```
PM> Install-Package Caliburn.Light.WPF
```

**For WinUI:**
```
PM> Install-Package Caliburn.Light.WinUI
```

**For Avalonia:**
```
PM> Install-Package Caliburn.Light.Avalonia
```

The platform packages automatically include `Caliburn.Light.Core` as a dependency.

### After installation

Now it is time to wire up the framework. Caliburn.Light is designed to work with any dependency injection container. The examples below use `Microsoft.Extensions.DependencyInjection`.

#### WPF

```csharp
using Caliburn.Light;
using Caliburn.Light.WPF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Windows;

namespace YourNamespace;

public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

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

    protected override void OnStartup(StartupEventArgs e)
    {
        Configure();

        base.OnStartup(e);

        _serviceProvider!.GetRequiredService<IWindowManager>()
            .ShowWindow(_serviceProvider.GetRequiredService<ShellViewModel>());
    }
}
```

**Note**: Make sure to remove the StartupUri value from App.xaml. Caliburn.Light will be handling the main window creation for you.

#### WinUI

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

#### Avalonia

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

    public override void OnFrameworkInitializationCompleted()
    {
        var serviceProvider = Configure();

        serviceProvider.GetRequiredService<IWindowManager>()
            .ShowWindow(serviceProvider.GetRequiredService<ShellViewModel>());

        base.OnFrameworkInitializationCompleted();
    }
}
```
