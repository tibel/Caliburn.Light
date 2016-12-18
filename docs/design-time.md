# Design Time support

Enabling Caliburn.Light inside the Visual Studio designer (or Blend) is quite easy.

You have to set a Desinger-DataContext:

``` csharp
<Window 
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:vm="clr-namespace:CaliburnDesignTimeData.ViewModels"
    xmlns:cal="clr-namespace:Caliburn.Light;assembly=Caliburn.Light.Platform"
    mc:Ignorable="d" 
    d:DataContext="{d:DesignInstance Type=vm:MainPageViewModel, IsDesignTimeCreatable=True}">
```
