namespace Caliburn.Light
{
    /// <summary>
    /// Service that handles sharing data with the Share Charm.
    /// </summary>
    public interface ISharingService
    {
        /// <summary>
        /// Programmatically initiates the user interface for sharing content with another application.
        /// </summary>
        void ShowShareUI();
    }
}
