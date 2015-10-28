using System;

namespace Caliburn.Light
{
    /// <summary>
    /// Builds an <see cref="IDelegateCommand"/> in a strongly typed fashion.
    /// </summary>
    public static class DelegateCommand
    {
        /// <summary>
        /// Gets a <see cref="DelegateCommandBuilder"/>.
        /// </summary>
        /// <returns>The command builder.</returns>
        public static DelegateCommandBuilder NoParameter()
        {
            return new DelegateCommandBuilder();
        }

        /// <summary>
        /// Gets a <see cref="DelegateCommandBuilder&lt;TParameter&gt;"/>.
        /// </summary>
        /// <typeparam name="TParameter">The type of the command parameter.</typeparam>
        /// <param name="coerceParameter">The function to coerce the provided value to <typeparamref name="TParameter"/>.</param>
        /// <returns>The command builder.</returns>
        public static DelegateCommandBuilder<TParameter> WithParameter<TParameter>(Func<object, TParameter> coerceParameter = null)
        {
            return new DelegateCommandBuilder<TParameter>(coerceParameter);
        }
    }
}
