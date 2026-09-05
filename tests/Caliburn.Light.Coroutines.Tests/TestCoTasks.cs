using System.Collections;
using System.Collections.Generic;
using Caliburn.Light;

namespace Caliburn.Light.Coroutines.Tests;

internal sealed class TestCoTask : ICoTask
{
    public event EventHandler<CoTaskCompletedEventArgs>? Completed;

    public CommandExecutionContext? Context { get; private set; }

    public void BeginExecute(CommandExecutionContext context)
    {
        Context = context;
    }

    public void Complete(Exception? error = null, bool wasCancelled = false)
    {
        Completed?.Invoke(this, new CoTaskCompletedEventArgs(error, wasCancelled));
    }
}

internal sealed class ThrowingEnumerator : IEnumerator<ICoTask>
{
    public ICoTask Current => throw new InvalidOperationException("Current is not available.");

    object IEnumerator.Current => Current;

    public bool MoveNext()
    {
        throw new InvalidOperationException("enumerator failure");
    }

    public void Reset() => throw new NotSupportedException();

    public void Dispose()
    {
    }
}
