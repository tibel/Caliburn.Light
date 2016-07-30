using System;

namespace Caliburn.Light
{
    /// <summary>
    /// Implemented by services that provide navigation.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Indicates whether the navigator can navigate forward.
        /// </summary>
        bool CanGoForward { get; }

        /// <summary>
        /// Indicates whether the navigator can navigate back.
        /// </summary>
        bool CanGoBack { get; }

        /// <summary>
        /// Navigates forward.
        /// </summary>
        void GoForward();

        /// <summary>
        /// Navigates back.
        /// </summary>
        void GoBack();

        /// <summary>
        /// Navigates to the specified view type.
        /// </summary>
        /// <param name="viewType"> The view type to navigate to.</param>
        /// <param name="parameter">The object parameter to pass to the target.</param>
        /// <returns> Whether or not navigation succeeded. </returns>
        bool Navigate(Type viewType, object parameter = null);

        /// <summary>
        /// Navigate to the specified model type.
        /// </summary>
        /// <param name="viewModelType">The model type to navigate to.</param>
        /// <param name="parameter">The object parameter to pass to the target.</param>
        /// <returns>Whether or not navigation succeeded.</returns>
        bool NavigateToViewModel(Type viewModelType, object parameter = null);
    }
}
