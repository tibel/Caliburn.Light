# Basic Configuration

Configuring Caliburn.Light is quite easy, when you know what steps you need.


## Attached Properties

### View-First
- `Bind.Model` (use on root nodes like Window/UserControl/Page)  
  Sets the DataContext and attaches the view to the view-model.
- `Bind.ModelWithoutContext` (use inside of DataTemplate)  
  Attaches the view to the view-model only.

### ViewModel-First
- `View.Model`  
  Locates the view for the specified VM instance and injects it at the content site.
  Sets the VM as DataContext on the view.
- `View.Context`  
  To support multiple views over the same ViewModel, set this property on the injection site.

## Samples

### WPF

For a basic WPF sample see [SimpleMDI]({{site.github.repository_url}}/tree/master/samples/Demo.SimpleMDI).

### Windows Runtime

For a WinRT sample see [HelloEventAggregator]({{site.github.repository_url}}/tree/master/samples/Demo.HelloEventAggregator).
