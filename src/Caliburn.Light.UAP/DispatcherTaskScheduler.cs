using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;

namespace Caliburn.Light
{
    internal sealed class DispatcherTaskScheduler : TaskScheduler
    {
        private readonly CoreDispatcher _dispatcher;

        public DispatcherTaskScheduler(CoreDispatcher dispatcher)
        {
            if (dispatcher is null)
                throw new ArgumentNullException(nameof(dispatcher));

            _dispatcher = dispatcher;
        }

        public override int MaximumConcurrencyLevel => 1;

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return null;
        }

        protected override void QueueTask(Task task)
        {
            var asyncAction = _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => TryExecuteTask(task));
            Observe(asyncAction);
        }

        private static async void Observe(IAsyncAction asyncAction)
        {
            await asyncAction;
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return _dispatcher.HasThreadAccess && TryExecuteTask(task);
        }
    }
}
