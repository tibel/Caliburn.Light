using System;

namespace Caliburn.Light
{
    /// <summary>
    /// Used to manage logging.
    /// </summary>
    public static class LogManager
    {
        private static ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;

        /// <summary>
        /// Gets whether the <see cref="LogManager"/> is initialized.
        /// </summary>
        public static bool IsInitialized => !ReferenceEquals(_loggerFactory, NullLoggerFactory.Instance);

        /// <summary>
        /// Initializes the logging system.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        public static void Initialize(ILoggerFactory loggerFactory = null)
        {
            _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        }

        /// <summary>
        /// Creates an <see cref="ILogger"/> for the provided type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>A logger for the provided type.</returns>
        public static ILogger GetLogger(Type type)
        {
            return _loggerFactory.GetLogger(type);
        }
    }
}
