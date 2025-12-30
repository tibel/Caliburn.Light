# Quick Start

This guide will help you get up and running with Caliburn.Light as quickly as possible.

## Prerequisites

- Visual Studio 2022 or later
- .NET 8.0 SDK or later
- For WinUI: Windows App SDK

## Creating a New Project

Choose the platform you want to target:

- [WPF Quick Start](#wpf-quick-start)
- [WinUI Quick Start](#winui-quick-start)
- [Avalonia Quick Start](#avalonia-quick-start)

---

## WPF Quick Start

### Step 1: Create a New WPF Project

1. Open Visual Studio
2. Create a new **WPF Application** project targeting .NET 8.0+
3. Name it something like `MyApp`

### Step 2: Install NuGet Packages

Install the following NuGet packages:

```
Install-Package Caliburn.Light.WPF
Install-Package Microsoft.Extensions.DependencyInjection
```

### Step 3: Delete MainWindow

Delete `MainWindow.xaml` and `MainWindow.xaml.cs` - Caliburn.Light will handle window creation.

### Step 4: Create Your First ViewModel

Create a new class called `ShellViewModel.cs`:

```csharp
using Caliburn.Light;
using System.Windows.Input;

namespace MyApp;

public class ShellViewModel : BindableObject
{
    private string _greeting = "Hello, Caliburn.Light!";

    public ShellViewModel()
    {
        SayHelloCommand = DelegateCommandBuilder.NoParameter()
            .OnExecute(() => Greeting = "Hello from the ViewModel!")
            .Build();
    }

    public string Greeting
    {
        get => _greeting;
        set => SetProperty(ref _greeting, value);
    }

    public ICommand SayHelloCommand { get; }
}
```

### Step 5: Create the View

Create a new **Window** called `ShellView.xaml`:

```xml
<Window x:Class="MyApp.ShellView"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="My First Caliburn.Light App" 
        Height="200" Width="400">
    <StackPanel Margin="20">
        <TextBlock Text="{Binding Greeting}" FontSize="24" Margin="0,0,0,20"/>
        <Button Content="Say Hello" Command="{Binding SayHelloCommand}" Padding="10,5"/>
    </StackPanel>
</Window>
```

The code-behind (`ShellView.xaml.cs`) should be minimal:

```csharp
using System.Windows;

namespace MyApp;

public partial class ShellView : Window
{
    public ShellView()
    {
        InitializeComponent();
    }
}
```

### Step 6: Configure the Application

Update `App.xaml.cs`:

```csharp
using Caliburn.Light;
using Caliburn.Light.WPF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Windows;

namespace MyApp;

public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        // Register Caliburn.Light services
        services.AddSingleton<IWindowManager, WindowManager>();
        services.AddSingleton<IEventAggregator, EventAggregator>();
        services.AddSingleton<IViewModelLocator, ViewModelLocator>();
        services.AddTransient(sp => sp.GetRequiredService<IOptions<ViewModelLocatorConfiguration>>().Value);

        // Register view-viewmodel mapping
        services.Configure<ViewModelLocatorConfiguration>(config => 
            config.AddMapping<ShellView, ShellViewModel>());

        // Register view and viewmodel
        services.AddTransient<ShellView>();
        services.AddTransient<ShellViewModel>();

        _serviceProvider = services.BuildServiceProvider();

        // Show the main window
        _serviceProvider.GetRequiredService<IWindowManager>()
            .ShowWindow(_serviceProvider.GetRequiredService<ShellViewModel>());
    }
}
```

### Step 7: Update App.xaml

Remove the `StartupUri` attribute from `App.xaml`:

```xml
<Application x:Class="MyApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources>
    </Application.Resources>
</Application>
```

### Step 8: Run the Application

Press F5 to run. You should see a window with "Hello, Caliburn.Light!" and a button that changes the text when clicked.

**Congratulations!** You've created your first Caliburn.Light application!

---

## WinUI Quick Start

### Step 1: Create a New WinUI Project

1. Open Visual Studio
2. Create a new **Blank App, Packaged (WinUI 3 in Desktop)** project
3. Name it something like `MyApp`

### Step 2: Install NuGet Packages

```
Install-Package Caliburn.Light.WinUI
Install-Package Microsoft.Extensions.DependencyInjection
```

### Step 3: Create Your First ViewModel

Create `ShellViewModel.cs`:

```csharp
using Caliburn.Light;
using System.Windows.Input;

namespace MyApp;

public class ShellViewModel : BindableObject
{
    private string _greeting = "Hello, Caliburn.Light!";

    public ShellViewModel()
    {
        SayHelloCommand = DelegateCommandBuilder.NoParameter()
            .OnExecute(() => Greeting = "Hello from the ViewModel!")
            .Build();
    }

    public string Greeting
    {
        get => _greeting;
        set => SetProperty(ref _greeting, value);
    }

    public ICommand SayHelloCommand { get; }
}
```

### Step 4: Create the View

Create `ShellView.xaml`:

```xml
<Window x:Class="MyApp.ShellView"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="My First Caliburn.Light App">
    <StackPanel Margin="20" VerticalAlignment="Center" HorizontalAlignment="Center">
        <TextBlock Text="{x:Bind ViewModel.Greeting, Mode=OneWay}" FontSize="24" Margin="0,0,0,20"/>
        <Button Content="Say Hello" Command="{x:Bind ViewModel.SayHelloCommand}" Padding="10,5"/>
    </StackPanel>
</Window>
```

Code-behind (`ShellView.xaml.cs`):

```csharp
using Microsoft.UI.Xaml;

namespace MyApp;

public sealed partial class ShellView : Window
{
    public ShellView()
    {
        InitializeComponent();
    }

    public ShellViewModel? ViewModel => Content?.DataContext as ShellViewModel;
}
```

### Step 5: Configure the Application

Update `App.xaml.cs`:

```csharp
using Caliburn.Light;
using Caliburn.Light.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml;

namespace MyApp;

public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var services = new ServiceCollection();

        // Register Caliburn.Light services
        services.AddSingleton<IWindowManager, WindowManager>();
        services.AddSingleton<IEventAggregator, EventAggregator>();
        services.AddSingleton<IViewModelLocator, ViewModelLocator>();
        services.AddTransient(sp => sp.GetRequiredService<IOptions<ViewModelLocatorConfiguration>>().Value);

        // Register view-viewmodel mapping
        services.Configure<ViewModelLocatorConfiguration>(config => 
            config.AddMapping<ShellView, ShellViewModel>());

        // Register view and viewmodel
        services.AddTransient<ShellView>();
        services.AddTransient<ShellViewModel>();

        _serviceProvider = services.BuildServiceProvider();

        _serviceProvider.GetRequiredService<IWindowManager>()
            .ShowWindow(_serviceProvider.GetRequiredService<ShellViewModel>());
    }
}
```

---

## Avalonia Quick Start

### Step 1: Create a New Avalonia Project

1. Install the Avalonia templates: `dotnet new install Avalonia.Templates`
2. Create a new project: `dotnet new avalonia.app -o MyApp`

### Step 2: Install NuGet Packages

```
dotnet add package Caliburn.Light.Avalonia
dotnet add package Microsoft.Extensions.DependencyInjection
```

### Step 3: Create Your First ViewModel

Create `ShellViewModel.cs`:

```csharp
using Caliburn.Light;
using System.Windows.Input;

namespace MyApp;

public class ShellViewModel : BindableObject
{
    private string _greeting = "Hello, Caliburn.Light!";

    public ShellViewModel()
    {
        SayHelloCommand = DelegateCommandBuilder.NoParameter()
            .OnExecute(() => Greeting = "Hello from the ViewModel!")
            .Build();
    }

    public string Greeting
    {
        get => _greeting;
        set => SetProperty(ref _greeting, value);
    }

    public ICommand SayHelloCommand { get; }
}
```

### Step 4: Create the View

Create `ShellView.axaml`:

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        x:Class="MyApp.ShellView"
        Title="My First Caliburn.Light App"
        Width="400" Height="200">
    <StackPanel Margin="20" VerticalAlignment="Center" HorizontalAlignment="Center">
        <TextBlock Text="{Binding Greeting}" FontSize="24" Margin="0,0,0,20"/>
        <Button Content="Say Hello" Command="{Binding SayHelloCommand}" Padding="10,5"/>
    </StackPanel>
</Window>
```

### Step 5: Configure the Application

Update `App.axaml.cs`:

```csharp
using Avalonia;
using Avalonia.Markup.Xaml;
using Caliburn.Light;
using Caliburn.Light.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MyApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();

        // Register Caliburn.Light services
        services.AddSingleton<IWindowManager, WindowManager>();
        services.AddSingleton<IEventAggregator, EventAggregator>();
        services.AddSingleton<IViewModelLocator, ViewModelLocator>();
        services.AddTransient(sp => sp.GetRequiredService<IOptions<ViewModelLocatorConfiguration>>().Value);

        // Register view-viewmodel mapping
        services.Configure<ViewModelLocatorConfiguration>(config => 
            config.AddMapping<ShellView, ShellViewModel>());

        // Register view and viewmodel
        services.AddTransient<ShellView>();
        services.AddTransient<ShellViewModel>();

        var serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetRequiredService<IWindowManager>()
            .ShowWindow(serviceProvider.GetRequiredService<ShellViewModel>());

        base.OnFrameworkInitializationCompleted();
    }
}
```

---

## Next Steps

Now that you have a working application, explore these topics:

- [Commands](commands.md) - Learn about the command infrastructure
- [Screens, Conductors and Composition](composition.md) - Build complex view hierarchies
- [Validation](validation.md) - Add validation to your view models
- [The Event Aggregator](event-aggregator.md) - Implement loosely-coupled communication
- [The Window Manager](window-manager.md) - Show windows and dialogs

Check out the complete sample applications in the repository:

- [WPF Gallery](https://github.com/tibel/Caliburn.Light/tree/master/samples/Caliburn.Light.Gallery.WPF)
- [WinUI Gallery](https://github.com/tibel/Caliburn.Light/tree/master/samples/Caliburn.Light.Gallery.WinUI)
- [Avalonia Gallery](https://github.com/tibel/Caliburn.Light/tree/master/samples/Caliburn.Light.Gallery.Avalonia)
