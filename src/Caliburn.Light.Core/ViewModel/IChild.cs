using System;

namespace Caliburn.Light;

/// <summary>
/// Denotes a node within a parent/child hierarchy.
/// </summary>
[Obsolete("IChild is obsolete. Inherit from Screen or ParentAware, or implement IParentAware directly.", true)]
public interface IChild
{
    /// <summary>
    /// Gets or sets the parent.
    /// </summary>
    object? Parent { get; set; }
}
