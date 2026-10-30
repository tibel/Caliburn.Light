using System;

namespace Caliburn.Light;

internal sealed class ViewDispatcher : IDispatcher
{
    private readonly IViewAdapter _viewAdapter;
    private readonly WeakReference _view;

    public ViewDispatcher(IViewAdapter viewAdapter, object view)
    {
        _viewAdapter = viewAdapter;
        _view = new WeakReference(view);
    }

    public bool CheckAccess()
    {
        return _view.Target is object view && _viewAdapter.CheckAccess(view);
    }

    public void BeginInvoke(Action action)
    {
        if (_view.Target is object view)
            _viewAdapter.BeginInvoke(view, action);
    }
}
