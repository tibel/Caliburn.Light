---
layout: page
title: Nuget Package Installation
---

[NuGet][nuget] is a Visual Studio extension that makes it easy to add, remove, and update libraries and tools in Visual Studio projects that use the .NET Framework. Caliburn.Micro is proud to support the NuGet Package Manager.

### Installing the packages

With the latest version of Nuget installed, open the Package Manager Console and type:

```
PM> Install-Package Caliburn.Micro.Start
```

### After installation

#### Clean out your App.xaml.cs file. It should look like this:

``` csharp
namespace YourNamespace
{
    using System.Windows;

    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }
    }
}
```

#### Add the AppBoostrapper to your App.xaml's Resources section.

##### Silverlight

``` xml
<Application xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:YourNamespace"
             x:Class="YourNamespace.App">
    <Application.Resources>
        <local:AppBootstrapper x:Key="bootstrapper" />
    </Application.Resources>
</Application>
```

**Note**: You no longer need the default MainPage.xaml.

##### WPF

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

**Note**: Make sure to remove the StartupUri value. Caliburn.Micro will be handling the main window creation for you. As a result, you no longer need the MainWindow.xaml either.

##### Windows Phone

``` xml
<Application xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:YourNamespace"
             x:Class="YourNamespace.App">
    <Application.Resources>
        <local:AppBootstrapper x:Key="bootstrapper" />
    </Application.Resources>
</Application>
```

**Note**: If you move your MainPage.xaml into a Views folder, don't forget to update your WMAppManfiest.xml to point to the new URI, as follows:

``` xml
<Tasks>
   <DefaultTask Name="_default" NavigationPage="/Views/MainPage.xaml" />
</Tasks>
```

##### WinRT
For WinRT, the process of getting started is unfortunately quite different from the other platforms, due to significant design differences in the Windows Xaml APIs. For detailed instructions please see [Working with WinRT](./windows-runtime).

[nuget]: http://www.nuget.org/
