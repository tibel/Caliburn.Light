using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Caliburn.Light.WinUI;

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

    public bool IsInDesignTool => DesignMode.DesignModeEnabled;

    public bool CanHandle(object view)
    {
        return view is DependencyObject;
    }

    public object GetFirstNonGeneratedView(object view)
    {
        var dependencyObject = (DependencyObject)view;
        if (!View.GetIsGenerated(dependencyObject))
            return view;

        if (view is ContentControl contentControl)
            return contentControl.Content;

        if (view is UserControl userControl)
            return userControl.Content;

        if (view is Popup popup)
            return popup.Child;

        throw new NotSupportedException("Generated view type is not supported.");
    }

    public void ExecuteOnFirstLoad(object view, Action<object> handler)
    {
        if (view is FrameworkElement element)
            View.ExecuteOnFirstLoad(element, handler);
    }

    public void ExecuteOnLayoutUpdated(object view, Action<object> handler)
    {
        if (view is FrameworkElement element)
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
        var dependencyObject = (DependencyObject)view;
        var commandParameter = View.GetCommandParameter(dependencyObject);
        if (commandParameter is not null)
            return commandParameter;

        if (view is ButtonBase buttonBase)
            return buttonBase.CommandParameter;

        return null;
    }

    public IDispatcher GetDispatcher(object view)
    {
        var dependencyObject = (DependencyObject)view;
        return View.GetDispatcherFrom(dependencyObject.DispatcherQueue);
    }
}
