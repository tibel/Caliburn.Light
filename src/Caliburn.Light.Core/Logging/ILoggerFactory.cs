using System;

namespace Caliburn.Light
{
    /// <summary>
    /// Represents a type used to create instances of <see cref="ILogger"/>. 
    /// </summary>
    public interface ILoggerFactory
    {
        /// <summary>
        /// Creates an <see cref="ILogger"/> for the provided type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>A logger for the provided type.</returns>
        ILogger GetLogger(Type type);
    }
}
