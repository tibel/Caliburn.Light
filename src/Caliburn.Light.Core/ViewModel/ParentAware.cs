using System;

namespace Caliburn.Light;

/// <summary>
/// A base implementation of <see cref="IParentAware"/> that is aware of its parent.
/// </summary>
public class ParentAware : BindableObject, IParentAware
{
    private WeakReference? _parent;

    object? IParentAware.Parent => _parent?.Target;

    void IParentAware.AttachParent(object parent)
    {
        ArgumentNullException.ThrowIfNull(parent);

        if (_parent is null)
            _parent = new WeakReference(parent);
        else
            _parent.Target = parent;

        OnParentAttached(parent);
    }

    bool IParentAware.DetachParent(object parent)
    {
        ArgumentNullException.ThrowIfNull(parent);

        var currentParent = _parent?.Target;
        if (currentParent is null || !ReferenceEquals(currentParent, parent))
            return false;

        _parent = null;
        OnParentDetached(parent);
        return true;
    }

    /// <summary>
    /// Called when a parent is attached.
    /// </summary>
    /// <param name="parent">The parent.</param>
    protected virtual void OnParentAttached(object parent)
    {
    }

    /// <summary>
    /// Called when a parent is detached.
    /// </summary>
    /// <param name="parent">The parent.</param>
    protected virtual void OnParentDetached(object parent)
    {
    }

    /// <summary>
    /// Gets the parent of this instance.
    /// </summary>
    protected object? Parent => _parent?.Target;
}
