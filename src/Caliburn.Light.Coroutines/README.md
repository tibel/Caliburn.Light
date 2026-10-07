# Caliburn.Light.Coroutines

[![NuGet](https://img.shields.io/nuget/v/Caliburn.Light.Coroutines.svg)](https://www.nuget.org/packages/Caliburn.Light.Coroutines/)

Coroutine support for Caliburn.Light - compose asynchronous workflows as sequential steps.

## Overview

Caliburn.Light.Coroutines provides an `ICoTask`-based coroutine system that lets you compose asynchronous operations as sequential pipelines. Each step in a pipeline signals completion via an event, and the framework chains them together automatically.

- **`ICoTask` / `ICoTask<TResult>`**: The core abstraction — an asynchronous unit of work that signals completion via the `Completed` event
- **Sequential Composition**: Yield multiple `ICoTask` instances from an `IEnumerator<ICoTask>` to run them in sequence
- **Decorators**: Chain cross-cutting behaviors onto any coroutine:
  - `Rescue<TException>` — catch and handle specific exceptions with a recovery coroutine (optional `cancelCoTask` parameter, default `true`, controls whether the overall coroutine is reported as cancelled after the rescue runs)
  - `WhenCancelled` — run an alternative coroutine when the original is canceled
  - `OverrideCancel` — suppress cancellation and continue normally
- **Adapters**: Wrap existing constructs as coroutines with `Coroutine.From()`:
  - `Action` and `Func<TResult>` delegates
  - `Func<CommandExecutionContext, Task>`, `Func<CommandExecutionContext, Task<TResult>>`, `Func<Task>`, and `Func<Task<TResult>>`
  - `IEnumerator<ICoTask>` sequences
- **`SimpleCoTask`**: Factory for trivial coroutines — `Succeeded()`, `Cancelled()`, `Failed(exception)`
- **Task Integration**: Convert any `ICoTask` to `Task` with `ExecuteAsync()` for use with `async`/`await`

## Usage

```csharp
// Wrap a delegate as a coroutine
ICoTask step = Coroutine.From(() => Console.WriteLine("Hello"));

// Compose a sequence from a generator method
ICoTask sequence = Coroutine.From(GetSteps());

IEnumerator<ICoTask> GetSteps()
{
    yield return Coroutine.From(() => LoadData());
    yield return Coroutine.From(LoadFromServerAsync);
    yield return Coroutine.From(() => UpdateUI());
}

// Execute with async/await
await sequence.ExecuteAsync();

// Add error handling and cancellation recovery
ICoTask robust = Coroutine.From(LoadFromServerAsync)
    .Rescue<HttpRequestException>(ex => Coroutine.From(() => ShowError(ex)))
    .WhenCancelled(() => Coroutine.From(() => ShowCancelledMessage()));
```

Note that `Rescue` (with the default `cancelCoTask: true`) and `WhenCancelled` keep the overall coroutine marked as cancelled even when the fallback succeeds — so `await robust.ExecuteAsync()` throws `TaskCanceledException`. Use `.OverrideCancel()`, or `.OverrideCancel<TResult>(...)` when a result is involved, to turn the cancelled outcome into a successful one instead.

## Documentation

For complete documentation, visit the [Caliburn.Light documentation](https://github.com/tibel/Caliburn.Light/tree/main/docs).

## License

Caliburn.Light is licensed under the [MIT license](https://github.com/tibel/Caliburn.Light/blob/main/LICENSE).
