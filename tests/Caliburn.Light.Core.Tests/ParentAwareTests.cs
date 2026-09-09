using System;
using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

public class ParentAwareTests
{
    [Test]
    public async Task Parent_Initially_IsNull()
    {
        IParentAware parentAware = new ParentAware();

        await Assert.That(parentAware.Parent).IsNull();
    }

    [Test]
    public async Task AttachParent_SetsParent()
    {
        IParentAware parentAware = new ParentAware();
        var parent = new object();

        parentAware.AttachParent(parent);

        await Assert.That(parentAware.Parent).IsSameReferenceAs(parent);
    }

    [Test]
    public async Task AttachParent_ReplacesExistingParent()
    {
        IParentAware parentAware = new ParentAware();
        var firstParent = new object();
        var secondParent = new object();

        parentAware.AttachParent(firstParent);
        parentAware.AttachParent(secondParent);

        await Assert.That(parentAware.Parent).IsSameReferenceAs(secondParent);
    }

    [Test]
    public async Task DetachParent_MatchingParent_ClearsParent()
    {
        IParentAware parentAware = new ParentAware();
        var parent = new object();
        parentAware.AttachParent(parent);

        var detached = parentAware.DetachParent(parent);

        await Assert.That(detached).IsTrue();
        await Assert.That(parentAware.Parent).IsNull();
    }

    [Test]
    public async Task DetachParent_DifferentParent_ReturnsFalseAndKeepsParent()
    {
        IParentAware parentAware = new ParentAware();
        var parent = new object();
        parentAware.AttachParent(parent);

        var detached = parentAware.DetachParent(new object());

        await Assert.That(detached).IsFalse();
        await Assert.That(parentAware.Parent).IsSameReferenceAs(parent);
    }

    [Test]
    public async Task AttachParent_NullParent_Throws()
    {
        IParentAware parentAware = new ParentAware();

        var action = () => parentAware.AttachParent(null!);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    [Test]
    public async Task DetachParent_NullParent_Throws()
    {
        IParentAware parentAware = new ParentAware();

        var action = () => parentAware.DetachParent(null!);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    [Test]
    public async Task ViewAware_ImplementsIParentAware()
    {
        IParentAware parentAware = new ViewAware();
        var parent = new object();

        parentAware.AttachParent(parent);

        await Assert.That(parentAware.Parent).IsSameReferenceAs(parent);
    }

    [Test]
    public async Task AttachParent_CallsOnParentAttached()
    {
        var parentAware = new TestParentAware();
        var parent = new object();

        ((IParentAware)parentAware).AttachParent(parent);

        await Assert.That(parentAware.AttachedParent).IsSameReferenceAs(parent);
    }

    [Test]
    public async Task DetachParent_CallsOnParentDetached()
    {
        var parentAware = new TestParentAware();
        var parent = new object();
        ((IParentAware)parentAware).AttachParent(parent);

        ((IParentAware)parentAware).DetachParent(parent);

        await Assert.That(parentAware.DetachedParent).IsSameReferenceAs(parent);
    }

    [Test]
    public async Task DetachParent_NonMatchingParent_DoesNotCallOnParentDetached()
    {
        var parentAware = new TestParentAware();
        var parent = new object();
        var otherParent = new object();
        ((IParentAware)parentAware).AttachParent(parent);

        ((IParentAware)parentAware).DetachParent(otherParent);

        await Assert.That(parentAware.DetachedParent).IsNull();
    }

    private sealed class TestParentAware : ParentAware
    {
        public object? AttachedParent { get; private set; }

        public object? DetachedParent { get; private set; }

        protected override void OnParentAttached(object parent)
        {
            AttachedParent = parent;
        }

        protected override void OnParentDetached(object parent)
        {
            DetachedParent = parent;
        }
    }
}
