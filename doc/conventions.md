# All about Conventions

One of the main features of Caliburn.Light is manifest in its ability to remove the need for boiler plate code by acting on a series of conventions. Some people love conventions and some hate them. That’s why CM’s conventions are fully customizable and can even be turned off completely if not desired. If you are going to use conventions, and since they are ON by default, it’s good to know what those conventions are and how they work. That’s the subject of this article.

### View Resolution (ViewModel-First)

##### Basics

The first convention you are likely to encounter when using CM is related to view resolution. This convention affects any ViewModel-First areas of your application. In ViewModel-First, we have an existing ViewModel that we need to render to the screen. To do this, CM uses a simple naming pattern to find a UserControl1 that it should bind to the ViewModel and display. So, what is that pattern? Let’s just take a look at ViewLocator.LocateForModelType to find out:

``` csharp
public static Func<Type, DependencyObject, object, UIElement> LocateForModelType = (modelType, displayLocation, context) =>{
    var viewTypeName = modelType.FullName.Replace("Model", string.Empty);
    if(context != null)
    {
        viewTypeName = viewTypeName.Remove(viewTypeName.Length - 4, 4);
        viewTypeName = viewTypeName + "." + context;
    }

    var viewType = (from assmebly in AssemblySource.Instance
                    from type in assmebly.GetExportedTypes()
                    where type.FullName == viewTypeName
                    select type).FirstOrDefault();

    return viewType == null
        ? new TextBlock { Text = string.Format("{0} not found.", viewTypeName) }
        : GetOrCreateViewType(viewType);
};
```

Let’s ignore the “context” variable at first. To derive the view, we make an assumption that you are using the text “ViewModel” in the naming of your VMs, so we just change that to “View” everywhere that we find it by removing the word “Model”. This has the effect of changing both type names and namespaces. So ViewModels.CustomerViewModel would become Views.CustomerView. Or if you are organizing your application by feature: CustomerManagement.CustomerViewModel becomes CustomerManagement.CustomerView. Hopefully, that’s pretty straight forward. Once we have the name, we then search for types with that name. We search any assembly you have exposed to CM as searchable via AssemblySource.Instance.2 If we find the type, we create an instance (or get one from the IoC container if it’s registered) and return it to the caller. If we don’t find the type, we generate a view with an appropriate “not found” message.

Now, back to that “context” value. This is how CM supports multiple Views over the same ViewModel. If a context (typically a string or an enum) is provided, we do a further transformation of the name, based on that value. This transformation effectively assumes you have a folder (namespace) for the different views by removing the word “View” from the end and appending the context instead. So, given a context of “Master” our ViewModels.CustomerViewModel would become Views.Customer.Master.

##### Other Things To Know

Besides instantiation of your View, GetOrCreateViewType will call InitializeComponent on your View (if it exists). This means that for Views created by the ViewLocator, you don’t have to have code-behinds at all. You can delete them if that makes you happy :) You should also know that ViewLocator.LocateForModelType is never called directly. It is always called indirectly through ViewLocator.LocateForModel. LocateForModel takes an instance of your ViewModel and returns an instance of your View. One of the functions of LocateForModel is to inspect your ViewModel to see if it implements IViewAware. If so, it will call it’s GetView method to see if you have a cached view or if you are handling View creation explicitly. If not, then it passes your ViewModel’s type to LocateForModelType.

##### Customization

The out-of-the-box convention is pretty simple and based on a number of patterns we’ve used and seen others use in the real world. However, by no means are you limited to these simple patterns. You’ll notice that all the methods discussed above are implemented as Funcs rather than actual methods. This means that you can customize them by simply replacing them with your own implementations. If you just want to add to the existing behavior, simply store the existing Func in a variable, create a new Func that calls the old and and assign the new Func to ViewLocator.LocateForModelType.

**v1.1 Changes** In v1.1 we've completely changed the implementation of the LocateForModelType func. We now use an instance of our new NameTransformer class with pre-configured RexEx-based rules to do the name mapping. We support the same conventions out of the box as before, but you can now more easily add custom transformation rules.

##### Framework Usage

There are three places that the framework uses the ViewLocator; three places where you can expect the view location conventions to be applied. The first place is in Bootstrapper<T>. Here, your root ViewModel is passed to the locator in order to determine how your application’s shell should be rendered. In Silverlight this results in the setting or your RootVisual. In WPF, this creates your MainWindow. In fact, in WPF the bootstrapper delegates this to the WindowManager, which brings me to… The second place the ViewLocator is used is the WindowManager, which calls it to determine how any dialog ViewModels should be rendered. The third and final place that leverages these conventions is the View.Model attached property. Whenever you do ViewModel-First composition rendering by using the View.Model attached property on a UIElement, the locator is invoked to see how that composed ViewModel should be rendered at that location in the UI. You can use the View.Model attached property explicitly in your UI (optionally combining it with the View.Context attached property for contextual rendering), or it can be added by convention, thus causing conventional composition of views to occur. See the section below on property binding conventions.

### ViewModel Resolution (View-First)

##### Basics

Though Caliburn.Light prefers ViewModel-First development, there are times when you may want to take a View-First approach, especially when working with WP7. In the case where you start with a view, you will likely then need to resolve a ViewModel. We use a similar naming convention for this scenario as we did with view location. This is handled by ViewModelLocator.LocateForViewType. While with View location we change instances of “ViewModel” to “View”, with ViewModel location we change “View” to “ViewModel.” The other interesting difference is in how we get the instance of the ViewModel itself. Because your ViewModels may be registered by an interface or a concrete class we attempt to generate possible interface names as well. If we find a match, we resolve it from the IoC container.

##### Other Things To Know

ViewModelLocator.LocateForViewType is actually never called directly by the framework. It’s called internally by ViewModelLocator.LocateForView. LocateForView first checks your View instance’s DataContext to see if you’ve previous cached or custom created your ViewModel. If the DataContext is null, only then will it call into LocateForViewType. A final thing to note is that automatic InitializeComponent calls are not supported by view first, by its nature.

##### Customization

In v1.1 we've completely changed the implementation of the LocateForViewType func. We now use an instance of our new NameTransformer class with pre-configured RexEx-based rules to do the name mapping. We support the same conventions out of the box as before, but you can now more easily add custom transformation rules.

##### Framework Usage

The ViewModelLocator is only used by the WP7 version of the framework. It’s used by the FrameAdapter which insures that every time you navigate to a page, it is supplied with the correct ViewModel. It could be easily adapted for use by the Silverlight Navigation Framework if desired.

### ViewModelBinder

##### Basics

When we bind together your View and ViewModel, regardless of whether you use a ViewModel-First or a View-First approach, the ViewModelBinder.Bind method is called. This method sets the Action.Target of the View to the ViewModel and correspondingly sets the DataContext to the same value.4 It also checks to see if your ViewModel implements IViewAware, and if so, passes the View to your ViewModel. This allows for a more SupervisingController style design, if that fits your scenario better. The final important thing the ViewModelBinder does is determine if it needs to create any conventional property bindings or actions. To do this, it searches the UI for a list of element candidates for bindings/actions and compares that against the properties and methods of your ViewModel. When a match is found, it creates the binding or the action on your behalf.

##### Other Things To Know

On all platforms, conventions cannot by applied to the contents of a DataTemplate. This is a current limitation of the Xaml templating system. I have asked Microsoft to fix this, but I doubt they will respond. As a result, in order to have Binding and Action conventions applied to your DataTemplate, you must add a Bind.Model="{Binding}" attached property to the root element inside the DataTemplate. This provides the necessary hook for Caliburn.Light to apply its conventions each time a UI is instantiated from a DataTemplate.

On the WP7 platform, if the View you are binding is a PhoneApplicationPage, this service is responsible for wiring up actions to the ApplicationBar’s Buttons and Menus. See the WP7 specific docs for more information on that.

##### Customization

Should you decide that you don’t like the behavior of the ViewModelBinder (more details below), it follows the same patterns as the above framework services. It has several Funcs you can replace with your own implementations, such as Bind, BindActions and BindProperties. Probably the most important aspect of customization though, is the ability to turn off the binder’s convention features. To do this, set ViewModelBinder.ApplyConventionsByDefault to false. If you want to enable it on a view-by-view basis, you can set the View.ApplyConventions attached property to true on your View. This attached property works both ways. So, if you have conventions on by default, but need to turn them off on a view-by-view basis, you just set this property to false.

##### Framework Usage

The ViewModelBinder is used in three places inside of Caliburn.Light. The first place is inside the implementation of the View.Model attached property. This property takes your ViewModel, locates a view using the ViewLocator and then passes both of them along to the ViewModelBinder. After binding is complete, the View is injected inside the element on which the property is defined. That’s the ViewModel-First usage pattern. The second place that uses the ViewModelBinder is inside the implementation of the Bind.Model attached property. This property takes a ViewModel and passes it along with the element on which the property is defined to the ViewModelBinder. In other words, this is View-First, since you have already instantiated the View inline in your Xaml and are then just invoking the binding against a ViewModel. The final place that the ViewModelBinder is used is in the WP7 version of the framework. Inside of the FrameAdapter, when a page is navigated to, the ViewModelLocator is first used to obtain the ViewModel for that page. Then, the ViewModelBinder is uses to connect the ViewModel to the page.

### Element Location

##### Basics

Now that you understand the basic role of the ViewModelBinder and where it is used by the framework, I want to dig into the details of how it applies conventions. As mentioned above, the ViewModelBinder “searches the UI for a list of element candidates for bindings/actions and compares that against the properties and methods of your ViewModel.” The first step in understanding how this works is knowing how the framework determines which elements in your UI may be candidates for conventions. It does this by using a func on the static ExtensionMethods class called GetNamedElementsInScope.5 Basically this method does two things. First, it identifies a scope to search for elements in. This means it walks the tree until it finds a suitable root node, such as a Window, UserControl or element without a parent (indicating that we are inside a DataTemplate). Once it defines the “outer” borders of the scope, it begins it’s second task: locating all elements in that scope that have names. The search is careful to respect an “inner” scope boundary by not traversing inside of child user controls. The elements that are returned by this function are then used by the ViewModelBinder to apply conventions.


##### Other Things To Know

There are a few limitations to what the GetNamedElementsInScope method can accomplish out-of-the-box. It can only search the visual tree, ContentControl.Content and ItemsControl.Items. In WPF, it also searches HeaderContentControl.Header and HeaderedItemsControl.Header. What this means is that things like ContextMenus, Tooltips or anything else that isn’t in the visual tree or one of these special locations won’t get found when trying to apply conventions.

##### Customization

You may not encounter issues related to the above limitations of element location. But if you do, you can easily replace the default implementation with your own. Here’s an interesting technique you might choose to use: If the view is a UserControl or Window, instead of walking the tree for elements, use some reflection to discover all private fields that inherit from FrameworkElement. We know that when Xaml files are compiled, a private field is created for everything with an x:Name. Use this to your advantage. You will have to fall back to the existing implementation for DataTemplate UI though. I don’t provide this implementation out-of-the-box, because it is not guaranteed to succeed in Silverlight. The reason is due to the fact that Silverlight doesn’t allow you to get the value of a private field unless the calling code is the code that defines the field. However, if all of your views are defined in a single assembly, you can easily make the modification I just described by creating your new implementation in the same assembly as the views. Furthermore, if you have a multi-assembly project, you could write a little bit of plumbing code that would allow the GetNamedElementsInScope Func to find the assembly-specific implementation which could actually perform the reflection.

##### Framework Usage

I’ve already mentioned that element location occurs when the ViewModelBinder attempts to bind properties or methods by convention. But, there is a second place that uses this functionality: the Parser. Whenever you use Message.Attach and your action contains parameters, the message parser has to find the elements that you are using as parameter inputs. It would seem that we could just do a simple FindName, but FindName is case-sensitive. As a result, we have to use our custom implementation which does a case-insensitive search. This ensures that the same semantics for binding are used in both places.

### Action Matching

##### Basics

The next thing the ViewModelBinder does after locating the elements for convention bindings is inspect them for matches to methods on the ViewModel. It does this by using a bit of reflection to get the public methods of the ViewModel. It then loops over them looking for a case-insensitive name match with an element. If a match is found, and there aren’t any pre-existing Interaction.Triggers on the element, an action is attached. The check for pre-existing triggers is used to prevent the convention system from creating duplicate actions to what the developer may have explicitly declared in the markup. To be on the safe side, if you have declared any triggers on the matched element, it is skipped.

##### Other Things To Know

The conventional actions are created by setting the Message.Attach attached property on the element. Let’s look at how that is built up:

``` csharp
var message = method.Name;
var parameters = method.GetParameters();

if(parameters.Length > 0)
{
    message += "(";

    foreach(var parameter in parameters)
    {
        var paramName = parameter.Name;
        var specialValue = "$" + paramName.ToLower();

        if(MessageBinder.SpecialValues.Contains(specialValue))
            paramName = specialValue;

        message += paramName + ",";
    }

    message = message.Remove(message.Length - 1, 1);
    message += ")";
}

Log.Info("Added convention action for {0} as {1}.", method.Name, message);
Message.SetAttach(foundControl, message);
```

As you can see, we build a string representing the message. This string contains only the action part of the message; no event is declared. You can also see that it loops through the parameters of the method so that they are included in the action. If the parameter name is the same as a special parameter value, we make sure to append the “$” to it so that it will be recognized correctly by the Parser and later by the MessageBinder when the action is invoked.

When the Message.Attach property is set, the Parser immediately kicks in to convert the string message into some sort of TriggerBase with an associated ActionMessage. Because we don’t declare an event as part of the message, the Parser looks up the default Trigger for the type of element that the message is being attached to. For example, if the message was being attached to a Button, then we would get an EventTrigger with it’s Event set to Click. This information is configured through the ConventionManager with reasonable defaults out-of-the-box. See the sections on ConventionManager and ElementConventions below for more information on that. The ElementConvention is used to create the Trigger and then the parser converts the action information into an ActionMessage. The two are connected together and then added to the Interaction.Triggers collection of the element.

##### Customization

ViewModelBinder.BindActions is a Func and thus can be entirely replaced if desired. Adding to or changing the ElementConventions via the ConventionManager will also effect how actions are put together. More on that below.


##### Framework Usage

BindActions is used exclusively by the ViewModelBinder.

### Property Matching

##### Basics

Once action binding is complete, we move on to property binding. It follows a similar process by looping through the named elements and looking for case-insensitive name matches on properties. Once a match is found, we then get the ElementConventions from the ConventionManager so we can determine just how databinding on that element should occur. The ElementConvention defines an ApplyBinding Func that takes the view model type, property path, property info, element instance, and the convention itself. This Func is responsible for creating the binding on the element using all the contextual information provided. The neat thing is that we can have custom binding behaviors for each element if we want. CM defines a basic implementation of ApplyBinding for most elements, which lives on the ConventionManager. It’s called SetBinding and looks like this:

``` csharp
public static Func<Type, string, PropertyInfo, FrameworkElement, ElementConvention, bool> SetBinding =
    (viewModelType, path, property, element, convention) => {
        var bindableProperty = convention.GetBindableProperty(element);
        if(HasBinding(element, bindableProperty))
            return false;

        var binding = new Binding(path);

        ApplyBindingMode(binding, property);
        ApplyValueConverter(binding, bindableProperty, property);
        ApplyStringFormat(binding, convention, property);
        ApplyValidation(binding, viewModelType, property);
        ApplyUpdateSourceTrigger(bindableProperty, element, binding);

        BindingOperations.SetBinding(element, bindableProperty, binding);

        return true;
    };
```

The first thing this method does is get the dependency property that should be bound by calling GetBindableProperty on the ElementConvention. Next we check to see if there is already a binding set for that property. If there is, we don’t want to overwrite it. The developer is probably doing something special here, so we return false indicating that a binding has not been added. Assuming no binding exists, this method then basically delegates to other methods on the ConventionManager for the details of binding application. Hopefully that part makes sense. Once the binding is fully constructed we add it to the element and return true indicating that the convention was applied.

There’s another important aspect to property matching that I haven’t yet mentioned. We can match on deep property paths by convention as well. So, let’s say that you have a Customer property on your ViewModel that has a FirstName property you want to bind a Textbox to. Simply give the TextBox an x:Name of “Customer_FirstName” The ViewModelBinder will do all the work to make sure that that is a valid property and will pass the correct view model type, property info and property path along to the ElementConvention’s ApplyBinding func.

##### Other Things To Know

I mentioned above that “CM defines a basic implementation of ApplyBinding for most elements.” It also defines several custom implementations of the ApplyBinding Func for elements that are typically associated with specific usage patterns or composition. For WPF and Silverlight, there are custom binding behaviors for ItemsControl and Selector. In addition to binding the ItemsSource on an ItemsControl, the ApplyBinding func also inspects the ItemTemplate, DisplayMemberPath and ItemTemplateSelector (WPF) properties. If none of these are set, then the framework knows that since you haven’t specified a renderer for the items, it should add one conventionally.7 So, we set the ItemTemplate to a default DataTemplate. Here’s what it looks like:

``` xml
<DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
              xmlns:cal="clr-namespace:Caliburn.Light;assembly=Caliburn.Light">
    <ContentControl cal:View.Model="{Binding}" 
                    VerticalContentAlignment="Stretch"
                    HorizontalContentAlignment="Stretch" />
</DataTemplate>
```

Since this template creates a ContentControl with a View.Model attached property, we create the possibility of rich composition for ItemsControls. So, whatever the Item is, the View.Model attached property allows us to invoke the ViewModel-First workflow: locate the view for the item, pass the item and the view to the ViewModelBinder (which in turn sets it’s own conventions, possibly invoking more composition) and then take the view and inject it into the ContentControl. Selectors have the same behavior as ItemsControls, but with an additional convention around the SelectedItem property. Let’s say that your Selector is called Items. We follow the above conventions first by binding ItemsSource to Items and detecting whether or not we need to add a default DataTemplate. Then, we check to see if the SelectedItem property has been bound. If not, we look for three candidate properties on the ViewModel that could be bound to SelectedItem: ActiveItem, SelectedItem and CurrentItem. If we find one of these, we add the binding. So, the pattern here is that we first call ConventionManager.Singularize on the collection property’s name. In this case “Items” becomes “Item” Then we call ConventionManager.DerivePotentialSelectionNames which prepends “Active” “Selected” and “Current” to “Item” to make the three candidates above. Then, we create a binding if we find one of these on the ViewModel. For WPF, we have a special ApplyBinding behavior for the TabControl.8 It takes on all the conventions of Selector (setting it’s ContentTemplate instead of ItemTemplate to the DefaultDataTemplate), plus an additional convention for the tab header’s content. If the TabControl’s DisplayMemberPath is not set and the ViewModel implements IHaveDisplayName, then we set it’s ItemTemplate to the DefaultHeaderTemplate, which looks like this:

``` xml
<DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    <TextBlock Text="{Binding DisplayName, Mode=TwoWay}" />
</DataTemplate>
```

So, for a named WPF TabControl we can conventionally bind in the list of tabs (ItemsSource), the tab item’s name (ItemTemplate), the content for each tab (ContentTemplate) and keep the selected tab synchronized with the model (SelectedItem). That’s not bad for one line of Xaml like this:

``` xml
<TabControl x:Name="Items" />
```

In addition to the special cases listed above, we have one more that is important: ContentControl. In this case, we don’t supply a custom ApplyBinding Func, but we do supply a custom GetBindableProperty Func. For ContentControl, when we go to determine which property to bind to, we inspect the ContentTemplate and ContentTemplateSelector (WPF). If they are both null, you haven’t specified a renderer for your model. Therefore, we assume that you want to use the ViewModel-First workflow. We implement this by having the GetBindableProperty Func return the View.Model attached property as the property to be bound. In all other cases ContentControl would be bound on the Content property. By selecting the View.Model property in the absence of a ContentTemplate, we enable rich composition.

I hope you will find that these special cases make sense when you think about them. As always, if you don’t like them, you can change them…

##### Customization

As you might imagine, the BindProperties functionality is completely customizable by replacing the Func on the ViewModelBinder. For example, if you like the idea of Action conventions but not Property conventions, you could just replace this Func with one that doesn’t do anything. However, it’s likely that you will want more fine grained control. Fortunately, nearly every aspect of the ConventionManager or of a particular ElementConvention is customizable. More details about the ConventionManager are below.

One of the common ways you will configure conventions is by adding new conventions to the system. Most commonly this will be in adding the Silverlight toolkit controls or the WP7 toolkit controls. Here’s an example of how you would set up an advanced convention for the WP7 Pivot control which would make it work like the WPF TabControl:

``` csharp
ConventionManager.AddElementConvention<Pivot>(Pivot.ItemsSourceProperty, "SelectedItem", "SelectionChanged").ApplyBinding =
    (viewModelType, path, property, element, convention) => {
        ConventionManager
            .GetElementConvention(typeof(ItemsControl))
            .ApplyBinding(viewModelType, path, property, element, convention);
        ConventionManager
            .ConfigureSelectedItem(element, Pivot.SelectedItemProperty, viewModelType, path);
        ConventionManager
            .ApplyHeaderTemplate(element, Pivot.HeaderTemplateProperty, viewModelType);
    };
```

Pretty cool?

##### Framework Usage

BindProperties is used exclusively by the ViewModelBinder.

### Convention Manager

If you’ve read this far, you know that the ConventionManager is leveraged heavily by the Action and Property binding mechanisms. It is the gateway to fine-tuning the majority of the convention behavior in the framework. What follows is a list of the replaceable Funcs and Properties which you can use to customize the conventions of the framework:

##### Properties

 - BooleanToVisibilityConverter – The default IValueConverter used for converting Boolean to Visibility and back. Used by ApplyValueConverter.
 - IncludeStaticProperties - Indicates whether or not static properties should be included during convention name matching. False by default.
 - DefaultItemTemplate – Used when an ItemsControl or ContentControl needs a DataTemplate.
 - DefaultHeaderTemplate – Used by ApplyHeaderTemplate when the TabControl needs a header template.

##### Funcs

 - Singularize – Turns a word from its plural form to its singular form. The default implementation is really basic and just strips the trailing ‘s’.
 - DerivePotentialSelectionNames – Given a base collection name, returns a list of possible property names representing the selection. Uses Singularize.
 - SetBinding – The default implementation of ApplyBinding used by ElementConventions (more info below). Changing this will change how all conventional bindings are applied. Uses the following Funcs internally:
	 - HasBinding - Determines whether a particular dependency property already has a binding on the provided element. If a binding already exists, SetBinding is aborted.
	 - ApplyBindingMode - Applies the appropriate binding mode to the binding.
	 - ApplyValidation - Determines whether or not and what type of validation to enable on the binding.
	 - ApplyValueConverter - Determines whether a value converter is is needed and applies one to the binding. It only checks for BooleanToVisibility conversion by default.
	 - ApplyStringFormat - Determines whether a custom string format is needed and applies it to the binding. By default, if binding to a DateTime, uses the format "{0:MM/dd/yyyy}".
	 - ApplyUpdateSourceTrigger - Determines whether a custom update source trigger should be applied to the binding. For WPF, always sets to UpdateSourceTrigger=PropertyChanged. For Silverlight, calls ApplySilverlightTriggers.

##### Methods

 - AddElementConvention – Adds or replaces an ElementConvention.
 - GetElementConvention – Gets the convention for a particular element type. If not found, searches the type hierarchy for a match.
 - ApplyHeaderTemplate – Applies the header template convention to an element.
 - ApplySilverlightTriggers – For TextBox and PasswordBox, wires the appropriate events to binding updates in order to simulate WPF’s UpdateSourceTrigger=PropertyChanged.

### ElementConvention

ElementConventions can be added or replaced through ConventionManager.AddElementConvention. But, it’s important to know what these conventions are and how they are used throughout the framework. At the very bottom of this article is a code listing showing how all the elements are configured out-of-the-box. Here are the Properties and Funcs of the ElementConvention class with brief explanations:

##### Properties

 - ElementType – The type of element to which the convention applies.
 - ParameterProperty – When using Message.Attach to declare an action, if a parameter that refers to an element is specified, but the property of that element is not, the ElementConvention will be looked up and the ParameterProperty will be used. For example, if we have this markup:

``` csharp
<TextBox x:Name="something" />
<Button cal:Message.Attach="MyMethod(something)" />
```

 - When the Button’s ActionMessage is created, we look up “something”, which is a TextBox. We get the ElementConvention for TextBox which has its ParameterProperty set to “Text.” Therefore, we create the parameter for MyMethod from something.Text.

##### Funcs

 - GetBindableProperty – Gets the property for the element which should be used in convention binding.
 - CreateTrigger – When Message.Attach is used to declare an Action, and the specific event is not specified, the ElementConvention will be looked up and the CreateTrigger Func will be called to create the Interaction.Trigger. For example in the Xaml above, when the ActionMessage is created for the Button, the Button’s ElementConvention will be looked up and its CreateTrigger Func will be called. In this case, the ElementConvention returns an EventTrigger configured to use the Click event.
 - ApplyBinding – As described above, when conventional databinding occurs, the element we are binding has its ElementConvention looked up and it’s ApplyBinding func is called. By default this just passes through to ConventionManager.SetBinding. But certain elements (see above…or below) customize this in order to enable more powerful composition scenarios.

There is a helper method on the ConventionManager called AddElementConvention which can be used like this:

``` csharp
ConventionManager.AddElementConvention<Rating>(Rating.ValueProperty, "Value", "ValueChanged");
```

In the case mentioned above, the first parameter value of Rating.ValueProperty tells the convention system what the default bindable property is for the element. So, if we have a convention match on a Rating control, we set up the binding against the ValueProperty. The second parameter represents the default property to be used in Action bindings. So, if you create an action binding with an ElementName that points to a Rating control, but do not specify the property, we fall back to the "Value" property. Finally, the thrid parameter represents the default event for the control. So, if we attach an action to a rating control, but don't specify the event to trigger that action, the system will fall back to the "ValueChanged" event. These element conventions allow the developer to supply as much or as little information in a variety of situations, allowing the framework to fill in the missing details as approptiate.