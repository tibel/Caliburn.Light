using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

[NotInParallel("SharedStaticEventAndViewHelper")]
public class ViewHelperTests
{
    [Before(Test)]
    public void Setup()
    {
        ViewHelper.Reset();
    }

    [After(Test)]
    public void Cleanup()
    {
        ViewHelper.Reset();
    }

    [Test]
    public async Task IsInitialized_WhenNoAdapter_ReturnsFalse()
    {
        await Assert.That(ViewHelper.IsInitialized).IsFalse();
    }

    [Test]
    public async Task IsInitialized_AfterInitialize_ReturnsTrue()
    {
        ViewHelper.Initialize(new TestViewAdapter());

        await Assert.That(ViewHelper.IsInitialized).IsTrue();
    }

    [Test]
    public async Task Initialize_SameAdapterTwice_DoesNotDuplicate()
    {
        var adapter = new TestViewAdapter();

        ViewHelper.Initialize(adapter);
        ViewHelper.Initialize(adapter);

        await Assert.That(ViewHelper.IsInitialized).IsTrue();
    }

    [Test]
    public async Task Reset_ClearsAdapters()
    {
        ViewHelper.Initialize(new TestViewAdapter());

        ViewHelper.Reset();

        await Assert.That(ViewHelper.IsInitialized).IsFalse();
    }

    [Test]
    public async Task IsInDesignTool_WhenNoAdapter_ReturnsTrue()
    {
        await Assert.That(ViewHelper.IsInDesignTool).IsTrue();
    }

    [Test]
    public async Task IsInDesignTool_WhenAdapterReturnsFalse_ReturnsFalse()
    {
        ViewHelper.Initialize(new TestViewAdapter { InDesignTool = false });

        await Assert.That(ViewHelper.IsInDesignTool).IsFalse();
    }

    [Test]
    public async Task IsInDesignTool_WhenAdapterReturnsTrue_ReturnsTrue()
    {
        ViewHelper.Initialize(new TestViewAdapter { InDesignTool = true });

        await Assert.That(ViewHelper.IsInDesignTool).IsTrue();
    }

    [Test]
    public async Task IsView_WhenNoAdapter_ReturnsFalse()
    {
        await Assert.That(ViewHelper.IsView(new object())).IsFalse();
    }

    [Test]
    public async Task IsView_WhenAdapterCanHandle_ReturnsTrue()
    {
        ViewHelper.Initialize(new TestViewAdapter { CanHandleResult = true });

        await Assert.That(ViewHelper.IsView(new object())).IsTrue();
    }

    [Test]
    public async Task IsView_WhenNoAdapterCanHandle_ReturnsFalse()
    {
        ViewHelper.Initialize(new TestViewAdapter { CanHandleResult = false });

        await Assert.That(ViewHelper.IsView(new object())).IsFalse();
    }

    [Test]
    public async Task GetFirstNonGeneratedView_DelegatesToAdapter()
    {
        var expected = new object();
        var adapter = new TestViewAdapter { FirstNonGeneratedView = expected };
        ViewHelper.Initialize(adapter);

        var result = ViewHelper.GetFirstNonGeneratedView("view");

        await Assert.That(result).IsSameReferenceAs(expected);
    }

    [Test]
    public async Task ExecuteOnFirstLoad_DelegatesToAdapter()
    {
        var adapter = new TestViewAdapter();
        ViewHelper.Initialize(adapter);

        Action<object> handler = _ => { };
        ViewHelper.ExecuteOnFirstLoad("view", handler);

        await Assert.That(adapter.LastFirstLoadView).IsEqualTo("view");
        await Assert.That(adapter.LastFirstLoadHandler).IsSameReferenceAs(handler);
    }

    [Test]
    public async Task ExecuteOnLayoutUpdated_DelegatesToAdapter()
    {
        var adapter = new TestViewAdapter();
        ViewHelper.Initialize(adapter);

        Action<object> handler = _ => { };
        ViewHelper.ExecuteOnLayoutUpdated("view", handler);

        await Assert.That(adapter.LastLayoutUpdatedView).IsEqualTo("view");
        await Assert.That(adapter.LastLayoutUpdatedHandler).IsSameReferenceAs(handler);
    }

    [Test]
    public async Task TryCloseAsync_DelegatesToAdapter()
    {
        var adapter = new TestViewAdapter { TryCloseResult = true };
        ViewHelper.Initialize(adapter);

        var result = await ViewHelper.TryCloseAsync("view");

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task TryCloseAsync_WhenAdapterReturnsFalse_ReturnsFalse()
    {
        var adapter = new TestViewAdapter { TryCloseResult = false };
        ViewHelper.Initialize(adapter);

        var result = await ViewHelper.TryCloseAsync("view");

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GetCommandParameter_DelegatesToAdapter()
    {
        var param = new object();
        var adapter = new TestViewAdapter { CommandParameter = param };
        ViewHelper.Initialize(adapter);

        var result = ViewHelper.GetCommandParameter("view");

        await Assert.That(result).IsSameReferenceAs(param);
    }

    [Test]
    public async Task GetCommandParameter_WhenNull_ReturnsNull()
    {
        var adapter = new TestViewAdapter { CommandParameter = null };
        ViewHelper.Initialize(adapter);

        var result = ViewHelper.GetCommandParameter("view");

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task SwitchToViewAsync_UsesViewAdapter()
    {
        var adapter = new TestViewAdapter { CheckAccessResult = false };
        ViewHelper.Initialize(adapter);
        var view = new object();
        var resumedWithViewAccess = false;

        async Task Switch()
        {
            await ViewHelper.SwitchToViewAsync(view);
            resumedWithViewAccess = adapter.CheckAccess(view);
        }

        var switchTask = Switch();
        await Assert.That(switchTask.IsCompleted).IsFalse();
        await Assert.That(adapter.LastBeginInvokeView).IsSameReferenceAs(view);

        var continuation = adapter.LastBeginInvokeAction ?? throw new InvalidOperationException();
        adapter.CheckAccessResult = true;
        continuation();
        await switchTask;

        await Assert.That(resumedWithViewAccess).IsTrue();
    }

    [Test]
    public async Task SwitchToViewAsync_NoAdapter_ThrowsBeforeReturningAwaitable()
    {
        await Assert.That(() => ViewHelper.SwitchToViewAsync(new object()))
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task CheckAccess_DelegatesToAdapter()
    {
        var adapter = new TestViewAdapter { CheckAccessResult = false };
        ViewHelper.Initialize(adapter);

        var result = ViewHelper.CheckAccess("view");

        await Assert.That(result).IsFalse();
        await Assert.That(adapter.LastCheckAccessView).IsEqualTo("view");
    }

    [Test]
    public async Task BeginInvoke_DelegatesToAdapter()
    {
        var adapter = new TestViewAdapter();
        ViewHelper.Initialize(adapter);
        Action action = () => { };

        ViewHelper.BeginInvoke("view", action);

        await Assert.That(adapter.LastBeginInvokeView).IsEqualTo("view");
        await Assert.That(ReferenceEquals(adapter.LastBeginInvokeAction, action)).IsTrue();
    }

    [Test]
    public async Task GetFirstNonGeneratedView_NoAdapterMatches_ThrowsInvalidOperationException()
    {
        ViewHelper.Initialize(new TestViewAdapter { CanHandleResult = false });

        await Assert.That(() => ViewHelper.GetFirstNonGeneratedView("view"))
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task TryCloseAsync_NoAdapterMatches_ThrowsInvalidOperationException()
    {
        ViewHelper.Initialize(new TestViewAdapter { CanHandleResult = false });

        await Assert.That(() => { _ = ViewHelper.TryCloseAsync("view"); })
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task GetCommandParameter_NoAdapterMatches_ThrowsInvalidOperationException()
    {
        ViewHelper.Initialize(new TestViewAdapter { CanHandleResult = false });

        await Assert.That(() => ViewHelper.GetCommandParameter("view"))
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task CheckAccess_NoAdapterMatches_ThrowsInvalidOperationException()
    {
        ViewHelper.Initialize(new TestViewAdapter { CanHandleResult = false });

        await Assert.That(() => ViewHelper.CheckAccess("view"))
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task BeginInvoke_NoAdapterMatches_ThrowsInvalidOperationException()
    {
        ViewHelper.Initialize(new TestViewAdapter { CanHandleResult = false });

        await Assert.That(() => ViewHelper.BeginInvoke("view", () => { }))
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task MultipleAdapters_UsesCorrectOne()
    {
        var adapter1 = new TestViewAdapter { CanHandleResult = false };
        var expected = new object();
        var adapter2 = new TestViewAdapter { CanHandleResult = true, FirstNonGeneratedView = expected };
        ViewHelper.Initialize(adapter1);
        ViewHelper.Initialize(adapter2);

        var result = ViewHelper.GetFirstNonGeneratedView("view");

        await Assert.That(result).IsSameReferenceAs(expected);
    }

    private sealed class TestViewAdapter : IViewAdapter
    {
        public bool InDesignTool { get; set; }
        public bool CanHandleResult { get; set; } = true;
        public object FirstNonGeneratedView { get; set; } = new object();
        public bool TryCloseResult { get; set; } = true;
        public object? CommandParameter { get; set; }
        public bool CheckAccessResult { get; set; } = true;

        public object? LastFirstLoadView { get; private set; }
        public Action<object>? LastFirstLoadHandler { get; private set; }
        public object? LastLayoutUpdatedView { get; private set; }
        public Action<object>? LastLayoutUpdatedHandler { get; private set; }
        public object? LastCheckAccessView { get; private set; }
        public object? LastBeginInvokeView { get; private set; }
        public Action? LastBeginInvokeAction { get; private set; }

        public bool IsInDesignTool => InDesignTool;

        public bool CanHandle(object view) => CanHandleResult;

        public object GetFirstNonGeneratedView(object view) => FirstNonGeneratedView;

        public void ExecuteOnFirstLoad(object view, Action<object> handler)
        {
            LastFirstLoadView = view;
            LastFirstLoadHandler = handler;
        }

        public void ExecuteOnLayoutUpdated(object view, Action<object> handler)
        {
            LastLayoutUpdatedView = view;
            LastLayoutUpdatedHandler = handler;
        }

        public Task<bool> TryCloseAsync(object view) => Task.FromResult(TryCloseResult);

        public object? GetCommandParameter(object view) => CommandParameter;

        public bool CheckAccess(object view)
        {
            LastCheckAccessView = view;
            return CheckAccessResult;
        }

        public void BeginInvoke(object view, Action action)
        {
            LastBeginInvokeView = view;
            LastBeginInvokeAction = action;
        }
    }

}
