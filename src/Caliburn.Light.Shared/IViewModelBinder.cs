#if NETFX_CORE
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace Caliburn.Light
{
    /// <summary>
    /// Binds a view to a view model.
    /// </summary>
    public interface IViewModelBinder
    {
        /// <summary>
        /// Binds the specified viewModel to the view.
        /// </summary>
        /// <param name="viewModel">The view model</param>
        /// <param name="view">The view.</param>
        /// <param name="context">The creation context (or null for default).</param>
        /// <param name="setDataContext">Whether to set the DataContext or leave unchanged.</param>
        void Bind(object viewModel, UIElement view, string context, bool setDataContext);
    }
}
