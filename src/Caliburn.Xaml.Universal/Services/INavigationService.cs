using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Navigation;
#if WINDOWS_PHONE_APP
using Windows.Phone.UI.Input;
#endif

namespace Caliburn.Light
{
    /// <summary>
    /// Implemented by services that provide (<see cref="Uri" /> based) navigation.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        ///   Raised after navigation.
        /// </summary>
        event NavigatedEventHandler Navigated;

        /// <summary>
        ///   Raised prior to navigation.
        /// </summary>
        event NavigatingCancelEventHandler Navigating;

        /// <summary>
        ///   Raised when navigation fails.
        /// </summary>
        event NavigationFailedEventHandler NavigationFailed;

        /// <summary>
        ///   Raised when navigation is stopped.
        /// </summary>
        event NavigationStoppedEventHandler NavigationStopped;

        /// <summary>
        /// Gets or sets the data type of the current content, or the content that should be navigated to.
        /// </summary>
        Type SourcePageType { get; set; }

        /// <summary>
        /// Gets the data type of the content that is currently displayed.
        /// </summary>
        Type CurrentSourcePageType { get; }

        /// <summary>
        ///   Indicates whether the navigator can navigate forward.
        /// </summary>
        bool CanGoForward { get; }

        /// <summary>
        ///   Indicates whether the navigator can navigate back.
        /// </summary>
        bool CanGoBack { get; }

        /// <summary>
        ///   Navigates to the specified content.
        /// </summary>
        /// <param name="sourcePageType"> The <see cref="System.Type" /> to navigate to. </param>
        /// <param name="parameter">The object parameter to pass to the target.</param>
        /// <returns> Whether or not navigation succeeded. </returns>
        bool Navigate(Type sourcePageType, object parameter = null);

        /// <summary>
        ///   Navigates forward.
        /// </summary>
        void GoForward();

        /// <summary>
        ///   Navigates back.
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

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Occurs when the user presses the hardware Back button.
        /// </summary>
        event EventHandler<BackPressedEventArgs> BackPressed;
#endif

        /// <summary>
        /// Stores the frame navigation state in local settings if it can.
        /// </summary>
        /// <returns>Whether the suspension was sucessful</returns>
        bool SuspendState();

        /// <summary>
        /// Tries to restore the frame navigation state from local settings.
        /// </summary>
        /// <returns>Whether the restoration of successful.</returns>
        bool ResumeState();
    }
}
