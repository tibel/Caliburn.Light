using System;
using System.Collections.Generic;

namespace Caliburn.Light;

internal sealed class SequentialCoTask : ICoTask
{
    private readonly IEnumerator<ICoTask> _enumerator;
    private CommandExecutionContext? _context;
    private bool _executing;
    private CoTaskCompletedEventArgs? _pendingArgs;

    public SequentialCoTask(IEnumerator<ICoTask> enumerator)
    {
        ArgumentNullException.ThrowIfNull(enumerator);

        _enumerator = enumerator;
    }

    public void BeginExecute(CommandExecutionContext context)
    {
        _context = context;
        ExecuteNext();
    }

    public event EventHandler<CoTaskCompletedEventArgs>? Completed;

    private void ExecuteNext()
    {
        _executing = true;

        try
        {
            while (true)
            {
                bool moveNextSucceeded;
                try
                {
                    moveNextSucceeded = _enumerator.MoveNext();
                }
                catch (Exception ex)
                {
                    OnCompleted(ex, false);
                    return;
                }

                if (!moveNextSucceeded)
                {
                    OnCompleted(null, false);
                    return;
                }

                var next = _enumerator.Current;
                if (next is null)
                    continue;

                _pendingArgs = null;
                try
                {
                    next.Completed += OnChildCompleted;
                    next.BeginExecute(_context!);
                }
                catch (Exception ex)
                {
                    next.Completed -= OnChildCompleted;
                    _pendingArgs = new CoTaskCompletedEventArgs(ex, false);
                }

                // async completion - will resume via ChildCompleted callback
                if (_pendingArgs is null)
                    return;

                // sync completion with error/cancel - stop the sequence
                if (_pendingArgs.WasCancelled || _pendingArgs.Error is not null)
                {
                    OnCompleted(_pendingArgs);
                    return;
                }

                // sync completion with success - continue to next child
            }
        }
        finally
        {
            _executing = false;

            // Handle async completion that arrived while still in the loop
            var pendingArgs = _pendingArgs;
            if (pendingArgs is not null)
            {
                _pendingArgs = null;

                if (pendingArgs.WasCancelled || pendingArgs.Error is not null)
                    OnCompleted(pendingArgs);
                else
                    ExecuteNext();
            }
        }
    }

    private void OnChildCompleted(object? sender, CoTaskCompletedEventArgs args)
    {
        ((ICoTask)sender!).Completed -= OnChildCompleted;

        if (_executing)
        {
            _pendingArgs = args;
            return;
        }

        if (args.WasCancelled || args.Error is not null)
        {
            OnCompleted(args);
            return;
        }

        ExecuteNext();
    }

    private void OnCompleted(Exception? error, bool wasCancelled)
    {
        OnCompleted(new CoTaskCompletedEventArgs(error, wasCancelled));
    }

    private void OnCompleted(CoTaskCompletedEventArgs args)
    {
        _context = null;
        _enumerator.Dispose();
        Completed?.Invoke(this, args);
    }
}
