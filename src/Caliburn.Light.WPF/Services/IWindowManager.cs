using System.Threading.Tasks;

namespace Caliburn.Light.WPF
{
    /// <summary>
    /// A service that manages windows.
    /// </summary>
    public interface IWindowManager
    {
        /// <summary>
        /// Shows a non-modal window for the specified model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The context.</param>
        void ShowWindow(object viewModel, string context = null);

        /// <summary>
        /// Shows a modal window for the specified model.
        /// </summary>
        /// <param name="ownerViewModel">The owner view model.</param>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The context.</param>
        Task ShowDialog(object ownerViewModel, object viewModel, string context = null);

        /// <summary>
        /// Attempts to bring the window to the foreground and activates it.
        /// </summary>
        /// <param name="viewModel">The view model of the window.</param>
        /// <returns>true if the window was successfully activated; otherwise, false.</returns>
        bool Activate(object viewModel);

        /// <summary>
        /// Shows a popup at the current mouse position.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The view context.</param>
        void ShowPopup(object viewModel, string context = null);
    }
}
