using System;
#if NETFX_CORE
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace Caliburn.Light
{
    /// <summary>
    /// Adapter for the view-model to interact with a XAML view.
    /// </summary>
    public sealed class ViewAdapter : IViewAdapter
    {
        /// <summary>
        /// Indicates whether or not the framework is running in the context of a designer.
        /// </summary>
        public bool IsInDesignTool
        {
            get { return ViewHelper.IsInDesignTool; }
        }

        /// <summary>
        /// Used to retrieve the root, non-framework-created view.
        /// </summary>
        /// <param name="view">The view to search.</param>
        /// <returns>
        /// The root element that was not created by the framework.
        /// </returns>
        public object GetFirstNonGeneratedView(object view)
        {
            var dependencyObject = view as DependencyObject;
            if (dependencyObject == null) return view;
            return ViewHelper.GetFirstNonGeneratedView(dependencyObject);
        }

        /// <summary>
        /// Executes the handler the fist time the view is loaded.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="handler">The handler.</param>
        public void ExecuteOnFirstLoad(object view, Action<object> handler)
        {
            var element = view as FrameworkElement;
            if (element != null)
                ViewHelper.ExecuteOnFirstLoad(element, handler);
        }

        /// <summary>
        /// Executes the handler the next time the view's LayoutUpdated event fires.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="handler">The handler.</param>
        public void ExecuteOnLayoutUpdated(object view, Action<object> handler)
        {
            var element = view as FrameworkElement;
            if (element != null)
                ViewHelper.ExecuteOnLayoutUpdated(element, handler);
        }

        /// <summary>
        /// Tries to close the specified view.
        /// </summary>
        /// <param name="view">The view to close.</param>
        /// <param name="dialogResult">The dialog result.</param>
        /// <returns>true, when close could be initiated; otherwise false.</returns>
        public bool TryClose(object view, bool? dialogResult)
        {
            return ViewHelper.TryClose(view, dialogResult);
        }

        /// <summary>
        /// Gets the command parameter of the view.
        /// This can be <see cref="P:ICommandSource.CommandParameter"/> or 'cal:Bind.CommandParameter' (UAP only).
        /// </summary>
        /// <param name="view">The view.</param>
        /// <returns>The command parameter.</returns>
        public object GetCommandParameter(object view)
        {
            var dependencyObject = view as DependencyObject;
            if (dependencyObject == null) return null;
            return ViewHelper.GetCommandParameter(dependencyObject);
        }
    }
}
