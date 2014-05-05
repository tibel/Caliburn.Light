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
        private static IUIView _uiView;

        /// <summary>
        /// Initializes the <see cref="UIContext"/>.
        /// </summary>
        /// <param name="isInDesignTool">Whether or not the framework is running in the context of a designer.</param>
        /// <param name="uiView"></param>
        public static void Initialize(bool isInDesignTool, IUIView uiView)
        {
            _managedThreadId = Environment.CurrentManagedThreadId;
            _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            _isInDesignTool = isInDesignTool;
            _uiView = uiView;
        }

        /// <summary>
        /// Gets the view interaction object.
        /// </summary>
        public static IUIView View
        {
            get { return _uiView ?? NullUIView.Instance; }
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
