# Convention Based View/ViewModel Resolution

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
