# Customizing The Bootstrapper

In the [last part](./configuration) we discussed the most basic configuration for Caliburn.Micro and demonstrated a couple of simple features related to Actions and Conventions. In this part, I would like to explore the Bootstrapper class a little more. Let’s begin by configuring our application to use an IoC container. We’ll use MEF for this example, but Caliburn.Micro will work well with any container. First, go ahead and grab the code from Part 1. We are going to use that as our starting point. Add two additional references: System.ComponentModel.Composition and System.ComponentModel.Composition.Initialization. Those are the assemblies that contain MEF’s functionality. Now, let’s create a new Bootstrapper called MefBootstrapper. Use the following code:

``` csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

public class MefBootstrapper : BootstrapperBase
{
    private CompositionContainer container;

    public MefBootstrapper()
    {
        Initialize();
    }

    protected override void Configure()
    {
        container = CompositionHost.Initialize(
            new AggregateCatalog(
                AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>()
                )
            );

        var batch = new CompositionBatch();

        batch.AddExportedValue<IWindowManager>(new WindowManager());
        batch.AddExportedValue<IEventAggregator>(new EventAggregator());
        batch.AddExportedValue(container);

        container.Compose(batch);
    }

    protected override object GetInstance(Type serviceType, string key)
    {
        string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
        var exports = container.GetExportedValues<object>(contract);

        if (exports.Any())
            return exports.First();

        throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
    }

    protected override IEnumerable<object> GetAllInstances(Type serviceType)
    {
        return container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
    }

    protected override void BuildUp(object instance)
    {
        container.SatisfyImportsOnce(instance);
    }

    protected override void OnStartup(object sender, StartupEventArgs e)
    {
        DisplayRootViewFor<IShell>();
    }
}
```
*Note: We define IShell down below.*

That’s all the code to integrate MEF. First, we override the Configure method of the Bootstrapper class. This gives us an opportunity to set up our IoC container as well as perform any other framework configuration we may want to do, such as customizing conventions. In this case, I’m taking advantage of Silverlight’s CompositionHost to setup the CompositionContainer. You can just instantiate the container directly if you are working with .NET. Then, I’m creating an AggregateCatalog and populating it with AssemblyCatalogs; one for each Assembly in AssemblySource.Instance. So, what is AssemblySoure.Instance? This is the place that Caliburn.Micro looks for Views. You can add assemblies to this at any time during your application to make them available to the framework, but there is also a special place to do it in the Bootstrapper. Simply override SelectAssemblies like this:


``` csharp
protected override IEnumerable<Assembly> SelectAssemblies()
{
    return new[] {
        Assembly.GetExecutingAssembly()
    };
}
```

All you have to do is return a list of searchable assemblies. By default, the base class returns the assembly that your Application exists in. So, if all your views are in the same assembly as your application, you don’t even need to worry about this. If you have multiple referenced assemblies that contain views, this is an extension point you need to remember. Also, if you are dynamically loading modules, you’ll need to make sure they get registered with your IoC container and the AssemblySource.Instance when they are loaded. 

After creating the container and providing it with the catalogs, I make sure to add a few Caliburn.Micro-specific services. The framework provides default implementations of both IWindowManager and IEventAggregator. Those are pieces that I’m likely to take dependencies on elsewhere, so I want them to be available for injection. I also register the container with itself (just a personal preference).

After we configure the container, we need to tell Caliburn.Micro how to use it. That is the purpose of the three overrides that follow. “GetInstance” and “GetAllInstances” are required by the framework. “BuildUp” is optionally used to supply property dependencies to instances of IResult that are executed by the framework. 

### Word to the Wise
While Caliburn.Micro does provide ServiceLocator functionality through the Bootstrapper’s overrides and the IoC class, you should avoid using this directly in your application code. ServiceLocator is considered by many to be an anti-pattern. Pulling from a container tends to obscure the intent of the dependent code and can make testing more complicated. In future articles I will demonstrate at least one scenario where you may be tempted to access the ServiceLocator from a ViewModel. I’ll also demonstrate some solutions.2

Besides what is shown above, there are some other notable methods on the Bootstrapper. You can override OnStartup and OnExit to execute code when the application starts or shuts down respectively and OnUnhandledException to cleanup after any exception that wasn’t specifically handled by your application code. The last override, DisplayRootView, is unique. Let’s look at how it is implemented in Bootstrapper<TRootModel>

``` csharp
protected override void DisplayRootView() 
{
    var viewModel = IoC.Get<TRootModel>();
#if SILVERLIGHT
    var view = ViewLocator.LocateForModel(viewModel, null, null);
    ViewModelBinder.Bind(viewModel, view, null);

    var activator = viewModel as IActivate;
    if (activator != null)
        activator.Activate();

    Application.RootVisual = view;
#else
    IWindowManager windowManager;

    try
    {
        windowManager = IoC.Get<IWindowManager>();
    }
    catch
    {
        windowManager = new WindowManager();
    }

    windowManager.Show(viewModel);
#endif
}
```

The Silverlight version of this method resolves your root VM from the container, locates the view for it and binds the two together. It then makes sure to “activate” the VM if it implements the appropriate interface. The WPF version does the same thing by using the WindowManager class, more or less. DisplayRootView is basically a convenience implementation for model-first development. If you don’t like it, perhaps because you prefer view-first MVVM, then this is the method you want to override to change that behavior.

#### v1.1 Changes
In v1.1 we removed the DisplayRootView override and placed it's functionality in a helper method named DisplayRootViewFor. The generic bootstrapper now calls this method from the OnStartup override. To change this behavior, just override OnStartup, and instead of calling the base implementation, write your own activation code. This provides better support for splash screens, login screens and access to startup parameters.

Now that you understand all about the Bootstrapper, let’s get our sample working. We need to add the IShell interface. In our case, it’s just a marker interface. But, in a real application, you would have some significant shell-related functionality baked into this contract. Here’s the code:

``` csharp
public interface IShell
{
}
```

Now, we need to implement the interface and decorate our ShellViewModel with the appropriate MEF attributes:

``` csharp
[Export(typeof(IShell))]
public class ShellViewModel : PropertyChangedBase, IShell
{
   ...implementation is same as before...
}
```

Finally, make sure to update your App.xaml and change the HelloBootstrapper to MefBootstrapper. That’s it! Your up and running with MEF and you have a handle on some of the other key extension points of the Bootstrapper as well.

### Using Caliburn.Micro in Office and WinForms Applications

Caliburn.Micro can be used from non-Xaml hosts. In order to accomplish this, you must follow a slightly different procedure, since your application does not initiate via the App.xaml. Instead, create a custom boostrapper by inheriting from BoostrapperBase (the non-generic version). When you inherit, you should pass "false" to the base constructor's "useApplication" parameter. This allows the bootstrapper to properly configure Caliburn.Micro without the presence of a Xaml application instance. All you need to do to start the framework is create an instance of your Bootstrapper and call the Initialize() method. Once the class is instantiated, you can use Caliburn.Micro like normal, probably by invoke the IWindowManager to display new UI.
