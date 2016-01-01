# Introduction

### Call Method Action
The Action mechanism allows you to “bind” UI triggers, such as a Button’s “Click” event, to methods on your View-Model or Presenter. The mechanism allows for passing parameters to the method as well. Parameters can be databound to other FrameworkElements or can pass special values, such as the DataContext or EventArgs. All parameters are automatically type converted to the method’s signature. In addition to invocation, the mechanism supports a “CanExecute” guard. If the Action has a corresponding Property or Method with the same name, but preceded by the word “Can,” the invocation of the Action will be blocked and the UI will be disabled. Actions also support async methods (methods that return Task).

**Note**: It is recommended to use the command pattern instead as no reflection is required for binding the View Model method.

### Screens and Conductors
The Screen, ScreenConductor and ScreenCollection patterns enable model-based tracking of the active or current item, enforcing of screen lifecycles and elegant shutdown or shutdown cancellation in an application. Caliburn.Light’s implementation of these patterns is an evolution of the one found in Caliburn and supports conducting any type of class, not just implementations of IScreen. You’ll find that Caliburn.Light’s screen implementation is quite thorough and even handles asynchronous shutdown scenarios with ease.

### Event Aggregator
The integrated EventAggregator is simple yet powerful. The aggregator follows a bus-style pub/sub model. You register a message handler with the aggregator, and it sends you any messages you are interested in.  References to handlers are held weakly. Even polymorphic subscriptions are supported.

### Coroutines
Any action can optionally choose to return ICoTask or IEnumerable<ICoTask>, opening the door to a powerful approach to handling asynchronous programming. Furthermore, implementations of ICoTask have access to an execution context which tells them what FrameworkElement triggered the action and what the Target is for that action. Such contextual information enables a loosely-coupled, declarative mechanism by which a Presenter or View-Model can communicate with its View without needing to hold a reference to it at any time.

**Note**: Coroutines were superseeded by async/await where the execution context can be added as a method parameter.

### View Locator
For every ViewModel in your application, Caliburn.Light has a basic strategy for locating the View that should render it. We do this based on naming conventions. For example, if your VM is called MyApplication.ViewModels.ShellViewModel, we will look for MyApplication.Views.ShellView. Additionally, we support multiple views over the same View-Model by attaching a View.Context in Xaml. So, given the same model as above, but with a View.Context=”Master” we would search for MyApplication.Views.Shell.Master. Of course, all this is customizable.

**Note**: It is recommended to use the registration based strategy as it is more explicit and refactoring safe.

### View Model Locator
Though Caliburn.Light favors the ViewModel-First approach, we also support View-First by providing a ViewModelLocator with the same mapping semantics as the ViewLocator.

### Window Manager
This service provides a View-Model-centric way of displaying Windows. Simply pass it an instance of the View Model and it will locate the view, wrap it in a Window if necessary, apply all conventions you have configured and show the window.

### BindableObject and BindableCollection
What self respecting XAML framework could go without a base implementation of INotifyPropertyChanged? The Caliburn.Light implementation enables string-based (use `nameof` operator) change notification.  BindableCollection<T> is a simple collection that inherits from ObservableCollection<T> and allows to suspend change notifications.

**Note**: It is not ensured that the events are raised on the UI thread.

### Bootstrapper
What’s required to configure this framework and get it up and running? Not much. Simply inherit from BootstrapperBase and add an instance of your custom bootstrapper to the Application’s ResourceDictionary. Done. If you want, you can override a few methods to plug in your own IoC container, declare what assemblies should be inspected for Views, etc. It’s pretty simple.

**Note**: Only for WPF

### CaliburnApplication
For Windows Store Apps instead of a custom bootstrapper your App has to inherit from CaliburnApplication to initialize the framework. This is due to the different nature of the WinRT XAML stack, but nearly as simple as using the bootstrapper in WPF.

**Note**: Only for WinRT

### Logging
Caliburn.Light implements a basic logging abstraction. This is important in any serious framework so you can plug in your logging framework of choice. All the most important parts of the framework are covered with logging. Want to know what the framework is doing or not doing? Turn on logging. Want to know what actions are being executed? Turn on logging. Want to know what events are being published? Turn on logging. You get the picture.

### MVVM and MVP
In case it isn’t obvious, this framework enables MVVM. MVVM isn’t hard on its own, but Caliburn.Light strives to go beyond simply getting it done. We want to write elegant, testable, maintainable and extensible presentation layer code...and we want it to be easy to do so. That’s what this is about. If you prefer using Supervising Controller and PassiveView to MVVM, go right ahead. You’ll find that Caliburn.Light can help you a lot, particularly its Screen/ScreenConductor implementation. If you are not interested in any of the goals I just mentioned, you’d best move along. This framework isn’t for you.
