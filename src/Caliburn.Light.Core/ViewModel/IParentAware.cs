namespace Caliburn.Light;

/// <summary>
/// Denotes an instance that is aware of its parent.
/// </summary>
public interface IParentAware
{
    /// <summary>
    /// Attaches a parent to this instance.
    /// </summary>
    /// <param name="parent">The parent.</param>
    void AttachParent(object parent);

    /// <summary>
    /// Detaches a parent from this instance.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <returns><see langword="true"/> if the parent was detached; otherwise, <see langword="false"/>.</returns>
    bool DetachParent(object parent);

    /// <summary>
    /// Gets the parent of this instance.
    /// </summary>
    object? Parent { get; }
}
