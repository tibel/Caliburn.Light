using System.Windows;

namespace Caliburn.Light
{
    /// <summary>
    /// Special value to resolve <see cref="CoroutineExecutionContext"/> parameter.
    /// </summary>
    public sealed class ExecutionContextResolver : Freezable, ISpecialValue
    {
        /// <summary>
        /// Identifies the <seealso cref="ExecutionContextResolver.SourceOverride"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceOverrideProperty = DependencyProperty.Register(
            "SourceOverride", typeof (object), typeof (ExecutionContextResolver), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the override for <see cref="CoroutineExecutionContext.Source"/>.
        /// </summary>
        public object SourceOverride
        {
            get { return GetValue(SourceOverrideProperty); }
            set { SetValue(SourceOverrideProperty, value); }
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <returns>The new instance.</returns>
        protected override Freezable CreateInstanceCore()
        {
            return new ExecutionContextResolver();
        }

        /// <summary>
        /// Resolves the value of this instance.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The resolved value.</returns>
        public object Resolve(CoroutineExecutionContext context)
        {
            var sourceOverride = SourceOverride;
            if (sourceOverride is object)
                context.Source = sourceOverride;

            return context;
        }
    }
}
