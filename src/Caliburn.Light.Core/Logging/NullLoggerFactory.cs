using System;

namespace Caliburn.Light
{
    /// <summary>
    /// A null-object <see cref="ILoggerFactory"/> implementation.
    /// </summary>
    public sealed class NullLoggerFactory : ILoggerFactory
    {
        /// <summary>
        /// Gets the single instance.
        /// </summary>
        public static readonly NullLoggerFactory Instance = new NullLoggerFactory();

        private static readonly ILogger _nullLoger = new NullLogger();

        private NullLoggerFactory()
        {
        }

        /// <summary>
        /// Creates an <see cref="ILogger"/> for the provided type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>A logger for the provided type.</returns>
        public ILogger GetLogger(Type type)
        {
            return _nullLoger;
        }

        private sealed class NullLogger : ILogger
        {
            public void Info(string format, params object[] args)
            {
            }

            public void Warn(string format, params object[] args)
            {
            }

            public void Error(string format, params object[] args)
            {
            }
        }
    }
}
