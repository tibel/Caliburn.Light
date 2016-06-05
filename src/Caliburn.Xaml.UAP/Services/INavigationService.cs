using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Navigation;

namespace Caliburn.Light
{
    /// <summary>
    /// Implemented by services that provide navigation.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Raised prior to navigation.
        /// </summary>
        event NavigatingCancelEventHandler Navigating;

        /// <summary>
        /// Raised after navigation.
        /// </summary>
        event NavigatedEventHandler Navigated;

        /// <summary>
        /// Raised when navigation fails.
        /// </summary>
        event NavigationFailedEventHandler NavigationFailed;

        /// <summary>
        /// Raised when navigation is stopped.
        /// </summary>
        event NavigationStoppedEventHandler NavigationStopped;

        /// <summary>
        /// Gets the content that is currently displayed.
        /// </summary>
        object Content { get; }

        /// <summary>
        /// Indicates whether the navigator can navigate forward.
        /// </summary>
        bool CanGoForward { get; }

        /// <summary>
        /// Indicates whether the navigator can navigate back.
        /// </summary>
        bool CanGoBack { get; }

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

        /// <summary>
        /// Navigates forward.
        /// </summary>
        void GoForward();

        /// <summary>
        /// Navigates back.
        /// </summary>
        void GoBack();

        /// <summary>
        /// Gets a collection of PageStackEntry instances representing the backward navigation history of the Frame.
        /// </summary>
        IList<PageStackEntry> BackStack { get; }

        /// <summary>
        /// Gets a collection of PageStackEntry instances representing the forward navigation history of the Frame.
        /// </summary>
        IList<PageStackEntry> ForwardStack { get; }
    }
}
