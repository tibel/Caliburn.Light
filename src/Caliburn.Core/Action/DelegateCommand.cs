
namespace Caliburn.Light
{
    /// <summary>
    /// Builds an <see cref="IDelegateCommand"/> in a strongly typed fashion.
    /// </summary>
    public static class DelegateCommand
    {
        /// <summary>
        /// Gets a <see cref="DelegateCommandBuilder&lt;TTarget, Object&gt;"/>
        /// </summary>
        /// <typeparam name="TTarget">The type of the command target.</typeparam>
        /// <returns>The command builder.</returns>
        public static DelegateCommandBuilder<TTarget, object> For<TTarget>()
            where TTarget : class
        {
            return new DelegateCommandBuilder<TTarget, object>();
        }

        /// <summary>
        /// Gets a <see cref="DelegateCommandBuilder&lt;TTarget, TParameter&gt;"/>
        /// </summary>
        /// <typeparam name="TTarget">The type of the command target.</typeparam>
        /// <typeparam name="TParameter">The type of the command parameter.</typeparam>
        /// <returns>The command builder.</returns>
        public static DelegateCommandBuilder<TTarget, TParameter> For<TTarget, TParameter>()
            where TTarget : class
        {
            return new DelegateCommandBuilder<TTarget, TParameter>();
        }
    }
}
