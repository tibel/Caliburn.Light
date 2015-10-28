using System.Windows.Input;

namespace Caliburn.Light
{
    /// <summary>
    /// Defines a command.
    /// </summary>
    public interface IDelegateCommand : ICommand
    {
        /// <summary>
        /// Raises <see cref="E:CanExecuteChanged"/> so every command invoker can requery to check if the command can execute.
        /// <remarks>Note that this will trigger the execution of <see cref="M:CanExecute"/> once for each invoker.</remarks>
        /// </summary>
        void RaiseCanExecuteChanged();

        /// <summary>
        /// Defines the method to be called when using x:Bind event binding.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="eventArgs">Event data for the event.</param>
        void OnEvent(object sender, object eventArgs);
    }
}
