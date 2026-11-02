namespace Caliburn.Light;

/// <summary>
/// Specifies the thread on which an event aggregator handler is executed.
/// </summary>
public enum EventAggregatorExecution
{
    /// <summary>
    /// Executes the handler on the thread that published the message.
    /// </summary>
    PublisherThread = 0,

    /// <summary>
    /// Executes the handler on a thread pool thread.
    /// </summary>
    ThreadPool,

    /// <summary>
    /// Executes the handler on the UI thread associated with its target's view.
    /// If no view can be found, executes on the publishing thread.
    /// </summary>
    UIThread
}
