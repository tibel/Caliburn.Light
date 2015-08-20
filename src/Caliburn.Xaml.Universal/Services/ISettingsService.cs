
namespace Caliburn.Light
{
    /// <summary>
    /// Service that handles the Settings Charm.
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Displays the Settings Charm pane to the user.
        /// </summary>
        void ShowSettingsUI();

        /// <summary>
        /// Registers a settings command with the service.
        /// </summary>
        /// <param name="command">The command to register.</param>
        void RegisterCommand(SettingsCommandBase command);
    }
}
