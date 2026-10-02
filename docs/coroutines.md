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
- `Coroutine` — helpers for wrapping delegates, tasks, and sequences as coroutines

Every co-task receives a `CommandExecutionContext` — the same type used to carry command invocation metadata, which `ExecuteAsync` forwards into the co-task. Its shape is documented in [Commands](commands.md#commandexecutioncontext).

## A simple coroutine

The simplest pattern is to create a sequence of steps and convert it into a single coroutine:

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using Caliburn.Light;

public class UserWorkflow
{
    public IEnumerator<ICoTask> LoadUser()
    {
        yield return Coroutine.From(() => IsBusy = true);
        yield return Coroutine.From(LoadUserAsync);
        yield return Coroutine.From(() => IsBusy = false);
        yield return Coroutine.From(() => DetailsVisible = true);
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
    await Coroutine.From(workflow.LoadUser()).ExecuteAsync();
```

This preserves a sequential flow while still allowing asynchronous steps in between.

## Adapters and wrappers

The `Coroutine` helper class wraps common patterns without requiring custom implementations:

```csharp
ICoTask syncStep = Coroutine.From(() => Console.WriteLine("Hello"));
ICoTask asyncStep = Coroutine.From(LoadDataAsync);
ICoTask sequence = Coroutine.From(GetSteps());

IEnumerator<ICoTask> GetSteps()
{
    yield return Coroutine.From(() => Console.WriteLine("Step 1"));
    yield return Coroutine.From(LoadDataAsync);
    yield return Coroutine.From(() => Console.WriteLine("Step 2"));
}
```

The following adapters are available:

- `Action` and `Func<TResult>`
- `Func<Task>` and `Func<Task<TResult>>` method groups or lambdas
- `Func<CommandExecutionContext, Task>`, `Func<CommandExecutionContext, Task<TResult>>`, `Func<Task>`, and `Func<Task<TResult>>` (lazy factories that are invoked when the co-task begins executing)
- `IEnumerator<ICoTask>` sequences

## Implementing `ICoTask`

If you need custom behavior, implement `ICoTask` and raise `Completed` when the work finishes. The framework treats both synchronous and asynchronous completion the same way.

When implementing `ICoTask` for async work, **never** use `async void` methods as an event handler substitute. Instead, execute the async work inside `BeginExecute`, observe the returned `Task` (by awaiting it or attaching continuations), and raise `Completed` exactly once when the Task completes. The example below shows the correct pattern:

```csharp
using System;
using System.Threading.Tasks;
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
        // Show or hide the busy indicator.
        Completed?.Invoke(this, new CoTaskCompletedEventArgs(null, false));
    }

}
```

If you prefer to express async work with the coroutine API without implementing `ICoTask` yourself, you can also use the lazy Task factory adapters described in the [Adapters and wrappers](#adapters-and-wrappers) section.

`CoTaskCompletedEventArgs` carries the final status:

- `Error` — an exception raised by the task
- `WasCancelled` — whether the task was canceled

The sequential co-task engine stops on error or cancellation and continues only when the current step succeeds.

## Error handling and cancellation

Coroutines provide decorators for recovery and continuation logic:

```csharp
ICoTask workflow = Coroutine.From(LoadUserAsync)
    .Rescue<HttpRequestException>(ex => Coroutine.From(() => ShowError(ex)))
    .WhenCancelled(() => Coroutine.From(RetryPrompt));

await workflow.ExecuteAsync();
```

Useful helpers include:

- `Rescue<TException>(...)` — execute an alternate coroutine when a matching exception occurs; the optional `cancelCoTask` parameter (default `true`) controls whether the overall coroutine is reported as cancelled after the rescue runs
- `Rescue(...)` — recover from any exception (same `cancelCoTask` option)
- `WhenCancelled(...)` — run a fallback coroutine when the current one is canceled
- `OverrideCancel(...)` — suppress cancellation and continue with a replacement result when needed
- `SimpleCoTask.Succeeded()`, `Cancelled()`, and `Failed(exception)` — create trivial coroutines

## Completion semantics

When a co-task completes, `CoTaskCompletedEventArgs` indicates the final outcome. The sequential executor interprets completion as follows:
- If `WasCancelled` is `true`, the coroutine is treated as cancelled (and `Error` may be ignored).
- If `Error` is non-null and `WasCancelled` is `false`, the coroutine is treated as failed with that exception.
- Otherwise the coroutine succeeded. If it is an `ICoTask<TResult>`, the result is read from `Result`.

**Important:** Each co-task must raise `Completed` exactly once per execution. If both an error and cancellation apply, cancellation takes precedence (as reflected by the executor's mapping to `TaskCanceledException`). Implementations should be deterministic about which one they signal. For Task-based adapters, `OperationCanceledException` from the awaited Task is translated to `WasCancelled = true`; other exceptions become `Error`. The framework does not require that `Task.IsCanceled` is set—an `OperationCanceledException` is sufficient.

Note that `Rescue` (with the default `cancelCoTask: true`) and `WhenCancelled` keep the overall coroutine marked as cancelled even when the fallback succeeds — so `await workflow.ExecuteAsync()` above throws `TaskCanceledException`. Use `.OverrideCancel()`, or `.OverrideCancel<TResult>(...)` when a result is involved, to turn the cancelled outcome into a successful one instead.

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
