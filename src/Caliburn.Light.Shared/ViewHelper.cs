﻿using System;
#if NETFX_CORE
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
#else
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
#endif

namespace Caliburn.Light
{
    /// <summary>
    /// Some helper methods when dealing with UI elements.
    /// </summary>
    public static class ViewHelper
    {
        private static bool? _isInDesignTool;

        /// <summary>
        /// Gets a value that indicates whether the process is running in design mode.
        /// </summary>
        public static bool IsInDesignTool
        {
            get
            {
                if (!_isInDesignTool.HasValue)
                {
#if NETFX_CORE
                    _isInDesignTool = DesignMode.DesignModeEnabled;
#else
                    var descriptor = DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty,
                        typeof (FrameworkElement));
                    _isInDesignTool = (bool)descriptor.Metadata.DefaultValue;
#endif
                }

                return _isInDesignTool.Value;
            }
        }

        /// <summary>
        /// Used to retrieve the root, non-framework-created view.
        /// </summary>
        /// <param name="view">The view to search.</param>
        /// <returns>The root element that was not created by the framework.</returns>
        /// <remarks>In certain instances the services create UI elements.
        /// For example, if you ask the window manager to show a UserControl as a dialog, it creates a window to host the UserControl in.
        /// The WindowManager marks that element as a framework-created element so that it can determine what it created vs. what was intended by the developer.
        /// Calling GetFirstNonGeneratedView allows the framework to discover what the original element was. 
        /// </remarks>
        public static object GetFirstNonGeneratedView(DependencyObject view)
        {
            if (!View.GetIsGenerated(view)) return view;

            if (!(view is ContentControl contentControl))
                throw new NotSupportedException("Generated view type is not supported.");

            return contentControl.Content;
        }

        private static readonly DependencyProperty PreviouslyAttachedProperty =
            DependencyProperty.RegisterAttached("PreviouslyAttached",
                typeof(bool), typeof (ViewHelper), new PropertyMetadata(BooleanBoxes.FalseBox));

        /// <summary>
        /// Executes the handler the fist time the element is loaded.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="handler">The handler.</param>
        public static void ExecuteOnFirstLoad(FrameworkElement element, Action<FrameworkElement> handler)
        {
            if ((bool) element.GetValue(PreviouslyAttachedProperty)) return;
            element.SetValue(PreviouslyAttachedProperty, BooleanBoxes.TrueBox);
            ExecuteOnLoad(element, handler);
        }

        /// <summary>
        /// Executes the handler immediately if the element is loaded, otherwise wires it to the Loaded event.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="handler">The handler.</param>
        /// <returns>true if the handler was executed immediately; false otherwise</returns>
        public static bool ExecuteOnLoad(FrameworkElement element, Action<FrameworkElement> handler)
        {
            if (IsElementLoaded(element))
            {
                handler(element);
                return true;
            }

            RoutedEventHandler loaded = null;
            loaded = delegate
            {
                element.Loaded -= loaded;
                handler(element);
            };
            element.Loaded += loaded;
            return false;
        }

        /// <summary>
        /// Executes the handler when the element is unloaded.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="handler">The handler.</param>
        public static void ExecuteOnUnload(FrameworkElement element, Action<FrameworkElement> handler)
        {
            RoutedEventHandler unloaded = null;
            unloaded = delegate
            {
                element.Unloaded -= unloaded;
                handler(element);
            };
            element.Unloaded += unloaded;
        }

        /// <summary>
        /// Executes the handler the next time the elements's LayoutUpdated event fires.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="handler">The handler.</param>
        public static void ExecuteOnLayoutUpdated(FrameworkElement element, Action<FrameworkElement> handler)
        {
#if NETFX_CORE
            EventHandler<object> onLayoutUpdate = null;
#else
            EventHandler onLayoutUpdate = null;
#endif
            onLayoutUpdate = delegate
            {
                element.LayoutUpdated -= onLayoutUpdate;
                handler(element);
            };
            element.LayoutUpdated += onLayoutUpdate;
        }

        /// <summary>
        /// Determines whether the specified <paramref name="element"/> is loaded.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>true if the element is loaded; otherwise, false.
        /// </returns>
        public static bool IsElementLoaded(FrameworkElement element)
        {
#if NETFX_CORE
            var content = Window.Current.Content;
            var parent = element.Parent ?? VisualTreeHelper.GetParent(element);
            return parent is object || (content is object && element == content);
#else
            return element.IsLoaded;
#endif
        }

        /// <summary>
        /// Tries to close the specified view.
        /// </summary>
        /// <param name="view">The view to close.</param>
        /// <param name="dialogResult">The dialog result.</param>
        /// <returns>true, when close could be initiated; otherwise false.</returns>
        public static bool TryClose(object view, bool? dialogResult)
        {
            if (view is Window window)
            {
#if !NETFX_CORE
                if (dialogResult.HasValue)
                    window.DialogResult = dialogResult;
                else
                    window.Close();
#else
                window.Close();
#endif
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
        /// Gets the command parameter of the element.
        /// This can be <see cref="P:ICommandSource.CommandParameter"/> or 'cal:Bind.CommandParameter'.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The command parameter.</returns>
        public static object GetCommandParameter(DependencyObject element)
        {
            var commandParameter = Bind.GetCommandParameter(element);
            if (commandParameter is object)
                return commandParameter;

#if NETFX_CORE
            if (element is ButtonBase buttonBase)
                return buttonBase.CommandParameter;
#else
            if (element is System.Windows.Input.ICommandSource commandSource)
                return commandSource.CommandParameter;
#endif

            return null;
        }
    }
}
