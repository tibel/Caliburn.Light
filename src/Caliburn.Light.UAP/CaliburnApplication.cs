using Windows.ApplicationModel;
using Windows.UI.Xaml;

namespace Caliburn.Light
{
    /// <summary>
    /// Encapsulates the application and its available services.
    /// </summary>
    public abstract class CaliburnApplication : Application
    {
        private bool _isInitialized;

        /// <summary>
        /// Start the framework.
        /// </summary>
        protected void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            ViewHelper.Initialize(new ViewAdapter());

            try
            {
                if (!DesignMode.DesignModeEnabled)
                    PrepareApplication();

                Configure();
            }
            catch
            {
                _isInitialized = false;
                throw;
            }
        }

        /// <summary>
        /// Provides an opportunity to hook into the application object.
        /// </summary>
        protected virtual void PrepareApplication()
        {
            Resuming += (_, e) => OnResuming(e);
            Suspending += (_, e) => OnSuspending(e);
            UnhandledException += (_, e) => OnUnhandledException(e);
        }

        /// <summary>
        /// Override to configure the framework and setup your IoC container.
        /// </summary>
        protected virtual void Configure()
        {
        }

        /// <summary>
        /// Override this to add custom behavior when the application transitions from Suspended state to Running state.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnResuming(object e)
        {
        }

        /// <summary>
        /// Override this to add custom behavior when the application transitions to Suspended state from some other state.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnSuspending(SuspendingEventArgs e)
        {
        }

        /// <summary>
        /// Override this to add custom behavior for unhandled exceptions.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnUnhandledException(UnhandledExceptionEventArgs e)
        {
        }
    }
}
