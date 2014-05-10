namespace Caliburn.Light
{
    /// <summary>
    /// Represents a command registered with the <see cref="ISettingsService" />
    /// </summary>
    public abstract class SettingsCommandBase
    {
        private readonly string _label;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsCommandBase"/> class.
        /// </summary>
        /// <param name="label">The label.</param>
        protected SettingsCommandBase(string label)
        {
            _label = label;
        }

        /// <summary>
        /// Gets the label.
        /// </summary>
        public string Label
        {
            get { return _label; }
        }

        /// <summary>
        /// Called when the command was selected in the Settings Charm.
        /// </summary>
        public abstract void OnSelected();
    }
}
