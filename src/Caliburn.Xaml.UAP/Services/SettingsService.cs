using System;
using System.Collections.Generic;
using System.Linq;
using Weakly;
using Windows.UI.ApplicationSettings;

namespace Caliburn.Light
{
    /// <summary>
    /// Serivce tha handles the settings charm
    /// </summary>
    public sealed class SettingsService : ISettingsService
    {
        private readonly List<SettingsCommandBase> _commands = new List<SettingsCommandBase>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsService" /> class.
        /// </summary>
        public SettingsService()
        {
            var settingsPane = SettingsPane.GetForCurrentView();
            settingsPane.CommandsRequested += OnCommandsRequested;
        }

        /// <summary>
        /// Displays the Settings Charm pane to the user.
        /// </summary>
        public void ShowSettingsUI()
        {
            SettingsPane.Show();
        }

        /// <summary>
        /// Registers a settings command with the service.
        /// </summary>
        /// <param name="command">The command to register.</param>
        public void RegisterCommand(SettingsCommandBase command)
        {
            _commands.Add(command);
        }

        /// <summary>
        /// Occurs when the user opens the settings pane.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="SettingsPaneCommandsRequestedEventArgs" /> instance containing the event data.</param>
        private void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            var settingsCommands = _commands.Select(c => new SettingsCommand(Guid.NewGuid(), c.Label, h => OnCommandSelected(c)));
            settingsCommands.ForEach(args.Request.ApplicationCommands.Add);
        }

        /// <summary>
        /// Called when a settings command was selected in the Settings Charm.
        /// </summary>
        /// <param name="command">The settings command.</param>
        private void OnCommandSelected(SettingsCommandBase command)
        {
            command.OnSelected();
        }
    }
}
