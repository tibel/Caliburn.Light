using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Caliburn.Light
{
    /// <summary>
    /// A basic implementation of <see cref="INavigationService" /> designed to adapt the <see cref="Frame" /> control.
    /// </summary>
    public class FrameAdapter : INavigationService
    {
        private readonly Frame _frame;
        private readonly IViewModelTypeResolver _viewModelTypeResolver;

        /// <summary>
        /// Creates an instance of <see cref="FrameAdapter" />.
        /// </summary>
        /// <param name="frame">The frame to represent as a <see cref="INavigationService" />.</param>
        /// <param name="viewModelTypeResolver">The view-model type resolver.</param>
        public FrameAdapter(Frame frame, IViewModelTypeResolver viewModelTypeResolver)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));
            if (viewModelTypeResolver == null)
                throw new ArgumentNullException(nameof(viewModelTypeResolver));

            _frame = frame;
            _viewModelTypeResolver = viewModelTypeResolver;
        }

        /// <summary>
        /// Raised after navigation.
        /// </summary>
        public event NavigatedEventHandler Navigated
        {
            add { _frame.Navigated += value; }
            remove { _frame.Navigated -= value; }
        }

        /// <summary>
        /// Raised prior to navigation.
        /// </summary>
        public event NavigatingCancelEventHandler Navigating
        {
            add { _frame.Navigating += value; }
            remove { _frame.Navigating -= value; }
        }

        /// <summary>
        /// Raised when navigation fails.
        /// </summary>
        public event NavigationFailedEventHandler NavigationFailed
        {
            add { _frame.NavigationFailed += value; }
            remove { _frame.NavigationFailed -= value; }
        }

        /// <summary>
        /// Raised when navigation is stopped.
        /// </summary>
        public event NavigationStoppedEventHandler NavigationStopped
        {
            add { _frame.NavigationStopped += value; }
            remove { _frame.NavigationStopped -= value; }
        }

        /// <summary>
        /// Gets the content that is currently displayed.
        /// </summary>
        public object Content
        {
            get { return _frame.Content; }
        }

        /// <summary>
        /// Navigates to the specified view type.
        /// </summary>
        /// <param name="viewType"> The view type to navigate to.</param>
        /// <param name="parameter">The object parameter to pass to the target.</param>
        /// <returns> Whether or not navigation succeeded. </returns>
        public bool Navigate(Type viewType, object parameter = null)
        {
            if (parameter == null)
                return _frame.Navigate(viewType);
            return _frame.Navigate(viewType, parameter);
        }

        /// <summary>
        /// Navigate to the specified model type.
        /// </summary>
        /// <param name="viewModelType">The model type to navigate to.</param>
        /// <param name="parameter">The object parameter to pass to the target.</param>
        /// <returns>Whether or not navigation succeeded.</returns>
        public bool NavigateToViewModel(Type viewModelType, object parameter = null)
        {
            var viewType = _viewModelTypeResolver.GetViewType(viewModelType, null);
            if (viewType == null)
            {
                throw new InvalidOperationException(string.Format("No view was found for {0}.", viewModelType.FullName));
            }

            return Navigate(viewType, parameter);
        }

        /// <summary>
        /// Navigates forward.
        /// </summary>
        public void GoForward()
        {
            _frame.GoForward();
        }

        /// <summary>
        /// Navigates back.
        /// </summary>
        public void GoBack()
        {
            _frame.GoBack();
        }

        /// <summary>
        /// Indicates whether the navigator can navigate forward.
        /// </summary>
        public bool CanGoForward
        {
            get { return _frame.CanGoForward; }
        }

        /// <summary>
        /// Indicates whether the navigator can navigate back.
        /// </summary>
        public bool CanGoBack
        {
            get { return _frame.CanGoBack; }
        }

        /// <summary>
        /// Gets a collection of PageStackEntry instances representing the backward navigation history of the Frame.
        /// </summary>
        public IList<PageStackEntry> BackStack
        {
            get { return _frame.BackStack; }
        }

        /// <summary>
        /// Gets a collection of PageStackEntry instances representing the forward navigation history of the Frame.
        /// </summary>
        public IList<PageStackEntry> ForwardStack
        {
            get { return _frame.ForwardStack; }
        }
    }
}
