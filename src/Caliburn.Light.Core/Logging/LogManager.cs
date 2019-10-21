using System;

namespace Caliburn.Light
{
    /// <summary>
    /// Used to manage logging.
    /// </summary>
    public static class LogManager
    {
        private static readonly Func<Type, ILogger> _getNullLogger = _ => NullLogger.Instance;

        private static Func<Type, ILogger> _getLogger;

        /// <summary>
        /// Gets whether the <see cref="LogManager"/> is initialized.
        /// </summary>
        public static bool IsInitialized => !ReferenceEquals(_getLogger, _getNullLogger);

        /// <summary>
        /// Initializes the logging system.
        /// </summary>
        /// <param name="getLogger">The function to create a <see cref="ILogger"/> for a type.</param>
        public static void Initialize(Func<Type, ILogger> getLogger = null)
        {
            _getLogger = getLogger ?? _getNullLogger;
        }

        /// <summary>
        /// Creates an <see cref="ILogger"/> for the provided type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>A logger for the provided type.</returns>
        public static ILogger GetLogger(Type type)
        {
            return _getLogger(type);
        }
    }
}
