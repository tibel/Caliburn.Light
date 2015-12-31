# IResult and Coroutines

Previously, I mentioned that there was one more compelling feature of the Actions concept called Coroutines. If you haven’t heard that term before, here’s what wikipedia has to say:

> In computer science, coroutines are program components that generalize subroutines to allow multiple entry points for suspending and resuming execution at certain locations. Coroutines are well-suited for implementing more familiar program components such as cooperative tasks, iterators,infinite lists and pipes.

Here’s one way you can thing about it: Imagine being able to execute a method, then pause it’s execution on some statement, go do something else, then come back and resume execution where you left off. This technique is extremely powerful in task-based programming, especially when those tasks need to run asynchronously. For example, let’s say we have a ViewModel that needs to call a web service asynchronously, then it needs to take the results of that, do some work on it and call another web service asynchronously. Finally, it must then display the result in a modal dialog and respond to the user’s dialog selection with another asynchronous task. Accomplishing this with the standard event-driven async model is not a pleasant experience. However, this is a simple task to accomplish by using coroutines. The problem…C# doesn’t implement coroutines natively. Fortunately, we can (sort of) build them on top of iterators.

There are two things necessary to take advantage of this feature in Caliburn.Micro: First, implement the IResult interface on some class, representing the task you wish to execute; Second, yield instances of IResult from an Action2. Let’s make this more concrete. Say we had a Silverlight application where we wanted to dynamically download and show screens not part of the main package. First we would probably want to show a “Loading” indicator, then asynchronously download the external package, next hide the “Loading” indicator and finally navigate to a particular screen inside the dynamic module. Here’s what the code would look like if your first screen wanted to use coroutines to navigate to a dynamically loaded second screen:

``` csharp
using System.Collections.Generic;
using System.ComponentModel.Composition;

[Export(typeof(ScreenOneViewModel))]
public class ScreenOneViewModel
{
    public IEnumerable<IResult> GoForward()
    {
        yield return Loader.Show("Downloading...");
        yield return new LoadCatalog("Caliburn.Micro.Coroutines.External.xap");
        yield return Loader.Hide();
        yield return new ShowScreen("ExternalScreen");
    }
}
```

First, notice that the Action “GoForward” has a return type of IEnumerable<IResult>. This is critical for using coroutines. The body of the method has four yield statements. Each of these yields is returning an instance of IResult. The first is a result to show the “Downloading” indicator, the second to download the xap asynchronously, the third to hide the “Downloading” message and the fourth to show a new screen from the downloaded xap. After each yield statement, the compiler will “pause” the execution of this method until that particular task completes. The first, third and fourth tasks are synchronous, while the second is asynchronous. But the yield syntax allows you to write all the code in a sequential fashion, preserving the original workflow as a much more readable and declarative structure. To understand a bit more how this works, have a look at the IResult interface:

``` csharp
public interface IResult
{
    void Execute(CoroutineExecutionContext context);
    event EventHandler<ResultCompletionEventArgs> Completed;
}
```

It’s a fairly simple interface to implement. Simply write your code in the “Execute” method and be sure to raise the “Completed” event when you are done, whether it be a synchronous or an asynchronous task. Because coroutines occur inside of an Action, we provide you with an ActionExecutionContext useful in building UI-related IResult implementations. This allows the ViewModel a way to declaratively state its intentions in controlling the view without having any reference to a View or the need for interaction-based unit testing. Here’s what the ActionExecutionContext looks like:

``` csharp
public class ActionExecutionContext
{
    public ActionMessage Message;
    public FrameworkElement Source;
    public object EventArgs;
    public object Target;
    public DependencyObject View;
    public MethodInfo Method;
    public Func<bool> CanExecute;
    public object this[string key];
}
```

And here’s an explanation of what all these properties mean:

<dl>
	<dt>Message</dt>
	<dd>The original ActionMessage that caused the invocation of this IResult.</dd>
	<dt>Source</dt>
	<dd>The FrameworkElement that triggered the execution of the Action.</dd>
	<dt>EventArgs</dt>
	<dd>Any event arguments associated with the trigger of the Action.</dd>
	<dt>Target</dt>
	<dd>The class instance on which the actual Action method exists.</dd>
	<dt>View</dt>
	<dd>The view associated with the Target.</dd>
	<dt>Method</dt>
	<dd>The MethodInfo specifying which method to invoke on the Target instance.</dd>
	<dt>CanExecute</dt>
	<dd>A function that returns true if the Action can be invoked, false otherwise.</dd>
	<dt>Key Index</dt>
	<dd>A place to store/retrieve any additional metadata which may be used by extensions to the framework.</dd>
</dl>

Bearing that in mind, I wrote a naive Loader IResult that searches the VisualTree looking for the first instance of a BusyIndicator to use to display a loading message. Here’s the implementation:

``` csharp
using System;
using System.Windows;
using System.Windows.Controls;

public class Loader : IResult
{
    readonly string message;
    readonly bool hide;

    public Loader(string message)
    {
        this.message = message;
    }

    public Loader(bool hide)
    {
        this.hide = hide;
    }

    public void Execute(CoroutineExecutionContext context)
    {
        var view = context.View as FrameworkElement;
        while(view != null)
        {
            var busyIndicator = view as BusyIndicator;
            if(busyIndicator != null)
            {
                if(!string.IsNullOrEmpty(message))
                    busyIndicator.BusyContent = message;
                busyIndicator.IsBusy = !hide;
                break;
            }

            view = view.Parent as FrameworkElement;
        }

        Completed(this, new ResultCompletionEventArgs());
    }

    public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };

    public static IResult Show(string message = null)
    {
        return new Loader(message);
    }

    public static IResult Hide()
    {
        return new Loader(true);
    }
}
```

See how I took advantage of context.View? This opens up a lot of possibilities while maintaining separation between the view and the view model. Just to list a few interesting things you could do with IResult implementations: show a message box, show a VM-based modal dialog, show a VM-based Popup at the user’s mouse position, play an animation, show File Save/Load dialogs, place focus on a particular UI element based on VM properties rather than controls, etc. Of course, one of the biggest opportunities is calling web services. Let’s look at how you might do that, but by using a slightly different scenario, dynamically downloading a xap:

``` csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.ReflectionModel;
using System.Linq;

public class LoadCatalog : IResult
{
    static readonly Dictionary<string, DeploymentCatalog> Catalogs = new Dictionary<string, DeploymentCatalog>();
    readonly string uri;

    [Import]
    public AggregateCatalog Catalog { get; set; }

    public LoadCatalog(string relativeUri)
    {
        uri = relativeUri;
    }

    public void Execute(CoroutineExecutionContext context)
    {
        DeploymentCatalog catalog;

        if(Catalogs.TryGetValue(uri, out catalog))
            Completed(this, new ResultCompletionEventArgs());
        else
        {
            catalog = new DeploymentCatalog(uri);
            catalog.DownloadCompleted += (s, e) =>{
                if(e.Error == null)
                {
                    Catalogs[uri] = catalog;
                    Catalog.Catalogs.Add(catalog);
                    catalog.Parts
                        .Select(part => ReflectionModelServices.GetPartType(part).Value.Assembly)
                        .Where(assembly => !AssemblySource.Instance.Contains(assembly))
                        .Apply(x => AssemblySource.Instance.Add(x));
                }
                else Loader.Hide().Execute(context);

                Completed(this, new ResultCompletionEventArgs {
                    Error = e.Error,
                    WasCancelled = false
                });
            };

            catalog.DownloadAsync();
        }
    }

    public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };
}
```

In case it wasn’t clear, this sample is using MEF. Furthermore, we are taking advantage of the DeploymentCatalog created for Silverlight 4. You don’t really need to know a lot about MEF or DeploymentCatalog to get the takeaway. Just take note of the fact that we wire for the DownloadCompleted event and make sure to fire the IResult.Completed event in its handler. This is what enables the async pattern to work. We also make sure to check the error and pass that along in the ResultCompletionEventArgs. Speaking of that, here’s what that class looks like:

``` csharp
public class ResultCompletionEventArgs : EventArgs
{
    public Exception Error;
    public bool WasCancelled;
}
```

Caliburn.Micro’s enumerator checks these properties after it get’s called back from each IResult. If there is either an error or WasCancelled is set to true, we stop execution. You can use this to your advantage. Let’s say you create an IResult for the OpenFileDialog. You could check the result of that dialog, and if the user canceled it, set WasCancelled on the event args. By doing this, you can write an action that assumes that if the code following the Dialog.Show executes, the user must have selected a file. This sort of technique can simplify the logic in such situations. Obviously, you could use the same technique for the SaveFileDialog or any confirmation style message box if you so desired. My favorite part of the LoadCatalog implementation shown above, is that the original implementation was written by a CM user! Thanks janoveh for this awesome submission! As a side note, one of the things we added to the CM project site is a “Recipes” section. We are going to be adding more common solutions such as this to that area in the coming months. So, it will be a great place to check for cool plugins and customizations to the framework.

Another thing you can do is create a series of IResult implementations built around your application’s shell. That is what the ShowScreen result used above does. Here is its implementation:

``` csharp
using System;
using System.ComponentModel.Composition;

public class ShowScreen : IResult
{
    readonly Type screenType;
    readonly string name;

    [Import]
    public IShell Shell { get; set; }

    public ShowScreen(string name)
    {
        this.name = name;
    }

    public ShowScreen(Type screenType)
    {
        this.screenType = screenType;
    }

    public void Execute(CoroutineExecutionContext context)
    {
        var screen = !string.IsNullOrEmpty(name)
            ? IoC.Get<object>(name)
            : IoC.GetInstance(screenType, null);

        Shell.ActivateItem(screen);
        Completed(this, new ResultCompletionEventArgs());
    }

    public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };

    public static ShowScreen Of<T>()
    {
        return new ShowScreen(typeof(T));
    }
}
```

This bring up another important feature of IResult. Before CM executes a result, it passes it through the IoC.BuildUp method allowing your container the opportunity to push dependencies in through the properties. This allows you to create them normally within your view models, while still allowing them to take dependencies on application services. In this case, we depend on IShell. You could also have your container injected, but in this case I chose to use the IoC static class internally. As a general rule, you should avoid pulling things from the container directly. However, I think it is acceptable when done inside of infrastructure code such as a ShowScreen IResult.

### Other Usages
Out-of-the-box Caliburn.Micro can execute coroutines automatically for any action invoked via an ActionMessage. However, there are times where you may wish to take advantage of the coroutine feature directly. To execute a coroutine, you can use the static Coroutine.BeginExecute method.

I hope this gives some explanation and creative ideas for what can be accomplished with IResult. Be sure to check out the sample application attached. There’s a few other interesting things in there as well.