using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Caliburn.Light.WPF
{
    /// <summary>
    /// A Caliburn.Light CoTask that lets you open a file.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public abstract class OpenFileDialogCoTask<TResult> : CoTask, ICoTask<TResult>
    {
        private readonly bool _multiselect;
        private readonly string _title;
        private string _fileTypeFilter;
        private string _initialDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenFileDialogCoTask{TResult}"/> class.
        /// </summary>
        /// <param name="multiselect">Determines whether it is allows to select multiple files.</param>
        /// <param name="title">The title of the dialog.</param>
        protected OpenFileDialogCoTask(bool multiselect, string title = null)
        {
            _multiselect = multiselect;
            _title = title;
        }

        /// <summary>
        /// Gets the opened file(s).
        /// </summary>
        public TResult Result { get; protected set; }

        /// <summary>
        /// Executes the result using the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void BeginExecute(CoroutineExecutionContext context)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.RestoreDirectory = true;
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;

            openFileDialog.Multiselect = _multiselect;
            openFileDialog.Filter = _fileTypeFilter;
            openFileDialog.Title = _title;
            openFileDialog.InitialDirectory = _initialDirectory;

            var activeWindow = CoTaskHelper.GetActiveWindow(context);

            bool fileSelected;
            try
            {
                fileSelected = openFileDialog.ShowDialog(activeWindow).GetValueOrDefault();
            }
            catch
            {
                if (string.IsNullOrEmpty(openFileDialog.InitialDirectory)) throw;
                openFileDialog.InitialDirectory = null;
                fileSelected = openFileDialog.ShowDialog(activeWindow).GetValueOrDefault();
            }

            OnCompleted(openFileDialog, new CoTaskCompletedEventArgs(null, !fileSelected));
        }

        /// <summary>
        /// Handles the completion of the execution.
        /// </summary>
        /// <param name="openFileDialog">The open file dialog.</param>
        /// <param name="args">The <see cref="CoTaskCompletedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCompleted(OpenFileDialog openFileDialog, CoTaskCompletedEventArgs args)
        {
            OnCompleted(args);
        }

        /// <summary>
        /// Create file filter for the dialog.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public OpenFileDialogCoTask<TResult> FilterFiles(string filter)
        {
            _fileTypeFilter = filter;
            return this;
        }

        /// <summary>
        /// Sets the initial <paramref name = "directory" /> of the dialog
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <returns></returns>
        public OpenFileDialogCoTask<TResult> In(string directory)
        {
            _initialDirectory = directory;
            return this;
        }
    }

    /// <summary>
    /// A Caliburn.Light CoTask that lets you open a file.
    /// </summary>
    public static class OpenFileDialogCoTask
    {
        /// <summary>
        /// Open a single file.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <returns></returns>
        public static OpenFileDialogCoTask<FileInfo> OneFile(string title = null)
        {
            return new OneFileCoTask(title);
        }

        /// <summary>
        /// Open multiple files.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <returns></returns>
        public static OpenFileDialogCoTask<IEnumerable<FileInfo>> MultipleFiles(string title = null)
        {
            return new MultipleFilesCoTask(title);
        }

        private sealed class OneFileCoTask : OpenFileDialogCoTask<FileInfo>
        {
            public OneFileCoTask(string title)
                : base(false, title)
            {
            }

            protected override void OnCompleted(OpenFileDialog openFileDialog, CoTaskCompletedEventArgs args)
            {
                if (!args.WasCancelled)
                    Result = new FileInfo(openFileDialog.FileName);

                base.OnCompleted(openFileDialog, args);
            }
        }

        private sealed class MultipleFilesCoTask : OpenFileDialogCoTask<IEnumerable<FileInfo>>
        {
            public MultipleFilesCoTask(string title)
                : base(true, title)
            {
                Result = new FileInfo[0];
            }

            protected override void OnCompleted(OpenFileDialog openFileDialog, CoTaskCompletedEventArgs args)
            {
                if (!args.WasCancelled)
                    Result = openFileDialog.FileNames.Select(name => new FileInfo(name)).ToArray();

                base.OnCompleted(openFileDialog, args);
            }
        }
    }
}
