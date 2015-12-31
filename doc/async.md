# Async (Task Support)

From the beginning Caliburn.Micro uses IResult and Coroutines for asynchronous operations.

With .NET 4.0 Microsoft added Task to the .NET Framework for better supporting parallel/async operations. And in .NET 4.5 they added async/await keywords to C# and VB.NET.

#### Caliburn.Micro has extensions to support Task.

Instead of registering an callback when a coroutine is completed you now can await it:

``` csharp
public static class Coroutine {

    public static void BeginExecute(
		IEnumerator<IResult> coroutine, 
		CoroutineExecutionContext context = null, 
		EventHandler<ResultCompletionEventArgs> callback = null) { }

    public static Task ExecuteAsync(
		IEnumerator<IResult> coroutine,
		CoroutineExecutionContext context = null)  { }

}
```

A Task object can be wrapped in an IResult and used inside a coroutine as if it were an IResult: 

``` csharp
public static class TaskExtensions {

    public static Task ExecuteAsync(
		this IResult result, CoroutineExecutionContext context = null) { }

    public static Task<TResult> ExecuteAsync<TResult>(
		this IResult<TResult> result,
		CoroutineExecutionContext context = null) { }

    public static TaskResult AsResult(this Task task) { }

    public static TaskResult<TResult> AsResult<TResult>(this Task<TResult> task) { }

}
```

##### Example

``` csharp
yield return Task.Delay(500).AsResult();
```

The other way round also works as you can wrap an IResult in a Task by ExecuteAsync.

##### Example

``` csharp
await new SimpleResult().ExecuteAsync();
```

And finally also EventAggregator supports Task with the IHandleWithTask<TMessage> interface.