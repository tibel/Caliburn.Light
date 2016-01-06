
namespace Caliburn.Light
{
    /// <summary>
    /// Defines a delegate command.
    /// </summary>
    public interface IDelegateCommand : IBindableCommand
    {
        /// <summary>
        /// Raises <see cref="E:CanExecuteChanged"/> so every command invoker can requery to check if the command can execute.
        /// </summary>
        /// <remarks>Note that this will trigger the execution of <see cref="M:CanExecute"/> once for each invoker.</remarks>
        void RaiseCanExecuteChanged();

        /// <summary>
        /// Determines whether the command is currently executing.
        /// </summary>
        bool IsExecuting { get; }
    }
}
