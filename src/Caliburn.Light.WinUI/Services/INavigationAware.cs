namespace Caliburn.Light.WinUI
{
    /// <summary>
    /// Denotes a class which is aware of navigation handling.
    /// </summary>
    public interface INavigationAware
    {
        /// <summary>
        /// Called when a page becomes the active page in a frame.
        /// </summary>
        /// <param name="parameter">An object passed to the target page for the navigation.</param>
        void OnNavigatedTo(object parameter);

        /// <summary>
        /// Called when a page is no longer the active page in a frame.
        /// </summary>
        void OnNavigatedFrom();
    }
}
