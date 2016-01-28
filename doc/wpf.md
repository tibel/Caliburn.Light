# WPF specifics

Use following behaviors for attaching an action to a view element
- `CallMethodAction`  
  Calls a method on the specified object (supports parameters and coroutines).
  It also maintains the Enabled state of the target element based on a guard method/property.
- `InvokeCommandAction`  
  Executes a specified ICommand when invoked.
  It also maintains the Enabled state of the target element based on the CanExecute method of the command.
