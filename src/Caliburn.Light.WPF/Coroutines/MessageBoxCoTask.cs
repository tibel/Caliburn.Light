using System;
using System.Windows;

namespace Caliburn.Light.WPF
{
    /// <summary>
    /// A Caliburn.Light CoTask that lets you show messages.
    /// </summary>
    public sealed class MessageBoxCoTask : CoTask, ICoTask<MessageBoxResult>
    {
        private readonly string _message;
        private string _caption = string.Empty;
        private MessageBoxButton _button = MessageBoxButton.OK;
        private MessageBoxImage _image = MessageBoxImage.None;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBoxCoTask"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public MessageBoxCoTask(string message)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException(nameof(message));

            _message = message;
            Result = MessageBoxResult.None;
        }

        /// <summary>
        /// Gets the message
        /// </summary>
        public MessageBoxResult Result { get; private set; }

        /// <summary>
        /// Sets the caption.
        /// </summary>
        /// <param name="text">The caption text.</param>
        /// <returns></returns>
        public MessageBoxCoTask Caption(string text = "")
        {
            _caption = text;
            return this;
        }

        /// <summary>
        /// Sets the button.
        /// </summary>
        /// <param name="buttons">The button.</param>
        /// <returns></returns>
        public MessageBoxCoTask Buttons(MessageBoxButton buttons = MessageBoxButton.OK)
        {
            _button = buttons;
            return this;
        }

        /// <summary>
        /// Sets the image.
        /// </summary>
        /// <param name="icon">The image.</param>
        /// <returns></returns>
        public MessageBoxCoTask Image(MessageBoxImage icon = MessageBoxImage.None)
        {
            _image = icon;
            return this;
        }

        /// <summary>
        /// Executes the result using the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void BeginExecute(CoroutineExecutionContext context)
        {
            var activeWindow = CoTaskHelper.GetActiveWindow(context);
            if (activeWindow is object)
            {
                Result = MessageBox.Show(activeWindow, _message, _caption, _button, _image);
            }
            else
            {
                Result = MessageBox.Show(_message, _caption, _button, _image);
            }

            OnCompleted(new CoTaskCompletedEventArgs(null, false));
        }
    }
}
