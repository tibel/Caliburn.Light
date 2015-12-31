# Design Time support

Enabling Caliburn.Micro inside the Visual Studio designer (or Blend) is quite easy.

You have to set a Desinger-DataContext and tell CM to enable its magic in your view XAML:

``` csharp
<Window 
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:vm="clr-namespace:CaliburnDesignTimeData.ViewModels"
    xmlns:cal="clr-namespace:Caliburn.Micro;assembly=Caliburn.Micro.Platform"
    mc:Ignorable="d" 
    d:DataContext="{d:DesignInstance Type=vm:MainPageViewModel, IsDesignTimeCreatable=True}"
    cal:Bind.AtDesignTime="True">
```

For this to work, the ViewModel must have a default constructor. If this isn't suitable, you can also use a ViewModelLocator for your design-time ViewModel creation.

### Issues

It seems that VS2010 has an issue in the WP7 designer and an exception in CM ConventionManager is thrown. You can workaround this by overriding ApplyValidation in your bootstrapper:

``` csharp
ConventionManager.ApplyValidation = (binding, viewModelType, property) => {
        if (typeof(INotifyDataErrorInfo).IsAssignableFrom(viewModelType)) {
            binding.ValidatesOnNotifyDataErrors = true;
            binding.ValidatesOnExceptions = true;
        }
    };
```