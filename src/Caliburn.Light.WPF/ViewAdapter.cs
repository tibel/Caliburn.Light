using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Caliburn.Light.WPF
{
    /// <summary>
    /// Adapter for the view-model to interact with a XAML view.
    /// </summary>
    public sealed class ViewAdapter : IViewAdapter
    {
        /// <summary>
        /// The instance.
        /// </summary>
        public static readonly ViewAdapter Instance = new ViewAdapter();

        private ViewAdapter()
        {
        }

        /// <summary>
        /// Indicates whether or not the framework is running in the context of a designer.
        /// </summary>
        public bool IsInDesignTool => DesignMode.DesignModeEnabled;

        bool IViewAdapter.CanHandle(object view)
        {
            return view is DependencyObject;
        }

        /// <summary>
        /// Used to retrieve the root, non-framework-created view.
        /// </summary>
        /// <param name="view">The view to search.</param>
        /// <returns>
        /// The root element that was not created by the framework.
        /// </returns>
        /// <remarks>In certain instances the services create UI elements.
        /// For example, if you ask the window manager to show a UserControl as a dialog, it creates a window to host the UserControl in.
        /// The WindowManager marks that element as a framework-created element so that it can determine what it created vs. what was intended by the developer.
        /// Calling GetFirstNonGeneratedView allows the framework to discover what the original element was. 
        /// </remarks>
        public object GetFirstNonGeneratedView(object view)
        {
            if (!(view is DependencyObject dependencyObject))
                return view;

            if (!View.GetIsGenerated(dependencyObject))
                return view;

            if (view is ContentControl contentControl)
                return contentControl.Content;

            if (view is Page page)
                return page.Content;

            if (view is Popup popup)
                return popup.Child;

            throw new NotSupportedException("Generated view type is not supported.");
        }

        /// <summary>
        /// Executes the handler the fist time the view is loaded.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="handler">The handler.</param>
        public void ExecuteOnFirstLoad(object view, Action<object> handler)
        {
            if (view is FrameworkElement element)
                View.ExecuteOnFirstLoad(element, handler);
        }

        /// <summary>
        /// Executes the handler the next time the view's LayoutUpdated event fires.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="handler">The handler.</param>
        public void ExecuteOnLayoutUpdated(object view, Action<object> handler)
        {
            if (view is FrameworkElement element)
                View.ExecuteOnLayoutUpdated(element, handler);
        }

        /// <summary>
        /// Tries to close the specified view.
        /// </summary>
        /// <param name="view">The view to close.</param>
        /// <returns>true, when close could be initiated; otherwise false.</returns>
        public bool TryClose(object view)
        {
            if (view is Window window)
            {
                window.Close();
                return true;
            }

            if (view is Popup popup)
            {
                popup.IsOpen = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the command parameter of the view.
        /// This can be <see cref="P:ICommandSource.CommandParameter"/> or 'cal:Bind.CommandParameter'.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <returns>The command parameter.</returns>
        public object GetCommandParameter(object view)
        {
            if (!(view is DependencyObject element))
                return null;

            var commandParameter = Bind.GetCommandParameter(element);
            if (commandParameter is object)
                return commandParameter;

            if (element is System.Windows.Input.ICommandSource commandSource)
                return commandSource.CommandParameter;

            return null;
        }

        /// <summary>
        /// Gets the Dispatcher this view is associated with.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <returns>The dispatcher for the view.</returns>
        public IDispatcher GetDispatcher(object view)
        {
            if (!(view is DependencyObject element))
                return null;

            return new ViewDispatcher(element.Dispatcher);
        }
    }
}
