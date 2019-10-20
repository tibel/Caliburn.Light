﻿using System;

namespace Caliburn.Light
{
    /// <summary>
    /// The exception that is thrown when a <see cref="SuspensionManager"/> operation failed.
    /// </summary>
    public class SuspensionManagerException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SuspensionManagerException"/> class.
        /// </summary>
        public SuspensionManagerException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SuspensionManagerException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SuspensionManagerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SuspensionManagerException"/> class with a specified error message 
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public SuspensionManagerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
