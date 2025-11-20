using System;
using System.Threading.Tasks;

namespace Caliburn.Light;

/// <summary>
/// Helper for asynchronous methods.
/// </summary>
public static class TaskHelper
{
    /// <summary>
    /// A completed boolean task with true.
    /// </summary>
    public static readonly Task<bool> TrueTask = Task.FromResult(true);

    /// <summary>
    /// A completed boolean task with false.
    /// </summary>
    public static readonly Task<bool> FalseTask = Task.FromResult(false);

    /// <summary>
    /// Awaits a task to observe the exception.
    /// </summary>
    /// <param name="task">The task to observe.</param>
    public static async void Observe(this Task task)
    {
        await task.ConfigureAwait(false);
    }

    /// <summary>
    /// Occurs when <see cref="AsyncCommand.ExecuteAsync(object)"/> or <see cref="IEventAggregatorHandler.HandleAsync(object)"/> is invoked and the operation has not completed synchronously.
    /// </summary>
    public static event EventHandler<TaskEventArgs>? Executing;

    /// <summary>
    /// Triggers the <see cref="Executing"/> event for the supplied <paramref name="task"/>.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="task">The running task.</param>
    public static void NotifyExecuting(object? sender, Task task)
    {
        Executing?.Invoke(sender, new TaskEventArgs(task));
    }
}
