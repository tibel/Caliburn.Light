using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Caliburn.Light
{
    internal sealed class DispatcherTaskScheduler : TaskScheduler
    {
        private readonly Dispatcher _dispatcher;
        private readonly Action<Task> _tryExecuteTask;

        public DispatcherTaskScheduler(Dispatcher dispatcher)
        {
            if (dispatcher is null)
                throw new ArgumentNullException(nameof(dispatcher));

            _dispatcher = dispatcher;
            _tryExecuteTask = task => TryExecuteTask(task);
        }

        public override int MaximumConcurrencyLevel => 1;

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return null;
        }

        protected override void QueueTask(Task task)
        {
            _dispatcher.BeginInvoke(_tryExecuteTask, task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return _dispatcher.CheckAccess() && TryExecuteTask(task);
        }
    }
}
