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
        private static bool _isInDesignTool;

        /// <summary>
        /// Initializes the <see cref="UIContext"/>.
        /// </summary>
        /// <param name="isInDesignTool">Whether or not the framework is running in the context of a designer.</param>
        public static void Initialize(bool isInDesignTool)
        {
            if (_managedThreadId.HasValue)
                throw new InvalidOperationException("UIContext is already initialized.");

            _managedThreadId = Environment.CurrentManagedThreadId;
            _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            _isInDesignTool = isInDesignTool;
        }

        private static void VerifyInitialized()
        {
            if (!_managedThreadId.HasValue)
                throw new InvalidOperationException("UIContext is not initialized.");
        }

        /// <summary>
        /// Indicates whether or not the framework is running in the context of a designer.
        /// </summary>
        public static bool IsInDesignTool
        {
            get
            {
                VerifyInitialized();
                return _isInDesignTool;
            }
        }

        /// <summary>
        /// Determines whether the calling thread is the thread associated with the UI context. 
        /// </summary>
        /// <returns></returns>
        public static bool CheckAccess()
        {
            VerifyInitialized();
            return _managedThreadId == Environment.CurrentManagedThreadId;
        }

        /// <summary>
        /// The <see cref="TaskScheduler"/> associated with the UI context.
        /// </summary>
        public static TaskScheduler TaskScheduler
        {
            get
            {
                VerifyInitialized();
                return _taskScheduler;
            }
        }
    }
}
