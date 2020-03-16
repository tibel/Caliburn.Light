﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Navigation;

namespace Caliburn.Light.WPF
{
    /// <summary>
    /// A service that manages windows.
    /// </summary>
    public class WindowManager : IWindowManager
    {
        private readonly IViewModelLocator _viewModelLocator;

        /// <summary>
        /// Creates an instance of <see cref="WindowManager"/>.
        /// </summary>
        /// <param name="viewModelLocator">The view-model locator.</param>
        public WindowManager(IViewModelLocator viewModelLocator)
        {
            if (viewModelLocator is null)
                throw new ArgumentNullException(nameof(viewModelLocator));

            _viewModelLocator = viewModelLocator;
        }

        /// <summary>
        /// Shows a modal dialog for the specified model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The context.</param>
        /// <param name="settings">The optional dialog settings.</param>
        /// <returns>The dialog result.</returns>
        public bool? ShowDialog(object viewModel, string context, IDictionary<string, object> settings)
        {
            if (viewModel is null)
                throw new ArgumentNullException(nameof(viewModel));

            var window = CreateWindow(viewModel, context, settings);

            var owner = InferOwnerOf(window);
            if (owner is object)
                window.Owner = owner;

            // defaults
            if (View.GetIsGenerated(window) && settings?.ContainsKey("WindowStartupLocation") != true)
                window.WindowStartupLocation = owner is object
                    ? WindowStartupLocation.CenterOwner
                    : WindowStartupLocation.CenterScreen;

            return window.ShowDialog();
        }

        /// <summary>
        /// Shows a window for the specified model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The context.</param>
        /// <param name="settings">The optional window settings.</param>
        public void ShowWindow(object viewModel, string context, IDictionary<string, object> settings)
        {
            if (viewModel is null)
                throw new ArgumentNullException(nameof(viewModel));

            if (Application.Current?.MainWindow is NavigationWindow navWindow)
                navWindow.Navigate(CreatePage(viewModel, context, settings));
            else
                CreateWindow(viewModel, context, settings).Show();
        }

        /// <summary>
        /// Shows a popup at the current mouse position.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The view context.</param>
        /// <param name="settings">The optional popup settings.</param>
        public void ShowPopup(object viewModel, string context, IDictionary<string, object> settings)
        {
            if (viewModel is null)
                throw new ArgumentNullException(nameof(viewModel));

            var popup = CreatePopup(viewModel, context, settings);

            // defaults
            if (settings?.ContainsKey("Placement") != true)
                popup.Placement = PlacementMode.MousePoint;
            if (settings?.ContainsKey("AllowsTransparency") != true)
                popup.AllowsTransparency = true;

            popup.IsOpen = true;
            popup.CaptureMouse();
        }

        /// <summary>
        /// Creates a popup.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The view context.</param>
        /// <param name="settings">The optional popup settings.</param>
        /// <returns>The popup.</returns>
        protected Popup CreatePopup(object viewModel, string context, IDictionary<string, object> settings)
        {
            var view = EnsurePopup(viewModel, _viewModelLocator.LocateForModel(viewModel, context));
            View.SetViewModelLocator(view, _viewModelLocator);

            if (context is object)
                View.SetContext(view, context);

            view.DataContext = viewModel;

            view.Closed += (s, _) => DeactivateAndDetach((FrameworkElement)s);

            if (viewModel is IViewAware viewAware)
                viewAware.AttachView(view, context);

            ApplySettings(view, settings);

            if (viewModel is IActivatable activatable)
                activatable.ActivateAsync().Observe();

            return view;
        }

        /// <summary>
        /// Ensures the view is a popup or provides one.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="view">The view.</param>
        /// <returns>The popup.</returns>
        protected virtual Popup EnsurePopup(object viewModel, UIElement view)
        {
            if (!(view is Popup popup))
            {
                popup = new Popup
                {
                    Child = view
                };

                View.SetIsGenerated(popup, true);
            }

            return popup;
        }

        /// <summary>
        /// Creates a window.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The view context.</param>
        /// <param name="settings">The optional window settings.</param>
        /// <returns>The window.</returns>
        protected Window CreateWindow(object viewModel, string context, IDictionary<string, object> settings)
        {
            var view = EnsureWindow(viewModel, _viewModelLocator.LocateForModel(viewModel, context));
            View.SetViewModelLocator(view, _viewModelLocator);

            if (context is object)
                View.SetContext(view, context);

            view.DataContext = viewModel;

            if (viewModel is IViewAware viewAware)
                viewAware.AttachView(view, context);

            if (viewModel is IHaveDisplayName haveDisplayName && !BindingOperations.IsDataBound(view, Window.TitleProperty))
            {
                var binding = new Binding("DisplayName") { Mode = BindingMode.OneWay };
                view.SetBinding(Window.TitleProperty, binding);
            }

            ApplySettings(view, settings);

            var conductor = new WindowConductor(viewModel, view);
            view.Closed += (s, _) => Detach((FrameworkElement)s);

            return conductor.View;
        }

        /// <summary>
        /// Makes sure the view is a window or is wrapped by one.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="view">The view.</param>
        /// <returns>The window.</returns>
        protected virtual Window EnsureWindow(object viewModel, UIElement view)
        {
            if (!(view is Window window))
            {
                window = new Window
                {
                    Content = view,
                    SizeToContent = SizeToContent.WidthAndHeight
                };

                View.SetIsGenerated(window, true);
            }

            return window;
        }

        /// <summary>
        /// Infers the owner of the window.
        /// </summary>
        /// <param name="window">The window to whose owner needs to be determined.</param>
        /// <returns>The owner.</returns>
        protected virtual Window InferOwnerOf(Window window)
        {
            var application = Application.Current;
            if (application is null)
                return null;

            var active = application.Windows.Cast<Window>().FirstOrDefault(x => x.IsActive);
            active = active ?? application.MainWindow;
            return ReferenceEquals(active, window) ? null : active;
        }

        /// <summary>
        /// Creates the page.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The context.</param>
        /// <param name="settings">The optional page settings.</param>
        /// <returns>The page.</returns>
        protected Page CreatePage(object viewModel, string context, IDictionary<string, object> settings)
        {
            var view = EnsurePage(viewModel, _viewModelLocator.LocateForModel(viewModel, context));
            View.SetViewModelLocator(view, _viewModelLocator);

            if (context is object)
                View.SetContext(view, context);

            view.DataContext = viewModel;

            view.Unloaded += (s, _) => DeactivateAndDetach((FrameworkElement)s);

            if (viewModel is IViewAware viewAware)
                viewAware.AttachView(view, context);

            if (viewModel is IHaveDisplayName haveDisplayName && !BindingOperations.IsDataBound(view, Page.TitleProperty))
            {
                var binding = new Binding("DisplayName") { Mode = BindingMode.OneWay };
                view.SetBinding(Page.TitleProperty, binding);
            }

            ApplySettings(view, settings);

            if (viewModel is IActivatable activatable)
                activatable.ActivateAsync().Observe();

            return view;
        }

        /// <summary>
        /// Ensures the view is a page or provides one.
        /// </summary>
        /// <param name="viewModel">The model.</param>
        /// <param name="view">The view.</param>
        /// <returns>The page.</returns>
        protected virtual Page EnsurePage(object viewModel, UIElement view)
        {
            if (!(view is Page page))
            {
                page = new Page
                {
                    Content = view
                };

                View.SetIsGenerated(page, true);
            }

            return page;
        }

        private static void ApplySettings(object target, IEnumerable<KeyValuePair<string, object>> settings)
        {
            if (settings is null) return;

            var type = target.GetType();
            foreach (var pair in settings)
            {
                var propertyInfo = type.GetRuntimeProperty(pair.Key);
                propertyInfo?.SetValue(target, pair.Value, null);
            }
        }

        private static void DeactivateAndDetach(FrameworkElement view)
        {
            if (view.DataContext is IActivatable activatable)
                activatable.DeactivateAsync(true).Observe();

            if (view.DataContext is IViewAware viewAware)
                viewAware.DetachView(view, View.GetContext(view));
        }

        private static void Detach(FrameworkElement view)
        {
            if (view.DataContext is IViewAware viewAware)
                viewAware.DetachView(view, View.GetContext(view));
        }
    }
}
