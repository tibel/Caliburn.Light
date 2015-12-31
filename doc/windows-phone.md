---
layout: page
title: Working with Windows Phone
---

**Important** - Windows Phone 8.1 is built on the Windows Runtime (WinRT) rather than Silverlight, the documentation at [Working with Windows RT (Windows 8 and Windows Phone 8.1)](./windows-runtime) applies best. If you are still using a Windows Phone 8.1 Silverlight project then follow along here.

In version 1.0 we had pretty good support for building apps for WP7, but in v1.1 we’ve taken things up a notch. Let’s look at the same HelloWP7 sample that we did previously, but see how it’s been updated to take advantage of our improved tombstoning, launcher/chooser support and strongly typed navigation. You’ll also notice that the code is cleaner overall.

### Bootstrapper

Here’s the cleaned up boostrapper in v1.1.

``` csharp
public class HelloWP7Bootstrapper : PhoneBootstrapperBase {  
    PhoneContainer container;  
  
    public HelloWP7Bootstrapper() {
        Initialize();
    }

    protected override void Configure() {  
        container = new PhoneContainer(RootFrame);

        if (!Execute.InDesignMode)
            container.RegisterPhoneServices();

        container.PerRequest<MainPageViewModel>();  
        container.PerRequest<PivotPageViewModel>();  
        container.PerRequest<TabViewModel>();  
  
        AddCustomConventions();  
    }  
  
    static void AddCustomConventions() {  
        //ellided  
    }  
  
    protected override object GetInstance(Type service, string key) {  
        return container.GetInstance(service, key);  
    }  
  
    protected override IEnumerable<object> GetAllInstances(Type service) {  
        return container.GetAllInstances(service);  
    }  
  
    protected override void BuildUp(object instance) {  
        container.BuildUp(instance);  
    }  
}  
```

There are two things to notice here. First, we’ve removed all the manual Caliburn.Micro service configuration and pushed it into the SimpleContainer. That gives you one line of code to configure the framework if you are using the OOTB container. Speaking of which, we now provide the SimpleContainer officially in the Caliburn.Micro.Extensions assembly. That helps you get started faster. You can always plug your own in, of coarse. In addition to the simplified configuration, notice that the ViewModels for pages are no longer registered using a string key. For v1.1 our ViewModelLocator has been re-implemented to pull VMs from the container by Type rather than key. It now follows the exact same naming strategies as the ViewLocator (but in reverse) and even derives possible interface names so that it resolves VMs from the container correctly. This both improves the consistency of ViewModel location as well as makes the configuration simpler.

The boostrapper is added to your App.xaml as always:

``` xml
<Application x:Class="Caliburn.Micro.HelloWP7.App"  
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"  
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"  
             xmlns:local="clr-namespace:Caliburn.Micro.HelloWP7">  
    <Application.Resources>  
        <local:HelloWP7Bootstrapper x:Key="bootstrapper" />  
    </Application.Resources>  
</Application>  
```

##### Important Note About App.xaml.cs

If you create your WP7 application using a standard Visual Studio template, the generated App.xaml.cs file will have a lot of code in it. The purpose of this code is to set up the root frame for the application and make sure everything gets initialized properly. Of course, that's what the bootstrapper's job is too (and in fact it does a few things better than the out-of-the-box code in addition to configuring CM). So, you don't need both. When using CM's PhoneBootstrapper, be sure to clear out all the code from the App.xaml.cs file except for the call to InitializeComponent in the constructor.

### INavigationService

Let’s review what CM’s INavigationService does for you. First, remember that WP7 enforces a View-First approach to UI at the platform level. Like it or not, the platform is going to create pages at will and the Frame control is going to conduct your application thusly. You don’t get to control that and there are no extensibility points, unlike the Silverlight version of the navigation framework. Rather than fight this, I’m going to recommend embracing the View-First approach for Pages in WP7, but maintaining a Model-First composition strategy for the sub-components of those pages and a Model-First approach to coding against the navigation system. In order to bridge this gap, I’ve enabled the INavigationService to hook into the native navigation frame’s functionality and augment it with the following behaviors:

##### When Navigating To a Page
 - Use the new ViewModelLocator to conventionally determine the Type of the VM that should be attached to the page being navigated to. Pull that VM by Type out of the container.
 - If a VM is found, use the ViewModelBinder to connect the Page to the located ViewModel.
 - Examine the Page’s QueryString. Look for properties on the VM that match the QueryString parameters and inject them, performing the necessary type coercion.
 - If the ViewModel implements the IActivate interface, call its Activate method.

##### When Navigating Away From a Page
 - Detect whether the associated ViewModel implements the IGuardClose interface.
 - If IGuardClose is implemented and the app is not being tombstoned or closed, invoke the CanClose method and use its result to optionally cancel page navigation.
 - If the ViewModel can close and implements the IDeactivate interface, call it’s Deactivate method. Always pass “false” to indicate that the VM should deactivate, but not necessarily close. This is because the phone may be deactivating, but not actually tombstoning or closing. There’s no way to know.

The behavior of the navigation service allows the correct VM to be hooked up to the page, allows that VM to be notified that it is being navigated to (IActivate), allows it to prevent navigation away from the current page (IGuardClose) and allows it to clean up after itself on navigation away, tombstoning or normal “closing” of the application (IDeactivate). All these interfaces (and a couple more) are implemented by the Screen class. If you prefer not to inherit from Screen, you can implement any of the interfaces individually of coarse. They provide a nice View-Model-Centric, testable and predictable way of responding to navigation without needing to wire up a ton of event handlers or write important application flow logic in the page’s code-behind.

These hooks into phone navigation enable a really smooth way of interacting with the phone’s navigation lifecycle. But now that we have an improved ViewModelLocator that matches exactly the ViewLocator and works on types, we can take things further. In v1.1 we’ve introduced support for strongly-typed navigation. Here’s what the new MainPageViewModel from the sample looks like using this new feature:

``` csharp
public class MainPageViewModel {  
    readonly INavigationService navigationService;  
  
    public MainPageViewModel(INavigationService navigationService) {  
        this.navigationService = navigationService;  
    }  
  
    public void GotoPageTwo() {  
        navigationService.UriFor<PivotPageViewModel>()  
            .WithParam(x => x.NumberOfTabs, 5)  
            .Navigate();  
    }  
}  
```

This allows you to specify a ViewModel to navigate to along with the query string parameters. Since this all happens using generics and lambdas, you can never miss-type a page Uri or mess up your query strings….and refactoring will work beautifully.

For the sake of completeness, here’s the page that will be bound to MainPageViewModel:

``` xml
<phone:PhoneApplicationPage x:Class="Caliburn.Micro.HelloWP7.MainPage"  
                            xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"  
                            xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"  
                            xmlns:phone="clr-namespace:Microsoft.Phone.Controls;assembly=Microsoft.Phone"  
                            xmlns:shell="clr-namespace:Microsoft.Phone.Shell;assembly=Microsoft.Phone"  
                            xmlns:cal="clr-namespace:Caliburn.Micro;assembly=Caliburn.Micro"  
                            SupportedOrientations="Portrait"  
                            Orientation="Portrait"  
                            shell:SystemTray.IsVisible="True">  
    <Grid Background="Transparent">  
        <Grid.RowDefinitions>  
            <RowDefinition Height="Auto" />  
            <RowDefinition Height="*" />  
        </Grid.RowDefinitions>  
        <StackPanel Grid.Row="0"  
                    Margin="24,24,0,12">  
            <TextBlock Text="WP7 Caliburn.Micro"  
                       Style='{StaticResource PhoneTextNormalStyle}' />  
            <TextBlock Text='Main Page'  
                       Margin='-3,-8,0,0'  
                       Style='{StaticResource PhoneTextTitle1Style}' />  
        </StackPanel>  
  
        <Grid Grid.Row='1'>  
            <Button x:Name='GotoPageTwo'  
                    Content='Goto Page Two' />  
        </Grid>  
    </Grid>  
  
    <phone:PhoneApplicationPage.ApplicationBar>  
        <shell:ApplicationBar IsVisible='True'>  
            <shell:ApplicationBar.Buttons>  
                <cal:AppBarButton IconUri='ApplicationIcon.png'  
                                  Text='Page Two'  
                                  Message='GotoPageTwo' />  
            </shell:ApplicationBar.Buttons>  
        </shell:ApplicationBar>  
    </phone:PhoneApplicationPage.ApplicationBar>  
</phone:PhoneApplicationPage> 
```

There’s really nothing new here in v1.1. But I just wanted to remind you that Caliburn.Micro supports Actions on the AppBar as long as you use CM’s AppBarButton and AppBarMenuItem :)

### IPhoneService

The IPhoneService wraps the phone’s frame and provides access to important information and events. We had this service in v1.0 but we’ve expanded it in v1.1 to expose a better event model. Those familiar with WP7 know that the phone has a series of events that fire in different circumstances: Launching, Activated, Deactivated and Closing. Unfortunately, these events obscure whether the phone is actually resurrecting from a tombstoned state or simply continuing execution. The current SDK does not make it easy for the developer to actually determine this, so Caliburn.Micro does the heavy lifting for you and provides the following event model:

 - Launching - Occurs when a fresh instance of the application is launching.
 - Activated - Occurs when a previously paused/tombstoned app is resumed/resurrected.
 - Deactivated - Occurs when the application is being paused or tombstoned.
 - Closing - Occurs when the application is closing.
 - Continuing - Occurs when the app is continuing from a temporarily paused state.
 - Continued - Occurs after the app has continued from a temporarily paused state.
 - Resurrecting - Occurs when the app is "resurrecting" from a tombstoned state.
 - Resurrected - Occurs after the app has "resurrected" from a tombstoned state.

Using these new events, you can more intelligently make decisions about whether or not you need to restore data. In the forthcoming Mango release, the platform will provide us information on whether the app is continuing or resurrecting. However, developers working with Caliburn.Micro can have that information now and when Mango arrives, we’ll update our implementation to use the new bits. Your code won’t have to change.

### Tombstoning

As you might imagine, our new tombstoning mechanism takes advantage of the new events so that it can more reliably and accurately save/restore important data. Let’s have a look at the PivotPageViewModel to see how it interacts with the tombstoning mechanism.

``` csharp
public class PivotPageViewModel : Conductor<IScreen>.Collection.OneActive {  
    readonly Func<TabViewModel> createTab;  
  
    public PivotPageViewModel(Func<TabViewModel> createTab) {  
        this.createTab = createTab;  
    }  
  
    public int NumberOfTabs { get; set; }  
  
    protected override void OnInitialize() {  
        Enumerable.Range(1, NumberOfTabs).Apply(x => {  
            var tab = createTab();  
            tab.DisplayName = "Item " + x;  
            Items.Add(tab);  
        });  
  
        ActivateItem(Items[0]);  
    }  
}  
```

The PivotPageViewModel will receive the number of pivot items to create through it’s NumberOfTabs property, which is pushed in from the query string, as mentioned above. It will then add these items to the conductor and activate the first one. If you’re familiar with the Pivot and CM’s previous sample, you’ll notice that our PivotFix is gone. Pivot has a horrible bug that will crash your application if you try to set the SelectedItem or SelectedIndex to an item 3 or greater from either end of the pivot collection, while the Pivot itself is not visible. This makes it really hard to restore this control from a tombstoned state because you have to set the value at the exact right time. Previously we used a PivotFix hack to work around the control’s bug, but the new tombstoning mechanism is powerful and extensible enough to just make it work. You’ll notice that there are no attributes describing tombstoning behavior. They’ve been removed in favor of a poco model inspired by Fluent NHibhernate. If you would rather have the attributes, you can actually build them on top of the new system. The new system is also more reliable than previously and has a lot more options for storage. Let’s see the class that describes the tombstoning behavior for PivotPageViewModel:

``` csharp
public class PivotPageModelStorage : StorageHandler<PivotPageViewModel> {  
    public override void Configure() {  
        this.ActiveItemIndex()  
            .InPhoneState()  
            .RestoreAfterViewLoad();  
    }  
}  
```

All you have to do to make a class participate in tombstoning is to inherit from StorageHandler<T>. The PhoneContainer will auto-register anything of this type in the assembly. Just override the Configure method and declare the tombstoning instructions. I’ve created some extension methods for common scenarios. Here’s what the above declaration states:

 - Persist the Conductor’s ActiveItem’s index
 - Store the index in PhoneState
 - Restore the value after the associated view has been loaded.

Let’s look at the storage handler for the TabViewModel to see some more options:

``` csharp
public class TabViewModelStorage : StorageHandler<TabViewModel> {  
    public override void Configure() {  
        Id(x => x.DisplayName);  
  
        Property(x => x.Text)  
            .InPhoneState()  
            .RestoreAfterActivation();  
    }  
}  
```

Here we are specifying an Id because we actually need to persist multiple instances of the same VM. When we restore, we’ll need to know how to map the properties back. We’re also storing the data in PhoneState, but this time we’re not waiting for the view to load, but just waiting for the TabViewModel to be activated by its owning Conductor.

Out of the box, we also support storing data in AppSettings. For example, if you wanted to same tab to be selected **across application restarts** not just when tombstoned, you could define the PivotPageModelStorage like this:

``` csharp
public class PivotPageModelStorage : StorageHandler<PivotPageViewModel> {  
    public override void Configure() {  
        this.ActiveItemIndex()  
            .InAppSettings()  
            .RestoreAfterViewLoad();  
    }  
}  
```

Pretty easy? All this works by collaborating with the IoC container and keying off of the new event model exposed by the IPhoneService. It’s pretty powerful and extensible. You can add your own storage mechanism or define your own restore timing. You can even implement IStorageHandler directly to write completely custom code on a class by class basis. You could easily add a version that inspected classes for custom attributes and built up the configuration, if you like the attribute model better. You can also store whole instances, not just their properties, and have them rehydrated properly and available for ctor injection.

##### Note About Restore Timing

If you want the data to be restored as soon as the object is created, leave off the timing specifier, ie. RestoreAfterViewLoad. The default is to restore the data immediately.

### Launchers and Choosers

Launchers and Choosers are painful to work with if you want to do MVVM. In v1.0 we provided a solution to this. I wasn’t happy with its implementation…it was unpredictable in certain scenarios. Once we established the new phone events, better IoC integration and new tombstoning mechanism, I realized I could build a better launcher/chooser system. Let’s take a look at the updated version of TabViewModel in order to see how it works:

``` csharp
public class TabViewModel : Screen, IHandle<TaskCompleted<PhoneNumberResult>> {  
    string text;  
    readonly IEventAggregator events;  
  
    public TabViewModel(IEventAggregator events) {  
        this.events = events;  
    }  
  
    public string Text {  
        get { return text; }  
        set {  
            text = value;  
            NotifyOfPropertyChange(() => Text);  
        }  
    }  
  
    public void Choose() {  
        events.RequestTask<PhoneNumberChooserTask>();  
    }  
  
    public void Handle(TaskCompleted<PhoneNumberResult> message) {  
        MessageBox.Show("The result was " + message.Result.TaskResult, DisplayName, MessageBoxButton.OK);  
    }  
  
    protected override void OnActivate() {  
        events.Subscribe(this);  
        base.OnActivate();  
    }  
  
    protected override void OnDeactivate(bool close) {  
        events.Unsubscribe(this);  
        base.OnDeactivate(close);  
    }  
}  
```

The most significant architectural change I made was to re-implement the launcher/chooser mechanism to work on top of the IEventAggregator. Take a look at the Choose method. The RequestTask method is just an extension method of the IEventAggregator that publishes a special event that the framework is subscribed to. The framework then starts the task. When it’s completed, the framework publishes an event TaskCompleted<T> where T is the result the the chooser returns. You can register for this in the same VM that published the chooser event or in an entirely different one if you like. In the case of our sample, we have 5 TabViewModels that can launch the same chooser. That’s probably not normal, but you can handle this situation in three ways. In our case, the VMs are in a Conductor, and only one of them can be active at a time, so we just Subscribe/Unsubscribe based on the Screen lifecycle so that only the active VM will receive the result. This is a version of the Latch pattern. The second way to handle this is through the event state. When you call the RequestTask method you can pass a state object which you can use for identification purposes later. Yes, this will be present even if the chooser causes a tombstone event. The final way is to have a single object that registers for the completed event, decoupling the launching from the completion. Thus multiple VM could launch the same chooser, but only one class would handle the result.

### IWindowManager

The IWindowManager was actually in v1.0, as a last minute addition. It’s a really easy way to show native-looking, custom message boxes or modal dialogs. You can also use it to show popups. There is a whole topic devoted to this in the docs.