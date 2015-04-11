using System;
using System.Threading.Tasks;

namespace Caliburn.Light
{
    /// <summary>
    /// Provides data for the event that is raised when a <see cref="Task"/> is created.
    /// </summary>
    public sealed class TaskCreatedEventArgs : EventArgs
    {
        private readonly Task _task;

        /// <summary> 
        /// Initializes a new instance of the <see cref="TaskCreatedEventArgs"/> class.
        /// </summary>
        /// <param name="task">The created Task.</param> 
        public TaskCreatedEventArgs(Task task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            _task = task;
        }

        /// <summary>
        /// The created task. 
        /// </summary>
        public Task Task
        {
            get { return _task; }
        }
    }
}
