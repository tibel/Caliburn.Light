using System;
using System.Collections.Generic;

namespace Caliburn.Light;

/// <summary>
/// The context used during the execution of a command.
/// </summary>
public sealed class CommandExecutionContext
{
    private List<KeyValuePair<string, object?>>? _values;

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

            var entry = _values?.Find(p => string.Equals(p.Key, key, StringComparison.Ordinal));
            return entry?.Value;
        }
        set
        {
            ArgumentNullException.ThrowIfNull(key);

            _values ??= new List<KeyValuePair<string, object?>>();
            var index = _values.FindIndex(p => string.Equals(p.Key, key, StringComparison.Ordinal));
            if (index < 0)
                _values.Add(new KeyValuePair<string, object?>(key, value));
            else
                _values[index] = new KeyValuePair<string, object?>(key, value);
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
