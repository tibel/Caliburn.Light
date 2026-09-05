using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Caliburn.Light;

/// <summary>
/// A base class for various implementations of <see cref="IConductor"/> that maintain an active item.
/// </summary>
/// <typeparam name="T">The type that is being conducted.</typeparam>
public abstract class ConductorBaseWithActiveItem<T> : ConductorBase<T>, IHaveActiveItem where T : class
{
    private T? _activeItem;

    /// <summary>
    /// The currently active item.
    /// </summary>
    public T? ActiveItem
    {
        get { return _activeItem; }
        set { ActivateItemAsync(value).Observe(); }
    }

    object? IHaveActiveItem.ActiveItem => ActiveItem;

    /// <summary>
    /// Sets the active item without any activation or deactivation.
    /// </summary>
    /// <param name="newItem">The new item to set as active.</param>
    /// <returns>True if the active item was changed; otherwise, false.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected bool SetActiveItem(T? newItem) => SetProperty(ref _activeItem, newItem, nameof(ActiveItem));

    /// <summary>
    /// Changes the active item.
    /// </summary>
    /// <param name="newItem">The new item to activate.</param>
    /// <param name="closePrevious">Indicates whether or not to close the previous active item.</param>
    [Obsolete("Override active-item transitions in the concrete conductor instead.", true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected Task ChangeActiveItemAsync(T? newItem, bool closePrevious)
    {
        throw new NotSupportedException(
            "ChangeActiveItemAsync is obsolete. Override the conductor's activation behavior instead.");
    }
}
