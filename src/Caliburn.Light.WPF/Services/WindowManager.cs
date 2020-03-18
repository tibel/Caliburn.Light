using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

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
        /// Shows a non-modal window for the specified model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The context.</param>
        public void ShowWindow(object viewModel, string context = null)
        {
            if (viewModel is null)
                throw new ArgumentNullException(nameof(viewModel));

            CreateWindow(viewModel, context).Show();
        }

        /// <summary>
        /// Shows a modal window for the specified model.
        /// </summary>
        /// <param name="ownerViewModel">The owner view model.</param>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The context.</param>
        public Task ShowDialog(object ownerViewModel, object viewModel, string context = null)
        {
            if (ownerViewModel is null)
                throw new ArgumentNullException(nameof(ownerViewModel));
            if (viewModel is null)
                throw new ArgumentNullException(nameof(viewModel));

            var owner = GetWindow(ownerViewModel);
            if (owner is null)
                throw new InvalidOperationException("Could not infer Owner window.");

            var window = CreateWindow(viewModel, context);
            window.Owner = owner;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            return window.ShowModal();
        }

        /// <summary>
        /// Attempts to bring the window to the foreground and activates it.
        /// </summary>
        /// <param name="viewModel">The view model of the window.</param>
        /// <returns>true if the window was successfully activated; otherwise, false.</returns>
        public bool Activate(object viewModel)
        {
            if (viewModel is null)
                throw new ArgumentNullException(nameof(viewModel));

            var window = GetWindow(viewModel);
            if (window is null)
                throw new InvalidOperationException("Could not infer window.");

            return window.Activate();
        }

        /// <summary>
        /// Shows a popup at the current mouse position.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The view context.</param>
        public void ShowPopup(object viewModel, string context)
        {
            if (viewModel is null)
                throw new ArgumentNullException(nameof(viewModel));

            var popup = CreatePopup(viewModel, context);
            popup.IsOpen = true;
            popup.CaptureMouse();
        }

        /// <summary>
        /// Creates a popup.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The view context.</param>
        /// <returns>The popup.</returns>
        protected Popup CreatePopup(object viewModel, string context)
        {
            var view = EnsurePopup(viewModel, _viewModelLocator.LocateForModel(viewModel, context));
            View.SetViewModelLocator(view, _viewModelLocator);

            view.DataContext = viewModel;

            return new PopupLifecycle(view, context).View;
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
                    Child = view,
                    Placement = PlacementMode.MousePoint,
                    AllowsTransparency = true
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
        /// <returns>The window.</returns>
        protected Window CreateWindow(object viewModel, string context)
        {
            var view = EnsureWindow(viewModel, _viewModelLocator.LocateForModel(viewModel, context));
            View.SetViewModelLocator(view, _viewModelLocator);

            view.DataContext = viewModel;

            if (viewModel is IHaveDisplayName && !BindingOperations.IsDataBound(view, Window.TitleProperty))
            {
                var binding = new Binding(nameof(IHaveDisplayName.DisplayName)) { Mode = BindingMode.OneWay };
                view.SetBinding(Window.TitleProperty, binding);
            }

            return new WindowLifecycle(view, context, false).View;
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
                    SizeToContent = SizeToContent.WidthAndHeight,
                };

                View.SetIsGenerated(window, true);
            }

            return window;
        }

        private static Window GetWindow(object viewModel)
        {
            object view = null;

            while(viewModel is object)
            {
                if (viewModel is IViewAware viewAware)
                    view = viewAware.GetView();

                if (view is object)
                    break;

                viewModel = viewModel is IChild child
                    ? child.Parent
                    : null;
            }

            return view is DependencyObject d
                ? Window.GetWindow(d)
                : null;
        }
    }
}
