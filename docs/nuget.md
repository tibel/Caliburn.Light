# NuGet Package Installation

[NuGet](http://www.nuget.org/) is a Visual Studio extension that makes it easy to add, remove, and update libraries and tools in Visual Studio projects that use the .NET Framework.

### Installing the package

With the latest version of NuGet installed, open the Package Manager Console and type:

```
PM> Install-Package Caliburn.Light
```

### After installation

Now it is time to wire up the framework.

#### WPF

Create your application bootstrapper.

``` csharp
using Caliburn.Light;
using System.Windows;

namespace YourNamespace
{
  public class AppBootstrapper : BootstrapperBase
  {
    private SimpleContainer _container;
    
    public AppBootstrapper()
    {
      Initialize();
    }
    
    protected override void Configure()
    {
      _container = new SimpleContainer();
      IoC.Initialize(_container);
      
      _container.RegisterSingleton<IWindowManager, WindowManager>();
      _container.RegisterSingleton<IEventAggregator, EventAggregator>();
      _container.RegisterSingleton<IViewModelLocator, ViewModelLocator>();
      _container.RegisterSingleton<IViewModelBinder, ViewModelBinder>();
      
      var typeResolver = new ViewModelTypeResolver();
      _container.RegisterInstance<IViewModelTypeResolver>(typeResolver);
      
      _container.RegisterPerRequest<ShellViewModel>();
      typeResolver.AddMapping<ShellView, ShellViewModel>();
    }
    
    protected override void OnStartup(StartupEventArgs e)
    {
      DisplayRootViewFor<ShellViewModel>();
    }
  }
}
```

Add your AppBoostrapper to your App.xaml's Resources section.

``` xml
<Application xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:YourNamespace"
             x:Class="YourNamespace.App">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary>
                    <local:AppBootstrapper x:Key="bootstrapper" />
                </ResourceDictionary>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

**Note**: Make sure to remove the StartupUri value. Caliburn.Light will be handling the main window creation for you.

#### Windows Store App

For WinRT, the process of getting started is unfortunately quite different, due to significant design differences in the Windows XAML APIs.
