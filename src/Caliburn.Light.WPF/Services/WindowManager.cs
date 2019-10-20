using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Navigation;

namespace Caliburn.Light
{
    /// <summary>
    /// A service that manages windows.
    /// </summary>
    public class WindowManager : IWindowManager
    {
        private readonly IViewModelLocator _viewModelLocator;
        private readonly IViewModelBinder _viewModelBinder;

        /// <summary>
        /// Creates an instance of <see cref="WindowManager"/>.
        /// </summary>
        /// <param name="viewModelLocator">The view-model locator.</param>
        /// <param name="viewModelBinder">The view-model binder.</param>
        public WindowManager(IViewModelLocator viewModelLocator, IViewModelBinder viewModelBinder)
        {
            if (viewModelLocator == null)
                throw new ArgumentNullException(nameof(viewModelLocator));
            if (viewModelBinder == null)
                throw new ArgumentNullException(nameof(viewModelBinder));

            _viewModelLocator = viewModelLocator;
            _viewModelBinder = viewModelBinder;
        }

        /// <summary>
        /// Shows a modal dialog for the specified model.
        /// </summary>
        /// <param name="rootModel">The root model.</param>
        /// <param name="context">The context.</param>
        /// <param name="settings">The dialog popup settings.</param>
        /// <returns>The dialog result.</returns>
        public virtual bool? ShowDialog(object rootModel, string context = null,
            IDictionary<string, object> settings = null)
        {
            return CreateWindow(rootModel, true, context, settings).ShowDialog();
        }

        /// <summary>
        /// Shows a window for the specified model.
        /// </summary>
        /// <param name="rootModel">The root model.</param>
        /// <param name="context">The context.</param>
        /// <param name="settings">The optional window settings.</param>
        public virtual void ShowWindow(object rootModel, string context = null,
            IDictionary<string, object> settings = null)
        {
            NavigationWindow navWindow = null;
            var application = Application.Current;
            if (application != null && application.MainWindow != null)
            {
                navWindow = application.MainWindow as NavigationWindow;
            }

            if (navWindow != null)
            {
                var page = CreatePage(rootModel, context, settings);
                navWindow.Navigate(page);
            }
            else
            {
                CreateWindow(rootModel, false, context, settings).Show();
            }
        }

        /// <summary>
        /// Shows a popup at the current mouse position.
        /// </summary>
        /// <param name="rootModel">The root model.</param>
        /// <param name="context">The view context.</param>
        /// <param name="settings">The optional popup settings.</param>
        public virtual void ShowPopup(object rootModel, string context = null,
            IDictionary<string, object> settings = null)
        {
            var popup = CreatePopup(rootModel, settings);
            var view = _viewModelLocator.LocateForModel(rootModel, context);

            popup.Child = view;
            ViewHelper.SetIsGenerated(popup, true);

            _viewModelBinder.Bind(rootModel, popup, context);

            var activatable = rootModel as IActivate;
            if (activatable != null)
            {
                activatable.Activate();
            }

            var deactivator = rootModel as IDeactivate;
            if (deactivator != null)
            {
                popup.Closed += (s, e) => deactivator.Deactivate(true);
            }

            popup.IsOpen = true;
            popup.CaptureMouse();
        }

        /// <summary>
        /// Creates a popup for hosting a popup window.
        /// </summary>
        /// <param name="rootModel">The model.</param>
        /// <param name="settings">The optional popup settings.</param>
        /// <returns>The popup.</returns>
        protected virtual Popup CreatePopup(object rootModel, IDictionary<string, object> settings)
        {
            settings = settings ?? new Dictionary<string, object>();
            if (!settings.ContainsKey("Placement"))
                settings.Add("Placement", PlacementMode.MousePoint);
            if (!settings.ContainsKey("AllowsTransparency"))
                settings.Add("AllowsTransparency", true);

            var popup = new Popup();
            ApplySettings(popup, settings);
            return popup;
        }

        /// <summary>
        /// Creates a window.
        /// </summary>
        /// <param name="rootModel">The view model.</param>
        /// <param name="isDialog">Whether or not the window is being shown as a dialog.</param>
        /// <param name="context">The view context.</param>
        /// <param name="settings">The optional popup settings.</param>
        /// <returns>The window.</returns>
        protected virtual Window CreateWindow(object rootModel, bool isDialog, string context,
            IDictionary<string, object> settings)
        {
            var view = EnsureWindow(rootModel, _viewModelLocator.LocateForModel(rootModel, context), isDialog);
            _viewModelBinder.Bind(rootModel, view, context);

            var haveDisplayName = rootModel as IHaveDisplayName;
            if (haveDisplayName != null && !BindingHelper.IsDataBound(view, Window.TitleProperty))
            {
                var binding = new Binding("DisplayName") {Mode = BindingMode.OneWay};
                view.SetBinding(Window.TitleProperty, binding);
            }

            ApplySettings(view, settings);

            var conductor = new WindowConductor(rootModel, view);
            return conductor.View;
        }

        /// <summary>
        /// Makes sure the view is a window or is wrapped by one.
        /// </summary>
        /// <param name="model">The view model.</param>
        /// <param name="view">The view.</param>
        /// <param name="isDialog">Whether or not the window is being shown as a dialog.</param>
        /// <returns>The window.</returns>
        protected virtual Window EnsureWindow(object model, object view, bool isDialog)
        {
            var window = view as Window;

            if (window == null)
            {
                window = new Window
                {
                    Content = view,
                    SizeToContent = SizeToContent.WidthAndHeight
                };

                ViewHelper.SetIsGenerated(window, true);

                var owner = InferOwnerOf(window);
                if (owner != null)
                {
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    window.Owner = owner;
                }
                else
                {
                    window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
            }
            else
            {
                var owner = InferOwnerOf(window);
                if (owner != null && isDialog)
                {
                    window.Owner = owner;
                }
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
            if (application == null)
                return null;

            var active = application.Windows.Cast<Window>().FirstOrDefault(x => x.IsActive);
            active = active ?? application.MainWindow;
            return ReferenceEquals(active, window) ? null : active;
        }

        /// <summary>
        /// Creates the page.
        /// </summary>
        /// <param name="rootModel">The root model.</param>
        /// <param name="context">The context.</param>
        /// <param name="settings">The optional popup settings.</param>
        /// <returns>The page.</returns>
        protected virtual Page CreatePage(object rootModel, string context, IDictionary<string, object> settings)
        {
            var view = EnsurePage(rootModel, _viewModelLocator.LocateForModel(rootModel, context));
            _viewModelBinder.Bind(rootModel, view, context);

            var haveDisplayName = rootModel as IHaveDisplayName;
            if (haveDisplayName != null && !BindingHelper.IsDataBound(view, Page.TitleProperty))
            {
                var binding = new Binding("DisplayName") {Mode = BindingMode.OneWay};
                view.SetBinding(Page.TitleProperty, binding);
            }

            ApplySettings(view, settings);

            var activatable = rootModel as IActivate;
            if (activatable != null)
            {
                activatable.Activate();
            }

            var deactivatable = rootModel as IDeactivate;
            if (deactivatable != null)
            {
                view.Unloaded += (s, e) => deactivatable.Deactivate(true);
            }

            return view;
        }

        /// <summary>
        /// Ensures the view is a page or provides one.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="view">The view.</param>
        /// <returns>The page.</returns>
        protected virtual Page EnsurePage(object model, object view)
        {
            var page = view as Page;

            if (page == null)
            {
                page = new Page {Content = view};
                ViewHelper.SetIsGenerated(page, true);
            }

            return page;
        }

        private static void ApplySettings(object target, IEnumerable<KeyValuePair<string, object>> settings)
        {
            if (settings == null) return;

            var type = target.GetType();
            foreach (var pair in settings)
            {
                var propertyInfo = type.GetRuntimeProperty(pair.Key);
                if (propertyInfo != null)
                    propertyInfo.SetValue(target, pair.Value, null);
            }
        }
    }
}
