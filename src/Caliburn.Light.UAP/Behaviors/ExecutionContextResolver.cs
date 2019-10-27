﻿using Windows.UI.Xaml;

namespace Caliburn.Light
{
    /// <summary>
    /// Special value to resolve <see cref="CoroutineExecutionContext"/> parameter.
    /// </summary>
    public sealed class ExecutionContextResolver : DependencyObject, ISpecialValue
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
