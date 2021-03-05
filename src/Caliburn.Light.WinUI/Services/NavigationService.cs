using System;
using Microsoft.UI.Xaml.Controls;

namespace Caliburn.Light.WinUI
{
    /// <summary>
    /// A basic implementation of <see cref="INavigationService" /> designed to adapt the <see cref="Frame" /> control.
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly Frame _frame;
        private readonly IViewModelTypeResolver _viewModelTypeResolver;

        /// <summary>
        /// Creates an instance of <see cref="NavigationService" />.
        /// </summary>
        /// <param name="frame">The frame to represent as a <see cref="INavigationService" />.</param>
        /// <param name="viewModelTypeResolver">The view-model type resolver.</param>
        public NavigationService(Frame frame, IViewModelTypeResolver viewModelTypeResolver)
        {
            if (frame is null)
                throw new ArgumentNullException(nameof(frame));
            if (viewModelTypeResolver is null)
                throw new ArgumentNullException(nameof(viewModelTypeResolver));

            _frame = frame;
            _viewModelTypeResolver = viewModelTypeResolver;
        }

        /// <summary>
        /// Navigates to the specified view type.
        /// </summary>
        /// <param name="viewType"> The view type to navigate to.</param>
        /// <param name="parameter">The object parameter to pass to the target.</param>
        /// <returns> Whether or not navigation succeeded. </returns>
        public bool Navigate(Type viewType, object parameter = null)
        {
            if (parameter is null)
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
            if (viewType is null)
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
    }
}
