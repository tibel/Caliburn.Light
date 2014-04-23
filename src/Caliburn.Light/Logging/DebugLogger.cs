#define DEBUG

using System;
using System.Diagnostics;

namespace Caliburn.Light
{
    /// <summary>
    /// A simple logger thats logs everything to the debugger.
    /// </summary>
    public sealed class DebugLogger : ILogger
    {
        private readonly string _typeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugLogger"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public DebugLogger(Type type)
        {
            _typeName = type.FullName;
        }

        /// <summary>
        /// Logs the message as info.
        /// </summary>
        /// <param name="format">A formatted message.</param>
        /// <param name="args">Parameters to be injected into the formatted message.</param>
        public void Info(string format, params object[] args)
        {
            Debug.WriteLine("[{1}] INFO: {0}", string.Format(format, args), _typeName);
        }

        /// <summary>
        /// Logs the message as warning.
        /// </summary>
        /// <param name="format">A formatted message.</param>
        /// <param name="args">Parameters to be injected into the formatted message.</param>
        public void Warn(string format, params object[] args)
        {
            Debug.WriteLine("[{1}] WARN: {0}", string.Format(format, args), _typeName);
        }

        /// <summary>
        /// Logs the message as error.
        /// </summary>
        /// <param name="format">A formatted message.</param>
        /// <param name="args">Parameters to be injected into the formatted message.</param>
        public void Error(string format, params object[] args)
        {
            Debug.WriteLine("[{1}] ERROR: {0}", string.Format(format, args), _typeName);
        }
    }
}
