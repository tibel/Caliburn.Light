using Microsoft.UI.Xaml;

namespace Caliburn.Light.WinUI;

/// <summary>
/// Special value to resolve <see cref="CommandExecutionContext"/> parameter.
/// </summary>
public sealed class ExecutionContextResolver : DependencyObject, ISpecialValue
{
    /// <summary>
    /// Identifies the <seealso cref="ExecutionContextResolver.SourceOverride"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SourceOverrideProperty = DependencyProperty.Register(
        "SourceOverride", typeof(object), typeof(ExecutionContextResolver), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the override for <see cref="CommandExecutionContext.Source"/>.
    /// </summary>
    public object? SourceOverride
    {
        get { return GetValue(SourceOverrideProperty); }
        set { SetValue(SourceOverrideProperty, value); }
    }

    /// <summary>
    /// Resolves the value of this instance.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>The resolved value.</returns>
    public object? Resolve(CommandExecutionContext context)
    {
        var sourceOverride = SourceOverride;
        if (sourceOverride is not null)
            context.Source = sourceOverride;

        return context;
    }
}
