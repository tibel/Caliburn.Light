using System.Windows;
using System.Windows.Threading;

namespace Caliburn.Light
{
    /// <summary>
    /// Inherit from this class in order to customize the configuration of the framework.
    /// </summary>
    public abstract class BootstrapperBase
    {
        private bool _isInitialized;

        /// <summary>
        /// The application.
        /// </summary>
        protected Application Application { get; private set; }

        /// <summary>
        /// Initialize the framework.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            try
            {
                ViewHelper.Initialize(ViewAdapter.Instance);

                Application = Application.Current;
                if (Application is object)
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
            Application.Startup += (_, e) => OnStartup(e);
            Application.DispatcherUnhandledException += (_, e) => OnUnhandledException(e);
            Application.Exit += (_, e) => OnExit(e);
        }

        /// <summary>
        /// Override to configure the framework and setup your IoC container.
        /// </summary>
        protected virtual void Configure()
        {
        }

        /// <summary>
        /// Override this to add custom behavior to execute when the application starts.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnStartup(StartupEventArgs e)
        {
        }

        /// <summary>
        /// Override this to add custom behavior to execute before the application shuts down.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnExit(ExitEventArgs e)
        {
        }

        /// <summary>
        /// Override this to add custom behavior for unhandled exceptions.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
        }
    }
}
