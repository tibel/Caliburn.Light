using System;
using System.Windows;

namespace Caliburn.Light
{
    /// <summary>
    /// Available message results.
    /// </summary>
    public enum MessageResult
    {
        /// <summary>
        /// No result available.
        /// </summary>
        None = 0,

        /// <summary>
        /// Message is acknowledged.
        /// </summary>
        OK = 1,

        /// <summary>
        /// Message is canceled.
        /// </summary>
        Cancel = 2,

        /// <summary>
        /// Message is acknowledged with yes.
        /// </summary>
        Yes = 6,

        /// <summary>
        /// Message is acknowledged with no.
        /// </summary>
        No = 7
    }

    /// <summary>
    /// Available message buttons.
    /// </summary>
    public enum MessageButton
    {
        /// <summary>
        /// OK button.
        /// </summary>
        OK = 0,

        /// <summary>
        /// OK and Cancel buttons.
        /// </summary>
        OKCancel = 1,

        /// <summary>
        /// Yes, No and Cancel buttons.
        /// </summary>
        YesNoCancel = 3,

        /// <summary>
        /// Yes and No buttons.
        /// </summary>
        YesNo = 4,
    }

    /// <summary>
    /// Available message images.
    /// </summary>
    public enum MessageImage
    {
        /// <summary>
        /// Show no image.
        /// </summary>
        None = 0,

        /// <summary>
        /// Error image.
        /// </summary>
        Error = 16,

        /// <summary>
        /// Question image.
        /// </summary>
        Question = 32,

        /// <summary>
        /// Warning image.
        /// </summary>
        Warning = 48,

        /// <summary>
        /// Information image.
        /// </summary>
        Information = 64,
    }

    /// <summary>
    /// A Caliburn.Light CoTask that lets you show messages.
    /// </summary>
    public class MessageBoxCoTask : CoTask, ICoTask<MessageResult>
    {
        private readonly string _message;
        private string _caption = string.Empty;
        private MessageButton _button = MessageButton.OK;
        private MessageImage _image = MessageImage.None;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBoxCoTask"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public MessageBoxCoTask(string message)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException("message");

            _message = message;
            Result = MessageResult.None;
        }

        /// <summary>
        /// Gets the message
        /// </summary>
        public MessageResult Result { get; protected set; }

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
        public MessageBoxCoTask Buttons(MessageButton buttons = MessageButton.OK)
        {
            _button = buttons;
            return this;
        }

        /// <summary>
        /// Sets the image.
        /// </summary>
        /// <param name="icon">The image.</param>
        /// <returns></returns>
        public MessageBoxCoTask Image(MessageImage icon = MessageImage.None)
        {
            _image = icon;
            return this;
        }

        private static MessageResult TranslateMessageBoxResult(MessageBoxResult result)
        {
            var value = result.ToString();
            return (MessageResult) Enum.Parse(typeof (MessageResult), value, true);
        }

        private static MessageBoxImage TranslateMessageImage(MessageImage image)
        {
            var value = image.ToString();
            return (MessageBoxImage) Enum.Parse(typeof (MessageBoxImage), value, true);
        }

        private static MessageBoxButton TranslateMessageButton(MessageButton button)
        {
            var value = button.ToString();
            return (MessageBoxButton) Enum.Parse(typeof (MessageBoxButton), value, true);
        }

        /// <summary>
        /// Executes the result using the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void BeginExecute(CoroutineExecutionContext context)
        {
            MessageBoxResult result;
            var messageBoxButton = TranslateMessageButton(_button);
            var messageBoxImage = TranslateMessageImage(_image);

            var activeWindow = CoTaskHelper.GetActiveWindow(context);
            if (activeWindow != null)
            {
                result = MessageBox.Show(activeWindow, _message, _caption, messageBoxButton, messageBoxImage);
            }
            else
            {
                result = MessageBox.Show(_message, _caption, messageBoxButton, messageBoxImage);
            }

            Result = TranslateMessageBoxResult(result);
            OnCompleted(new CoTaskCompletedEventArgs(null, false));
        }
    }
}
