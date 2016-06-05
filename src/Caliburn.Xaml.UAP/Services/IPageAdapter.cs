using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Caliburn.Light
{
    /// <summary>
    /// Integrate framework into <see cref="Page"/> navigation events.
    /// </summary>
    public interface IPageAdapter
    {
        /// <summary>
        /// Invoked immediately before the Page is unloaded and is no longer the current source of a parent Frame.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="e">The event data.</param>
        void OnNavigatingFrom(Page page, NavigatingCancelEventArgs e);

        /// <summary>
        /// Invoked immediately after the Page is unloaded and is no longer the current source of a parent Frame.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="e">The event data.</param>
        void OnNavigatedFrom(Page page, NavigationEventArgs e);

        /// <summary>
        /// Invoked when the Page is loaded and becomes the current source of a parent Frame.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="e">The event data.</param>
        void OnNavigatedTo(Page page, NavigationEventArgs e);
    }
}
