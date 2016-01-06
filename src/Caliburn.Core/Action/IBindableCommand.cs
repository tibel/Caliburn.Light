using System.ComponentModel;
using System.Windows.Input;

namespace Caliburn.Light
{
    /// <summary>
    /// Defines a command, that can be data-bound.
    /// </summary>
    public interface IBindableCommand : ICommand, INotifyPropertyChanged
    {
        /// <summary>
        /// Defines the method to be called when using x:Bind event binding.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="eventArgs">Event data for the event.</param>
        void OnEvent(object sender, object eventArgs);

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        bool IsExecutable { get; }
    }
}
