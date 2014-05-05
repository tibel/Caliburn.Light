
namespace Caliburn.Light
{
    /// <summary>
    /// A logger.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs the message as info.
        /// </summary>
        /// <param name="format">A formatted message.</param>
        /// <param name="args">Parameters to be injected into the formatted message.</param>
        void Info(string format, params object[] args);

        /// <summary>
        /// Logs the message as warning.
        /// </summary>
        /// <param name="format">A formatted message.</param>
        /// <param name="args">Parameters to be injected into the formatted message.</param>
        void Warn(string format, params object[] args);

        /// <summary>
        /// Logs the message as error.
        /// </summary>
        /// <param name="format">A formatted message.</param>
        /// <param name="args">Parameters to be injected into the formatted message.</param>
        void Error(string format, params object[] args);
    }
}
