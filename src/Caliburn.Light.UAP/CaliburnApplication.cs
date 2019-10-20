using Windows.ApplicationModel;
using Windows.UI.Xaml;

namespace Caliburn.Light
{
    /// <summary>
    /// Encapsulates the app and its available services.
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

            UIContext.Initialize(new ViewAdapter());

            try
            {
                if (!ViewHelper.IsInDesignTool)
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
        /// Invoked when the application creates a window.
        /// </summary>
        /// <param name="args">Event data for the event.</param>
        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            base.OnWindowCreated(args);

            // Because dispatchers are tied to windows Execute will fail in 
            // scenarios when the app has multiple windows open (though contract 
            // activation, this keeps Excute up to date with the currently activated window
            args.Window.Activated += (s, e) => UIContext.Initialize(new ViewAdapter());
        }

        /// <summary>
        /// Provides an opportunity to hook into the application object.
        /// </summary>
        protected virtual void PrepareApplication()
        {
            Resuming += (s, e) => OnResuming(e);
            Suspending += (s, e) => OnSuspending(e);
            UnhandledException += (s, e) => OnUnhandledException(e);
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
        /// <param name="e">The event args.</param>
        protected virtual void OnResuming(object e)
        {
        }

        /// <summary>
        /// Override this to add custom behavior when the application transitions to Suspended state from some other state.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected virtual void OnSuspending(SuspendingEventArgs e)
        {
        }

        /// <summary>
        /// Override this to add custom behavior for unhandled exceptions.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected virtual void OnUnhandledException(UnhandledExceptionEventArgs e)
        {
        }
    }
}
