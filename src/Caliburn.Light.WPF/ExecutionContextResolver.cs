﻿using System.Windows;

namespace Caliburn.Light.WPF
{
    /// <summary>
    /// Special value to resolve <see cref="CommandExecutionContext"/> parameter.
    /// </summary>
    public sealed class ExecutionContextResolver : Freezable, ISpecialValue
    {
        /// <summary>
        /// Identifies the <seealso cref="ExecutionContextResolver.SourceOverride"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceOverrideProperty = DependencyProperty.Register(
            "SourceOverride", typeof(object), typeof(ExecutionContextResolver), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the override for <see cref="CommandExecutionContext.Source"/>.
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
        public object Resolve(CommandExecutionContext context)
        {
            var sourceOverride = SourceOverride;
            if (sourceOverride is not null)
                context.Source = sourceOverride;

            return context;
        }
    }
}
