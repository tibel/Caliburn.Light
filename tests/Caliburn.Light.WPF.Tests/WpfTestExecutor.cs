using System.Windows.Threading;
using TUnit.Core;
using TUnit.Core.Interfaces;

namespace Caliburn.Light.WPF.Tests;

/// <summary>
/// TUnit test executor that runs each test on a dedicated STA thread
/// with a WPF Dispatcher message loop. Equivalent to WpfTestHelper.RunOnStaThread.
/// Apply via [TestExecutor&lt;WpfTestExecutor&gt;] at assembly, class, or method level.
/// </summary>
public class WpfTestExecutor : ITestExecutor
{
    public ValueTask ExecuteTest(TestContext context, Func<ValueTask> action)
    {
        var tcs = new TaskCompletionSource();
        var thread = new Thread(() =>
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    await action();
                    tcs.SetResult();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
                finally
                {
                    dispatcher.InvokeShutdown();
                }
            });
            Dispatcher.Run();
        })
        {
            IsBackground = true,
            Name = "WPF Test Thread"
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        return new ValueTask(tcs.Task);
    }
}
