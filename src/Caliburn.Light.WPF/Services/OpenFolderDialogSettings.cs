namespace Caliburn.Light.WPF
{
    /// <summary>
    /// Settings for an open folder dialog.
    /// </summary>
    public sealed class OpenFolderDialogSettings
    {
        /// <summary>
        /// Gets or sets the dialog title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the initial directory.
        /// </summary>
        public string InitialDirectory { get; set; } = string.Empty;
    }
}
