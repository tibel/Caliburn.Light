using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

#if !NET5_0
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal sealed class ModuleInitializerAttribute : Attribute { }
}
#endif

namespace Caliburn.Light.WinUI
{
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
            if (view is not DependencyObject dependencyObject)
                return view;

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

        public object GetCommandParameter(object view)
        {
            if (view is not DependencyObject element)
                return null;

            var commandParameter = View.GetCommandParameter(element);
            if (commandParameter is not null)
                return commandParameter;

            if (element is ButtonBase buttonBase)
                return buttonBase.CommandParameter;

            return null;
        }

        public IDispatcher GetDispatcher(object view)
        {
            if (view is not DependencyObject element)
                return null;

            return View.GetDispatcherFrom(element.Dispatcher);
        }
    }
}
