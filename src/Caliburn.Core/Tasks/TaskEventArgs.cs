using System;
using System.Threading.Tasks;

namespace Caliburn.Light
{
    /// <summary>
    /// Provides data for <see cref="TaskHelper"/> events.
    /// </summary>
    public sealed class TaskEventArgs : EventArgs
    {
        private readonly Task _task;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskEventArgs"/> class.
        /// </summary>
        /// <param name="task">The supplied Task.</param>
        public TaskEventArgs(Task task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            _task = task;
        }

        /// <summary>
        /// The supplied task.
        /// </summary>
        public Task Task
        {
            get { return _task; }
        }
    }
}
