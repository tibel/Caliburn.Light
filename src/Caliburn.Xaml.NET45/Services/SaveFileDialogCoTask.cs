using Microsoft.Win32;
using System;
using System.IO;

namespace Caliburn.Light
{
    /// <summary>
    /// A Caliburn.Light CoTask that lets you save a file.
    /// </summary>
    public sealed class SaveFileDialogCoTask : CoTask, ICoTask<FileInfo>
    {
        private readonly string _title;
        private string _fileName;
        private string _fileTypeFilter;
        private string _initialDirectory;
        private string _defaultFileExtension;
        private bool _promptForOverwrite;
        private bool _promptForCreate;

        /// <summary>
        /// Gets the opened file(s).
        /// </summary>
        public FileInfo Result { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveFileDialogCoTask"/> class.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        private SaveFileDialogCoTask(string title = null)
        {
            _title = title;
        }

        /// <summary>
        /// Executes the result using the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void BeginExecute(CoroutineExecutionContext context)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.AddExtension = true;
            saveFileDialog.CheckPathExists = true;

            saveFileDialog.Filter = _fileTypeFilter;
            saveFileDialog.DefaultExt = _defaultFileExtension;

            saveFileDialog.Title = _title;
            saveFileDialog.FileName = _fileName;
            saveFileDialog.CreatePrompt = _promptForCreate;
            saveFileDialog.OverwritePrompt = _promptForOverwrite;

            var activeWindow = CoTaskHelper.GetActiveWindow(context);

            bool fileSelected;
            try
            {
                saveFileDialog.InitialDirectory = _initialDirectory;
                fileSelected = saveFileDialog.ShowDialog(activeWindow).GetValueOrDefault();
            }
            catch (FileNotFoundException)
            {
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                fileSelected = saveFileDialog.ShowDialog(activeWindow).GetValueOrDefault();
            }

            if (fileSelected)
                Result = new FileInfo(saveFileDialog.FileName);

            OnCompleted(new CoTaskCompletedEventArgs(null, !fileSelected));
        }

        /// <summary>
        /// Create file filter for the dialog.
        /// </summary>
        /// <param name="filter">The file type filter.</param>
        /// <param name="defaultExtension">The default file name extension applied to files that are saved.</param>
        /// <returns></returns>
        public SaveFileDialogCoTask FilterFiles(string filter, string defaultExtension)
        {
            _fileTypeFilter = filter;
            _defaultFileExtension = defaultExtension;
            return this;
        }

        /// <summary>
        /// Sets the initial <paramref name = "directory" /> of the dialog
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <returns></returns>
        public SaveFileDialogCoTask In(string directory)
        {
            _initialDirectory = directory;
            return this;
        }

        /// <summary>
        /// Sets the initial <paramref name="fileName" /> of the dialog
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns></returns>
        public SaveFileDialogCoTask FileName(string fileName)
        {
            _fileName = fileName;
            return this;
        }

        /// <summary>
        /// Ask the user for permission if the file will be overriden.
        /// </summary>
        /// <returns></returns>
        public SaveFileDialogCoTask PromptForOverwrite()
        {
            _promptForOverwrite = true;
            return this;
        }

        /// <summary>
        /// Ask the user for permission if a new file will be created.
        /// </summary>
        /// <returns></returns>
        public SaveFileDialogCoTask PromptForCreate()
        {
            _promptForCreate = true;
            return this;
        }

        /// <summary>
        /// Save a single file.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <returns></returns>
        public static SaveFileDialogCoTask OneFile(string title = null)
        {
            return new SaveFileDialogCoTask(title);
        }
    }
}
