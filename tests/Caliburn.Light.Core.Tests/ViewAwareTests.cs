using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

public class ViewAwareTests
{
    [Test]
    public async Task AttachView_NullContext_StoresViewUnderDefaultContext()
    {
        var viewAware = new ViewAware();
        IViewAware va = viewAware;
        var view = new object();

        va.AttachView(view, null);

        await Assert.That(va.GetView(null)).IsEqualTo(view);
    }

    [Test]
    public async Task AttachView_SpecificContext_StoresViewUnderThatContext()
    {
        var viewAware = new ViewAware();
        IViewAware va = viewAware;
        var view = new object();

        va.AttachView(view, "context1");

        await Assert.That(va.GetView("context1")).IsEqualTo(view);
    }

    [Test]
    public async Task GetView_NoViewAttached_ReturnsNull()
    {
        var viewAware = new ViewAware();
        IViewAware va = viewAware;

        var result = va.GetView(null);

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task GetView_UnknownContext_ReturnsNull()
    {
        var viewAware = new ViewAware();
        IViewAware va = viewAware;
        va.AttachView(new object(), "ctx1");

        var result = va.GetView("ctx2");

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task DetachView_ExistingView_ReturnsTrue()
    {
        var viewAware = new ViewAware();
        IViewAware va = viewAware;
        var view = new object();
        va.AttachView(view, null);

        var result = va.DetachView(view, null);

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task DetachView_ExistingView_RemovesView()
    {
        var viewAware = new ViewAware();
        IViewAware va = viewAware;
        var view = new object();
        va.AttachView(view, null);

        va.DetachView(view, null);

        await Assert.That(va.GetView(null)).IsNull();
    }

    [Test]
    public async Task DetachView_NonExistingContext_ReturnsFalse()
    {
        var viewAware = new ViewAware();
        IViewAware va = viewAware;
        var view = new object();

        var result = va.DetachView(view, "nonexistent");

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task AttachView_SameContext_ReplacesExistingView()
    {
        var viewAware = new ViewAware();
        IViewAware va = viewAware;
        var view1 = new object();
        var view2 = new object();

        va.AttachView(view1, "ctx");
        va.AttachView(view2, "ctx");

        await Assert.That(va.GetView("ctx")).IsEqualTo(view2);
    }

    [Test]
    public async Task AttachView_MultipleContexts_StoresSeparately()
    {
        var viewAware = new ViewAware();
        IViewAware va = viewAware;
        var view1 = new object();
        var view2 = new object();

        va.AttachView(view1, "ctx1");
        va.AttachView(view2, "ctx2");

        await Assert.That(va.GetView("ctx1")).IsEqualTo(view1);
        await Assert.That(va.GetView("ctx2")).IsEqualTo(view2);
    }

    [Test]
    public async Task GetViews_ReturnsAllAttachedViews()
    {
        var viewAware = new ViewAware();
        IViewAware va = viewAware;
        var view1 = new object();
        var view2 = new object();
        va.AttachView(view1, "ctx1");
        va.AttachView(view2, "ctx2");

        var views = va.GetViews().ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        await Assert.That(views.Count).IsEqualTo(2);
        await Assert.That(views["ctx1"]).IsEqualTo(view1);
        await Assert.That(views["ctx2"]).IsEqualTo(view2);
    }

    [Test]
    public async Task GetViews_NoViewsAttached_ReturnsEmpty()
    {
        var viewAware = new ViewAware();
        IViewAware va = viewAware;

        var views = va.GetViews().ToList();

        await Assert.That(views.Count).IsEqualTo(0);
    }

    [Test]
    public async Task AttachView_NullView_ThrowsArgumentNullException()
    {
        var viewAware = new ViewAware();
        IViewAware va = viewAware;

        var action = () => va.AttachView(null!, null);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    [Test]
    public async Task DetachView_NullView_ThrowsArgumentNullException()
    {
        var viewAware = new ViewAware();
        IViewAware va = viewAware;

        var action = () => va.DetachView(null!, null);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }
}
