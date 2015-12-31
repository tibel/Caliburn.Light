# Cheat Sheet

This serves as a quick guide to the most frequently used conventions and features in the Caliburn.Micro project. 

### Wiring Events
This is automatically wiring events on controls to call methods on the ViewModel.

#### Convention

``` xml
<Button x:Name="Save">
```
This will cause the Click event of the Button to call "Save" method on the ViewModel. 

#### Short Syntax

``` xml
<Button cal:Message.Attach="Save">
```

This will again cause the "Click" event of the Button to call "Save" method on the ViewModel. 

Different events can be used like this: 

``` xml
<Button cal:Message.Attach="[Event MouseEnter] = [Action Save]">
```

Different parameters can be passed to the method like this:

``` xml
<Button cal:Message.Attach="[Event MouseEnter] = [Action Save($this)]"> 
``` 

<dl>
	<dt>$eventArgs</dt>
	<dd>Passes the EventArgs or input parameter to your Action. Note: This will be null for guard methods since the trigger hasn’t actually occurred.</dd>
	<dt>$dataContext</dt>
	<dd>Passes the DataContext of the element that the ActionMessage is attached to. This is very useful in Master/Detail scenarios where the ActionMessage may bubble to a parent VM but needs to carry with it the child instance to be acted upon.</dd>
	<dt>$source</dt>
	<dd>The actual FrameworkElement that triggered the ActionMessage to be sent.</dd>
	<dt>$view</dt>
	<dd>The view (usually a UserControl or Window) that is bound to the ViewModel.</dd>
	<dt>$executionContext</dt>
	<dd>The action's execution context, which contains all the above information and more. This is useful in advanced scenarios.</dd>
	<dt>$this</dt>
	<dd>The actual UI element to which the action is attached. In this case, the element itself won't be passed as a parameter, but rather its default property.</dd>
</dl>


#### Long Syntax

``` xml
<UserControl x:Class="Caliburn.Micro.CheatSheet.ShellView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" 
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" 
    xmlns:i="clr-namespace:System.Windows.Interactivity;assembly=System.Windows.Interactivity" 
    xmlns:cal="http://www.caliburnproject.org"> 
    <StackPanel> 
        <TextBox x:Name="Name" />
        <Button Content="Save"> 
            <i:Interaction.Triggers> 
                <i:EventTrigger EventName="Click"> 
                    <cal:ActionMessage MethodName="Save"> 
                       <cal:Parameter Value="{Binding ElementName=Name, Path=Text}" /> 
                    </cal:ActionMessage> 
                </i:EventTrigger> 
            </i:Interaction.Triggers> 
        </Button> 
    </StackPanel> 
</UserControl>
```

This syntax is Expression Blend friendly. 

### Databinding

This is automatically binding dependency properties on controls to properties on the ViewModel.

#### Convention

``` xml
<TextBox x:Name="FirstName" />
```

Will cause the "Text" property of the TextBox to be bound to the "FirstName" property on the ViewModel. 

#### Explicit

``` xml
<TextBox Text="{Binding Path=FirstName, Mode=TwoWay}" />
```

This is the normal way of binding properties.

#### Event Aggregator

The three different methods on the Event Aggregator are:

``` csharp
public interface IEventAggregator {  
    void Subscribe(object instance);  
    void Unsubscribe(object instance);  
    void Publish(object message, Action<System.Action> marshal);  
}
```

An event can be a simple class such as:

``` csharp
public class MyEvent {
    public MyEvent(string myData) {
        this.MyData = myData;
    }

    public string MyData { get; private set; }
}
```