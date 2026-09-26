# Coroutines

Caliburn.Light.Coroutines adds a lightweight coroutine model for composing asynchronous workflows as a sequence of steps. This is useful when a task naturally reads as “do this, wait for it, then do that next” instead of nesting callbacks or `await` expressions.

It complements the async/await-first APIs in the rest of Caliburn.Light and remains especially handy for workflow-style UI logic.

## Install

```bash
dotnet add package Caliburn.Light.Coroutines
```

## Core types

The coroutine model is built around a small set of types:

- `ICoTask` — a single unit of work that executes and raises `Completed`
- `ICoTask<TResult>` — a typed co-task that also exposes a result value
- `CommandExecutionContext` — metadata about the command invocation (source, target, event args, and custom values)
- `Coroutine` — helpers for wrapping delegates, tasks, and sequences as coroutines

## A simple coroutine

The simplest pattern is to create a sequence of steps and convert it into a single coroutine:

```csharp
using System.Collections.Generic;
using Caliburn.Light;

public class UserWorkflow
{
    public IEnumerator<ICoTask> LoadUser()
    {
        yield return new Action(() => IsBusy = true).AsCoTask();
        yield return LoadUserAsync().AsCoTask();
        yield return new Action(() => IsBusy = false).AsCoTask();
        yield return new Action(() => DetailsVisible = true).AsCoTask();
    }

    private bool IsBusy { get; set; }
    private bool DetailsVisible { get; set; }

    private Task<User> LoadUserAsync()
    {
        return Task.FromResult(new User());
    }
}
```

Then execute it with `ExecuteAsync()`:

```csharp
var workflow = new UserWorkflow();
await workflow.LoadUser().AsCoTask().ExecuteAsync();
```

This preserves a sequential flow while still allowing asynchronous steps in between.

## Adapters and wrappers

The `Coroutine` helper class wraps common patterns without requiring custom implementations:

```csharp
ICoTask syncStep = new Action(() => Console.WriteLine("Hello")).AsCoTask();
ICoTask asyncStep = LoadDataAsync().AsCoTask();
ICoTask sequence = GetSteps().AsCoTask();

IEnumerator<ICoTask> GetSteps()
{
    yield return new Action(() => Console.WriteLine("Step 1")).AsCoTask();
    yield return LoadDataAsync().AsCoTask();
    yield return new Action(() => Console.WriteLine("Step 2")).AsCoTask();
}
```

The following adapters are available:

- `Action` and `Func<TResult>`
- `Task` and `Task<TResult>`
- `IEnumerator<ICoTask>` sequences

## Implementing `ICoTask`

If you need custom behavior, implement `ICoTask` and raise `Completed` when the work finishes. The framework treats both synchronous and asynchronous completion the same way.

```csharp
using System;
using Caliburn.Light;

public sealed class BusyIndicatorCoTask : ICoTask
{
    private readonly string _message;
    private readonly bool _show;

    public BusyIndicatorCoTask(string message, bool show)
    {
        _message = message;
        _show = show;
    }

    public event EventHandler<CoTaskCompletedEventArgs>? Completed;

    public void BeginExecute(CommandExecutionContext context)
    {
        // Execute the work here.
        // Once finished, notify the coroutine engine.
        Completed?.Invoke(this, new CoTaskCompletedEventArgs());
    }
}
```

`CoTaskCompletedEventArgs` carries the final status:

- `Error` — an exception raised by the task
- `WasCancelled` — whether the task was canceled

The sequential co-task engine stops on error or cancellation and continues only when the current step succeeds.

## Error handling and cancellation

Coroutines provide decorators for recovery and continuation logic:

```csharp
ICoTask workflow = LoadUserAsync()
    .AsCoTask()
    .Rescue<HttpRequestException>(ex => ShowError(ex).AsCoTask())
    .WhenCancelled(() => RetryPrompt().AsCoTask());

await workflow.ExecuteAsync();
```

Useful helpers include:

- `Rescue<TException>(...)` — execute an alternate coroutine when a matching exception occurs
- `Rescue(...)` — recover from any exception
- `WhenCancelled(...)` — run a fallback coroutine when the current one is canceled
- `OverrideCancel(...)` — suppress cancellation and continue with a replacement result when needed
- `SimpleCoTask.Succeeded()`, `Cancelled()`, and `Failed(exception)` — create trivial coroutines

## `CommandExecutionContext`

When a co-task is invoked as part of a command flow, it receives a `CommandExecutionContext` containing contextual metadata:

```csharp
public sealed class CommandExecutionContext
{
    public object? Source { get; set; }
    public object? Target { get; set; }
    public object? EventArgs { get; set; }
    public object? this[string key] { get; set; }
}
```

This lets UI tasks access the original source element or command arguments without tightly coupling the workflow to the view.

## When to use coroutines

Prefer coroutines for workflow-oriented, multi-step operations such as:

- showing a busy indicator, then loading data, then updating UI state
- running a sequence of service calls that must happen in order
- prompting the user, handling the response, and continuing with a follow-up operation

For everyday app logic, `async`/`await` is still the simpler default. Coroutines shine when the code is better expressed as a declarative sequence.

## See also

- [Quick Start](quick-start.md)
- [Async (Task Support)](async.md)
- [Commands](commands.md)
