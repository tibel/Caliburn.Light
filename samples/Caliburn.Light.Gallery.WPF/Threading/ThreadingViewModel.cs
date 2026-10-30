using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Caliburn.Light.Gallery.WPF.Threading;

public sealed class ThreadingViewModel : ViewAware, IHaveDisplayName
{
    public string? DisplayName => "Threading";

    public ThreadingViewModel()
    {
        SwitchToCommand = DelegateCommandBuilder.NoParameter()
            .OnExecute(OnSwitchTo)
            .Build();

        ConfigureAwaitFalseCommand = DelegateCommandBuilder.NoParameter()
            .OnExecute(OnConfigureAwaitFalse)
            .Build();

        ConfigureAwaitTrueCommand = DelegateCommandBuilder.NoParameter()
            .OnExecute(OnConfigureAwaitTrue)
            .Build();
    }

    public ICommand SwitchToCommand { get; }
    public ICommand ConfigureAwaitFalseCommand { get; }
    public ICommand ConfigureAwaitTrueCommand { get; }

    private async Task OnSwitchTo()
    {
        await Task.Delay(10).ConfigureAwait(false);
        Trace.Assert(!IsOnUIThread());

        if (GetAttachedView() is object view)
            await ViewHelper.SwitchToViewAsync(view);

        Trace.Assert(IsOnUIThread());
        Trace.TraceInformation("On UI thread after SwitchTo().");
    }

    private async Task OnConfigureAwaitFalse()
    {
        await Task.Delay(10).ConfigureAwait(false);
        Trace.Assert(!IsOnUIThread());

        Trace.TraceInformation("On ThreadPool thread after ConfigureAwait(false).");
    }

    private async Task OnConfigureAwaitTrue()
    {
        await Task.Delay(10).ConfigureAwait(true);

        Trace.Assert(IsOnUIThread());
        Trace.TraceInformation("On UI thread after ConfigureAwait(true).");
    }

    private bool IsOnUIThread()
    {
        return GetAttachedView() is object view && ViewHelper.CheckAccess(view);
    }
}
