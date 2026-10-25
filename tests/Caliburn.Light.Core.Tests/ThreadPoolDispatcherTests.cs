using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

public class ThreadPoolDispatcherTests
{
    [Test]
    public async Task Instance_CalledMultipleTimes_ReturnsSameSingleton()
    {
        var instance1 = ThreadPoolDispatcher.Instance;
        var instance2 = ThreadPoolDispatcher.Instance;

        await Assert.That(instance1).IsSameReferenceAs(instance2);
    }

    [Test]
    public async Task BeginInvoke_RunsActionOnThreadPool()
    {
        var dispatcher = ThreadPoolDispatcher.Instance;
        var tcs = new TaskCompletionSource<(bool IsThreadPoolThread, bool HasAccess)>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        dispatcher.BeginInvoke(() =>
            tcs.SetResult((Thread.CurrentThread.IsThreadPoolThread, dispatcher.CheckAccess())));

        var result = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await Assert.That(result.IsThreadPoolThread).IsTrue();
        await Assert.That(result.HasAccess).IsTrue();
    }

    [Test]
    public async Task SwitchTo_FromDedicatedThread_ResumesOnThreadPool()
    {
        var dispatcher = ThreadPoolDispatcher.Instance;
        var resumedOnThreadPool = await Task.Factory.StartNew(
            async () =>
            {
                await dispatcher.SwitchTo();
                return dispatcher.CheckAccess();
            },
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default).Unwrap();

        await Assert.That(resumedOnThreadPool).IsTrue();
    }
}
