![Logo](./logo.png?raw=true)
# Caliburn.Light

Caliburn.Light is a fork of [Caliburn.Micro](http://caliburnmicro.com/) that
- is modular/magic-free (does not include conventions)
- does not switch to UI thread automatically for everything
- integrates some ideas of [Prism](http://msdn.microsoft.com/en-us/library/ff648465.aspx) and [MVVMLight](http://www.mvvmlight.net/) 
- uses weak events (see [Weakly](https://github.com/tibel/Weakly))
- supports `ICommand` out-of-the-box



## Install
Caliburn.Light is available through NuGet:

**Install-Package** [Caliburn.Light](https://www.nuget.org/packages/Caliburn.Light/)



## Attached Properties

### View-First
- `Bind.Model` (use on root nodes like Window/UserControl/Page)  
  Sets the DataContext and attaches the view to the view-model.
- `Bind.ModelWithoutContext` (use inside of DataTemplate)  
  Attaches the view to the view-model only.
- `Bind.AtDesignTime`
  To enable view-model binding inside the Visual Studio designer or Blend.

### ViewModel-First
- `View.Model`  
  Locates the view for the specified VM instance and injects it at the content site.
  Sets the VM as DataContext on the view.
- `View.Context`  
  To support multiple views over the same ViewModel, set this property on the injection site.



## Behaviors
For attaching an action to a view element
- `CallMethodAction`  
  Calls a method on the specified object (supports parameters and coroutines).
  It also maintains the Enabled state of the target element based on a guard method/property.
- `InvokeCommandAction`  
  Executes a specified ICommand when invoked.
  It also maintains the Enabled state of the target element based on the CanExecute method of the command.
