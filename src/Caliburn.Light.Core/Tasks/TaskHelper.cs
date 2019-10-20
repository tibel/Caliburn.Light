using System.Threading.Tasks;

namespace Caliburn.Light
{
    /// <summary>
    /// Helper to create completed, canceled and faulted tasks.
    /// </summary>
    internal static class TaskHelper
    {
        public static readonly Task<bool> TrueTask = Task.FromResult(true);

        public static readonly Task CompletedTask = TrueTask;
    }
}
