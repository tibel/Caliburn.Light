using System;
using System.Threading.Tasks;

namespace Caliburn.Light
{
    /// <summary>
    /// Subsystem to observe asynchronous operations.
    /// </summary>
    public static class AsyncSubsystem
    {
        /// <summary>
        /// Occurs when a <see cref="Task"/> is created.
        /// </summary>
        public static event EventHandler<TaskCreatedEventArgs> TaskCreated;

        /// <summary>
        /// Adds the <paramref name="task"/> to the subsystem.
        /// </summary>
        /// <param name="task">The task.</param>
        public static void AddTask(Task task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            var handler = TaskCreated;
            if (handler != null)
                handler(null, new TaskCreatedEventArgs(task));
        }
    }
}
