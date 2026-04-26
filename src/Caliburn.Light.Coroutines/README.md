# Caliburn.Light.Coroutines

[![NuGet](https://img.shields.io/nuget/v/Caliburn.Light.Coroutines.svg)](https://www.nuget.org/packages/Caliburn.Light.Coroutines/)

Coroutine support for Caliburn.Light - composable, asynchronous task sequences for MVVM command execution.

## Overview

Caliburn.Light.Coroutines provides an `ICoTask`-based coroutine system that lets you compose asynchronous operations as sequential pipelines. Each step in a pipeline signals completion via an event, and the framework chains them together automatically.

- **`ICoTask` / `ICoTask<TResult>`**: The core abstraction — an asynchronous unit of work that signals completion via the `Completed` event
- **Sequential Composition**: Yield multiple `ICoTask` instances from an `IEnumerator<ICoTask>` to run them in sequence
- **Decorators**: Chain cross-cutting behaviors onto any coroutine:
  - `Rescue<TException>` — catch and handle specific exceptions with a recovery coroutine
  - `WhenCancelled` — run an alternative coroutine when the original is canceled
  - `OverrideCancel` — suppress cancellation and continue normally
- **Adapters**: Wrap existing constructs as coroutines with `AsCoTask()`:
  - `Action` and `Func<TResult>` delegates
  - `Task` and `Task<TResult>`
  - `IEnumerator<ICoTask>` sequences
- **`SimpleCoTask`**: Factory for trivial coroutines — `Succeeded()`, `Cancelled()`, `Failed(exception)`
- **Task Integration**: Convert any `ICoTask` to `Task` with `ExecuteAsync()` for use with `async`/`await`

## Usage

```csharp
// Wrap a delegate as a coroutine
ICoTask step = new Action(() => Console.WriteLine("Hello")).AsCoTask();

// Compose a sequence from a generator method
ICoTask sequence = GetSteps().AsCoTask();

IEnumerator<ICoTask> GetSteps()
{
    yield return new Action(() => LoadData()).AsCoTask();
    yield return LoadFromServerAsync().AsCoTask();
    yield return new Action(() => UpdateUI()).AsCoTask();
}

// Execute with async/await
await sequence.ExecuteAsync();

// Add error handling and cancellation recovery
ICoTask robust = LoadFromServerAsync()
    .AsCoTask()
    .Rescue<HttpRequestException>(ex => ShowError(ex).AsCoTask())
    .WhenCancelled(() => ShowCancelledMessage().AsCoTask());
```

## Documentation

For complete documentation, visit the [Caliburn.Light documentation](https://github.com/tibel/Caliburn.Light/tree/main/docs).

## License

Caliburn.Light is licensed under the [MIT license](https://github.com/tibel/Caliburn.Light/blob/main/LICENSE).
