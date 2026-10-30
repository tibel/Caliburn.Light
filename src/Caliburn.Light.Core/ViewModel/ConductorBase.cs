using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Caliburn.Light;

/// <summary>
/// A base class for various implementations of <see cref="IConductor"/>.
/// </summary>
/// <typeparam name="T">The type that is being conducted.</typeparam>
public abstract class ConductorBase<T> : Screen, IConductor where T : class
{
    private static readonly DefaultCloseStrategy<T> _defaultCloseStrategy = new DefaultCloseStrategy<T>();
    private ICloseStrategy<T>? _closeStrategy;

    /// <summary>
    /// Gets or sets the close strategy.
    /// </summary>
    /// <value>The close strategy.</value>
    public ICloseStrategy<T> CloseStrategy
    {
        get { return _closeStrategy ?? _defaultCloseStrategy; }
        set { _closeStrategy = value; }
    }

    Task IConductor.ActivateItemAsync(object? item)
    {
        return ActivateItemAsync((T?)item);
    }

    Task IConductor.DeactivateItemAsync(object item, bool close)
    {
        return DeactivateItemAsync((T)item, close);
    }

    IEnumerable IConductor.GetChildren()
    {
        return GetChildren();
    }

    /// <summary>
    /// Gets the children.
    /// </summary>
    /// <returns>The collection of children.</returns>
    public abstract IReadOnlyList<T> GetChildren();

    /// <summary>
    /// Activates the specified item.
    /// </summary>
    /// <param name="item">The item to activate.</param>
    public abstract Task ActivateItemAsync(T? item);

    /// <summary>
    /// Deactivates the specified item.
    /// </summary>
    /// <param name="item">The item to deactivate.</param>
    /// <param name="close">Indicates whether or not to close the item after deactivating it.</param>
    public abstract Task DeactivateItemAsync(T item, bool close);
}
