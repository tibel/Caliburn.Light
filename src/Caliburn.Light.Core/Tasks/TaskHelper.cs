using System.Threading.Tasks;

namespace Caliburn.Light
{
    internal static class TaskHelper
    {
        public static async void Observe(this Task task)
        {
            await task.ConfigureAwait(false);
        }
    }
}
