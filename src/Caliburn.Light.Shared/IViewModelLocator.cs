﻿#if NETFX_CORE
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace Caliburn.Light
{
    /// <summary>
    /// Locates view and view-model instances.
    /// </summary>
    public interface IViewModelLocator
    {
        /// <summary>
        /// Locates the view for the specified model instance.
        /// </summary>
        /// <param name="model">the model instance.</param>
        /// <param name="context">The context (or null).</param>
        /// <returns>The view.</returns>
        UIElement LocateForModel(object model, string context);

        /// <summary>
        /// Locates the view model for the specified view instance.
        /// </summary>
        /// <param name="view">The view instance.</param>
        /// <returns>The view model.</returns>
        object LocateForView(UIElement view);
    }
}
