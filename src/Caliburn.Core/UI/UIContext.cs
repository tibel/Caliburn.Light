using System;
using System.Threading.Tasks;

namespace Caliburn.Light
{
    /// <summary>
    /// Shared UI context
    /// </summary>
    public static class UIContext
    {
        private static int? _managedThreadId;
        private static TaskScheduler _taskScheduler;
        private static bool _isInDesignTool = true;
        private static IViewAdapter _viewAdapter;

        /// <summary>
        /// Initializes the <see cref="UIContext"/>.
        /// </summary>
        /// <param name="isInDesignTool">Whether or not the framework is running in the context of a designer.</param>
        /// <param name="viewAdapter"></param>
        public static void Initialize(bool isInDesignTool, IViewAdapter viewAdapter)
        {
            _managedThreadId = Environment.CurrentManagedThreadId;
            _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            _isInDesignTool = isInDesignTool;
            _viewAdapter = viewAdapter;
        }

        /// <summary>
        /// Gets the view interaction object.
        /// </summary>
        public static IViewAdapter ViewAdapter
        {
            get { return _viewAdapter ?? NullViewAdapter.Instance; }
        }

        /// <summary>
        /// Indicates whether or not the framework is running in the context of a designer.
        /// </summary>
        public static bool IsInDesignTool
        {
            get { return _isInDesignTool; }
        }

        /// <summary>
        /// Determines whether the calling thread is the thread associated with the UI context. 
        /// </summary>
        /// <returns></returns>
        public static bool CheckAccess()
        {
            return !_managedThreadId.HasValue || _managedThreadId == Environment.CurrentManagedThreadId;
        }

        /// <summary>
        /// The <see cref="TaskScheduler"/> associated with the UI context.
        /// </summary>
        public static TaskScheduler TaskScheduler
        {
            get { return _taskScheduler ?? TaskScheduler.Current; }
        }
    }
}
