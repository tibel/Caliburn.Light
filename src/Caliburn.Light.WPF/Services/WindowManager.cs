using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
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
            ArgumentNullException.ThrowIfNull(viewModelLocator);

            _viewModelLocator = viewModelLocator;
        }

        /// <summary>
        /// Shows a non-modal window for the specified model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The context.</param>
        public void ShowWindow(object viewModel, string? context)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            CreateWindow(viewModel, context).Show();
        }

        /// <summary>
        /// Shows a modal window for the specified model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The context.</param>
        /// <param name="ownerViewModel">The owner view model.</param>
        public Task ShowDialog(object viewModel, string context, object ownerViewModel)
        {
            ArgumentNullException.ThrowIfNull(ownerViewModel);
            ArgumentNullException.ThrowIfNull(viewModel);

            var owner = GetWindow(ownerViewModel);
            var window = CreateWindow(viewModel, context);

            if (owner is null)
            {
                var tcs = new TaskCompletionSource<bool?>();

                window.Closed += (s, e) => tcs.TrySetResult(null);
                window.Show();

                return tcs.Task;
            }
            else
            {
                window.Owner = owner;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                return window.ShowModal();
            }
        }

        /// <summary>
        /// Attempts to bring the window to the foreground and activates it.
        /// </summary>
        /// <param name="viewModel">The view model of the window.</param>
        /// <returns>true if the window was successfully activated; otherwise, false.</returns>
        public bool Activate(object viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            var window = GetWindow(viewModel);
            return window?.Activate() ?? false;
        }

        /// <summary>
        /// Shows a popup at the current mouse position.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The view context.</param>
        public void ShowPopup(object viewModel, string? context)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            var popup = CreatePopup(viewModel, context);
            popup.IsOpen = true;
            popup.CaptureMouse();
        }

        /// <summary>
        /// Shows a message box.
        /// </summary>
        /// <param name="settings">The message box settings.</param>
        /// <param name="ownerViewModel">The owner view model.</param>
        /// <returns>The message box result.</returns>
        public Task<MessageBoxResult> ShowMessageBox(MessageBoxSettings settings, object ownerViewModel)
        {
            ArgumentNullException.ThrowIfNull(ownerViewModel);
            ArgumentNullException.ThrowIfNull(settings);

            var owner = GetWindow(ownerViewModel);

            var result = owner is null
                ? MessageBox.Show(settings.Text, settings.Caption, settings.Button, settings.Image)
                : MessageBox.Show(owner, settings.Text, settings.Caption, settings.Button, settings.Image);

            return Task.FromResult(result);
        }

        /// <summary>
        /// Shows a file open dialog.
        /// </summary>
        /// <param name="settings">The open file dialog settings.</param>
        /// <param name="ownerViewModel">The owner view model.</param>
        /// <returns>A list of selected files.</returns>
        public Task<IReadOnlyList<string>> ShowOpenFileDialog(OpenFileDialogSettings settings, object ownerViewModel)
        {
            ArgumentNullException.ThrowIfNull(ownerViewModel);
            ArgumentNullException.ThrowIfNull(settings);

            var owner = GetWindow(ownerViewModel);

            var openFileDialog = new OpenFileDialog
            {
                RestoreDirectory = true,
                CheckFileExists = true,
                CheckPathExists = true,

                Multiselect = settings.Multiselect,
                Filter = settings.FileTypeFilter,
                Title = settings.Title,
                InitialDirectory = settings.InitialDirectory
            };

            bool? result;
            try
            {
                result = owner is null
                    ? openFileDialog.ShowDialog()
                    : openFileDialog.ShowDialog(owner);
            }
            catch when (!string.IsNullOrEmpty(openFileDialog.InitialDirectory))
            {
                openFileDialog.InitialDirectory = null;

                result = owner is null
                    ? openFileDialog.ShowDialog()
                    : openFileDialog.ShowDialog(owner);
            }

            var selectedFiles = result.GetValueOrDefault() ? openFileDialog.FileNames : Array.Empty<string>();
            return Task.FromResult<IReadOnlyList<string>>(selectedFiles);
        }

        /// <summary>
        /// Shows a file save dialog.
        /// </summary>
        /// <param name="settings">The save file dialog settings.</param>
        /// <param name="ownerViewModel">The owner view model.</param>
        /// <returns>The selected file.</returns>
        public Task<string> ShowSaveFileDialog(SaveFileDialogSettings settings, object ownerViewModel)
        {
            ArgumentNullException.ThrowIfNull(ownerViewModel);
            ArgumentNullException.ThrowIfNull(settings);

            var owner = GetWindow(ownerViewModel);

            var saveFileDialog = new SaveFileDialog
            {
                RestoreDirectory = true,
                AddExtension = true,
                CheckPathExists = true,

                Filter = settings.FileTypeFilter,
                DefaultExt = settings.DefaultFileExtension,

                Title = settings.Title,
                FileName = settings.InitialFileName,
                CreatePrompt = settings.PromptForCreate,
                OverwritePrompt = settings.PromptForOverwrite,
                InitialDirectory = settings.InitialDirectory
            };

            bool? result;
            try
            {
                result = owner is null
                    ? saveFileDialog.ShowDialog()
                    : saveFileDialog.ShowDialog(owner);
            }
            catch when (!string.IsNullOrEmpty(saveFileDialog.InitialDirectory))
            {
                saveFileDialog.InitialDirectory = null;

                result = owner is null
                    ? saveFileDialog.ShowDialog()
                    : saveFileDialog.ShowDialog(owner);
            }

            var selectedFile = result.GetValueOrDefault() ? saveFileDialog.FileName : string.Empty;
            return Task.FromResult(selectedFile);
        }

        /// <summary>
        /// Creates a popup.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The view context.</param>
        /// <returns>The popup.</returns>
        protected Popup CreatePopup(object viewModel, string? context)
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
            if (view is not Popup popup)
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
        protected Window CreateWindow(object viewModel, string? context)
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
            if (view is not Window window)
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

        /// <summary>
        /// Gets the window from the given view model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <returns>The window or null.</returns>
        protected static Window? GetWindow(object? viewModel)
        {
            object? view = null;

            while (viewModel is not null)
            {
                if (viewModel is IViewAware viewAware)
                    view = viewAware.GetViews().FirstOrDefault().Value;

                if (view is not null)
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
