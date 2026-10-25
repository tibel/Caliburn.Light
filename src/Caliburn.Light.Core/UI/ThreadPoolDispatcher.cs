using System;
using System.Threading;

namespace Caliburn.Light;

/// <summary>
/// Dispatches actions to the .NET thread pool.
/// </summary>
public sealed class ThreadPoolDispatcher : IDispatcher
{
    /// <summary>
    /// Gets the shared <see cref="ThreadPoolDispatcher"/> instance.
    /// </summary>
    public static readonly ThreadPoolDispatcher Instance = new ThreadPoolDispatcher();

    private ThreadPoolDispatcher()
    {
    }

    /// <inheritdoc />
    public void BeginInvoke(Action action) => ThreadPool.QueueUserWorkItem(static state => ((Action)state!).Invoke(), action);

    /// <inheritdoc />
    public bool CheckAccess() => Thread.CurrentThread.IsThreadPoolThread;
}
