using System;
using System.Collections.Generic;

namespace Caliburn.Light
{
    /// <summary>
    /// Represents a flyout command registered with the <see cref="ISettingsService" />.
    /// </summary>
    public class FlyoutSettingsCommand : SettingsCommandBase
    {
        private readonly Type _viewModelType;
        private readonly IDictionary<string, object> _viewSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlyoutSettingsCommand" /> class.
        /// </summary>
        /// <param name="label">The label to use in the settings charm.</param>
        /// <param name="viewModelType">The view model to display.</param>
        /// <param name="viewSettings">Additional settings to pass to the <see cref="ISettingsWindowManager" />.</param>
        public FlyoutSettingsCommand(string label, Type viewModelType, IDictionary<string, object> viewSettings)
            : base(label)
        {
            _viewModelType = viewModelType;
            _viewSettings = viewSettings;
        }

        /// <summary>
        /// The view model to display.
        /// </summary>
        public Type ViewModelType
        {
            get { return _viewModelType; }
        }

        /// <summary>
        /// Additional settings to pass to the <see cref="ISettingsWindowManager" />.
        /// </summary>
        public IDictionary<string, object> ViewSettings
        {
            get { return _viewSettings; }
        }

        /// <summary>
        /// Called when the command was selected in the Settings Charm.
        /// </summary>
        public override void OnSelected()
        {
            var viewModel = IoC.GetInstance(ViewModelType);
            if (viewModel == null) return;

            var windowManager = IoC.GetInstance<ISettingsWindowManager>();
            windowManager.ShowSettingsFlyout(viewModel, Label, ViewSettings);
        }
    }
}
