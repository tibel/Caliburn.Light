
namespace Caliburn.Light
{
    /// <summary>
    /// Builds an <see cref="IDelegateCommand"/> in a strongly typed fashion.
    /// </summary>
    public static class DelegateCommand
    {
        /// <summary>
        /// Gets a <see cref="DelegateCommandBuilder&lt;TTarget&gt;"/>
        /// </summary>
        /// <typeparam name="TTarget">The type of the command target.</typeparam>
        /// <param name="target">The command target.</param>
        /// <returns>The command builder.</returns>
        public static DelegateCommandBuilder<TTarget> For<TTarget>(TTarget target)
            where TTarget : class
        {
            return new DelegateCommandBuilder<TTarget>(target);
        }

        /// <summary>
        /// Gets a <see cref="DelegateCommand&lt;TParameter&gt;"/>
        /// </summary>
        /// <typeparam name="TParameter">The type of the command parameter.</typeparam>
        /// <returns>The command builder.</returns>
        public static DelegateCommand<TParameter> WithParameter<TParameter>()
        {
            return new DelegateCommand<TParameter>();
        }
    }

    /// <summary>
    /// Builds an <see cref="IDelegateCommand"/> in a strongly typed fashion.
    /// </summary>
    public sealed class DelegateCommand<TParameter>
    {
        /// <summary>
        /// Gets a <see cref="DelegateCommandBuilder&lt;TTarget, TParameter&gt;"/>
        /// </summary>
        /// <typeparam name="TTarget">The type of the command target.</typeparam>
        /// <param name="target">The command target.</param>
        /// <returns>The command builder.</returns>
        public DelegateCommandBuilder<TTarget, TParameter> For<TTarget>(TTarget target)
            where TTarget : class
        {
            return new DelegateCommandBuilder<TTarget, TParameter>(target);
        }
    }
}
