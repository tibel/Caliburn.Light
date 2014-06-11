using System;
using System.Collections.Generic;
using System.Reflection;
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
                ViewHelper.ExecuteOnFirstLoad(element, (s, e) => handler(s));
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
                ViewHelper.ExecuteOnLayoutUpdated(element, (s, e) => handler(s));
        }

        /// <summary>
        /// Tries to close the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model to close.</param>
        /// <param name="views">The associated views.</param>
        /// <param name="dialogResult">The dialog result.</param>
        public void TryClose(object viewModel, ICollection<object> views, bool? dialogResult)
        {
            var child = viewModel as IChild;
            if (child != null)
            {
                var conductor = child.Parent as IConductor;
                if (conductor != null)
                {
                    conductor.CloseItem(viewModel);
                    return;
                }
            }

            foreach (var contextualView in views)
            {
                var viewType = contextualView.GetType();
                var closeMethod = viewType.GetRuntimeMethod("Close", new Type[0]);
                if (closeMethod != null)
                {
#if !SILVERLIGHT && !NETFX_CORE
                    var isClosed = false;
                    if (dialogResult != null)
                    {
                        var resultProperty = contextualView.GetType().GetProperty("DialogResult");
                        if (resultProperty != null)
                        {
                            resultProperty.SetValue(contextualView, dialogResult, null);
                            isClosed = true;
                        }
                    }

                    if (!isClosed)
                    {
                        closeMethod.Invoke(contextualView, null);
                    }
#else
                    closeMethod.Invoke(contextualView, null);
#endif
                    return;
                }

                var isOpenProperty = viewType.GetRuntimeProperty("IsOpen");
                if (isOpenProperty != null)
                {
                    isOpenProperty.SetValue(contextualView, false, null);
                    return;
                }
            }

            LogManager.GetLogger(typeof(Screen)).Info("TryClose requires a parent IConductor or a view with a Close method or IsOpen property.");
        }
    }
}
