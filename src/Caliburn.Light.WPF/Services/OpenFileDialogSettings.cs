namespace Caliburn.Light.WPF
{
    /// <summary>
    /// Settings for a file open dialog.
    /// </summary>
    public sealed class OpenFileDialogSettings
    {
        /// <summary>
        /// Gets or sets the dialog title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets whether it is allows to select multiple files.
        /// </summary>
        public bool Multiselect { get; set; }

        /// <summary>
        /// Gets or sets the file type filter.
        /// </summary>
        public string FileTypeFilter { get; set; }

        /// <summary>
        /// Gets or sets the initial directory.
        /// </summary>
        public string InitialDirectory { get; set; }
    }
}
