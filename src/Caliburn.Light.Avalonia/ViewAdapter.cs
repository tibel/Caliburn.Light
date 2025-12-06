using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Caliburn.Light.Avalonia;

internal sealed class ViewAdapter : IViewAdapter
{
    private ViewAdapter()
    {
    }

    [ModuleInitializer]
    public static void Initialize()
    {
        ViewHelper.Initialize(new ViewAdapter());
    }

    public bool IsInDesignTool => Design.IsDesignMode;

    public bool CanHandle(object view)
    {
        return view is AvaloniaObject;
    }

    public object GetFirstNonGeneratedView(object view)
    {
        var AvaloniaObject = (AvaloniaObject)view;
        if (!View.GetIsGenerated(AvaloniaObject))
            return view;

        if (view is ContentControl contentControl)
            return contentControl.Content!;

        if (view is Popup popup)
            return popup.Child!;

        throw new NotSupportedException("Generated view type is not supported.");
    }

    public void ExecuteOnFirstLoad(object view, Action<object> handler)
    {
        if (view is Control element)
            View.ExecuteOnFirstLoad(element, handler);
    }

    public void ExecuteOnLayoutUpdated(object view, Action<object> handler)
    {
        if (view is Control element)
            View.ExecuteOnLayoutUpdated(element, handler);
    }

    public Task<bool> TryCloseAsync(object view)
    {
        if (view is Window window)
        {
            window.Close();
            return TaskHelper.TrueTask;
        }

        if (view is Popup popup)
        {
            popup.IsOpen = false;
            return TaskHelper.TrueTask;
        }

        return TaskHelper.FalseTask;
    }

    public object? GetCommandParameter(object view)
    {
        var AvaloniaObject = (AvaloniaObject)view;
        var commandParameter = View.GetCommandParameter(AvaloniaObject);
        if (commandParameter is not null)
            return commandParameter;

        if (view is ICommandSource commandSource)
            return commandSource.CommandParameter;

        return null;
    }

    public IDispatcher GetDispatcher(object view)
    {
        return View.GetDispatcherFrom(Dispatcher.UIThread);
    }
}
