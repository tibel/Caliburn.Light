using System;
using System.Collections.Generic;

namespace Caliburn.Light;

/// <summary>
/// The context used during the execution of a command.
/// </summary>
public sealed class CommandExecutionContext
{
    private Dictionary<string, object?>? _values;

    /// <summary>
    /// Gets or sets additional data needed to invoke the command.
    /// </summary>
    /// <param name="key">The data key.</param>
    /// <returns>Custom data associated with the context.</returns>
    public object? this[string key]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(key);

            return _values?.TryGetValue(key, out var result) == true ? result : null;
        }
        set
        {
            ArgumentNullException.ThrowIfNull(key);

            _values ??= new Dictionary<string, object?>();
            _values[key] = value;
        }
    }

    /// <summary>
    /// The source from which the command originates.
    /// </summary>
    public object? Source { get; set; }

    /// <summary>
    /// The instance on which the command is invoked.
    /// </summary>
    public object? Target { get; set; }

    /// <summary>
    /// Any event arguments associated with the command invocation.
    /// </summary>
    public object? EventArgs { get; set; }
}
