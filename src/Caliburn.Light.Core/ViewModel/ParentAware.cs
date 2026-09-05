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
        _parent = new WeakReference(parent);
    }

    bool IParentAware.DetachParent(object parent)
    {
        ArgumentNullException.ThrowIfNull(parent);

        if (!ReferenceEquals(_parent?.Target, parent))
            return false;

        _parent = null;
        return true;
    }

    /// <summary>
    /// Gets the parent of this instance.
    /// </summary>
    protected object? Parent => _parent?.Target;
}
