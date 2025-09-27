
namespace Caliburn.Light.WinUI
{
    /// <summary>
    /// Extension methods related to navigation.
    /// </summary>
    public static class NavigationServiceHelper
    {
        /// <summary>
        /// Navigates to the specified content.
        /// </summary>
        /// <param name="navigationService">The navigation service.</param>
        /// <param name="parameter">The object parameter to pass to the target.</param>
        /// <typeparam name="TView">The page type to navigate to.</typeparam>
        /// <returns>Whether or not navigation succeeded.</returns>
        public static bool Navigate<TView>(this INavigationService navigationService, object? parameter = null)
        {
            return navigationService.Navigate(typeof(TView), parameter);
        }

        /// <summary>
        /// Navigate to the specified model type.
        /// </summary>
        /// <param name="navigationService">The navigation service.</param>
        /// <param name="parameter">The object parameter to pass to the target.</param>
        /// <typeparam name="TViewModel">The model type to navigate to.</typeparam>
        /// <returns>Whether or not navigation succeeded.</returns>
        public static bool NavigateToViewModel<TViewModel>(this INavigationService navigationService, object? parameter = null)
        {
            return navigationService.NavigateToViewModel(typeof(TViewModel), parameter);
        }
    }
}
