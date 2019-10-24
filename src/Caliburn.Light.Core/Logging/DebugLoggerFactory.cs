#define DEBUG

using System;
using System.Diagnostics;

namespace Caliburn.Light
{
    /// <summary>
    /// A logger factory for a simple logger thats logs everything to the debugger.
    /// </summary>
    public sealed class DebugLoggerFactory : ILoggerFactory
    {
        /// <summary>
        /// Creates an <see cref="ILogger"/> for the provided type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>A logger for the provided type.</returns>
        public ILogger GetLogger(Type type)
        {
            return new DebugLogger(type);
        }

        private sealed class DebugLogger : ILogger
        {
            private readonly string _typeName;

            public DebugLogger(Type type)
            {
                _typeName = type.FullName;
            }

            public void Info(string format, params object[] args)
            {
                Debug.WriteLine("[{1}] INFO: {0}", string.Format(format, args), _typeName);
            }

            public void Warn(string format, params object[] args)
            {
                Debug.WriteLine("[{1}] WARN: {0}", string.Format(format, args), _typeName);
            }

            public void Error(string format, params object[] args)
            {
                Debug.WriteLine("[{1}] ERROR: {0}", string.Format(format, args), _typeName);
            }
        }
    }
}
