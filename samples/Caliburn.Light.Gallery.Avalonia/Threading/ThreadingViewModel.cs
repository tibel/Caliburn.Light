using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Caliburn.Light.Gallery.Avalonia.Threading;

public sealed class ThreadingViewModel : ViewAware, IHaveDisplayName
{
    private IDispatcher _dispatcher = CurrentThreadDispatcher.Instance;

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

    protected override void OnViewAttached(object view, string context)
    {
        base.OnViewAttached(view, context);
        _dispatcher = ViewHelper.GetDispatcher(view);
    }

    protected override void OnViewDetached(object view, string context)
    {
        _dispatcher = CurrentThreadDispatcher.Instance;
        base.OnViewDetached(view, context);
    }

    private async Task OnSwitchTo()
    {
        await Task.Delay(10).ConfigureAwait(false);
        Trace.Assert(!_dispatcher.CheckAccess());

        await _dispatcher.SwitchTo();
        Trace.Assert(_dispatcher.CheckAccess());

        Trace.TraceInformation("On UI thread after SwitchTo().");
    }

    private async Task OnConfigureAwaitFalse()
    {
        await Task.Delay(10).ConfigureAwait(false);
        Trace.Assert(!_dispatcher.CheckAccess());

        Trace.TraceInformation("On ThreadPool thread after ConfigureAwait(false).");
    }

    private async Task OnConfigureAwaitTrue()
    {
        await Task.Delay(10).ConfigureAwait(true);

        Trace.Assert(_dispatcher.CheckAccess());
        Trace.TraceInformation("On UI thread after ConfigureAwait(true).");
    }
}
