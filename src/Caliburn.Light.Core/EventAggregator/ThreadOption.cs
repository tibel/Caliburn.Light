
namespace Caliburn.Light
{
    /// <summary>
    /// Specifies on which thread a event subscriber will be called. 
    /// </summary>
    public enum ThreadOption
    {
        /// <summary>
        /// Use this setting to receive the event on the publishers' thread. This is the default setting.
        /// </summary>
        PublisherThread = 0,

        /// <summary>
        /// Use this setting to asynchronously receive the event on a .NET Framework thread-pool thread.
        /// </summary>
        BackgroundThread,

        /// <summary>
        /// Use this setting to receive the event on the UI thread.
        /// </summary>
        UIThread,
    }
}
