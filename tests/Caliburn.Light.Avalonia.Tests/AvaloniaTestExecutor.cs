using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using TUnit.Core;
using TUnit.Core.Interfaces;

namespace Caliburn.Light.Avalonia.Tests;

/// <summary>
/// TUnit test executor that dispatches test execution onto the Avalonia UI thread.
/// Boots a headless Avalonia Application once per process, then dispatches each test
/// onto the Avalonia Dispatcher. Apply via [TestExecutor&lt;AvaloniaTestExecutor&gt;] at
/// assembly, class, or method level.
/// </summary>
public class AvaloniaTestExecutor : ITestExecutor
{
    private static bool _initialized;
    private static readonly object _lock = new();

    public async ValueTask ExecuteTest(TestContext context, Func<ValueTask> action)
    {
        EnsureInitialized();

        if (Dispatcher.UIThread.CheckAccess())
        {
            await action();
            return;
        }

        var tcs = new TaskCompletionSource();

#pragma warning disable TUnit0031 // async void is intentional — tcs bridges completion
        Dispatcher.UIThread.Post(async () =>
#pragma warning restore TUnit0031
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
        });

        await tcs.Task;
    }

    private static void EnsureInitialized()
    {
        if (_initialized) return;

        lock (_lock)
        {
            if (_initialized) return;

            var ready = new ManualResetEventSlim(false);

            var thread = new Thread(() =>
            {
                AppBuilder.Configure<TestApp>()
                    .UseHeadless(new AvaloniaHeadlessPlatformOptions())
                    .AfterSetup(_ => ready.Set())
                    .StartWithClassicDesktopLifetime([], ShutdownMode.OnExplicitShutdown);
            })
            {
                IsBackground = true,
                Name = "Avalonia UI Thread"
            };
            thread.Start();

            if (!ready.Wait(TimeSpan.FromSeconds(15)))
                throw new TimeoutException("Avalonia runtime failed to initialize within 15 seconds.");

            _initialized = true;
        }
    }

    /// <summary>
    /// Minimal Avalonia application for headless testing.
    /// </summary>
    private class TestApp : Application { }
}
