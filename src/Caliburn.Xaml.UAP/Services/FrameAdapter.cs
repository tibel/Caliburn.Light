using System;
using System.Collections.Generic;
using System.Reflection;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Caliburn.Light
{
    /// <summary>
    /// A basic implementation of <see cref="INavigationService" /> designed to adapt the <see cref="Frame" /> control.
    /// </summary>
    public class FrameAdapter : INavigationService
    {
        private static readonly ILogger Log = LogManager.GetLogger(typeof (FrameAdapter));

        private static DependencyProperty PageKeyProperty =
            DependencyProperty.RegisterAttached("_PageKey", typeof(string), typeof(FrameAdapter), null);

        private readonly Frame _frame;
        private readonly IViewModelLocator _viewModelLocator;
        private readonly IViewModelBinder _viewModelBinder;
        private readonly IViewModelTypeResolver _viewModelTypeResolver;
        private readonly ISuspensionManager _suspensionManager;

        /// <summary>
        /// Creates an instance of <see cref="FrameAdapter" />.
        /// </summary>
        /// <param name="frame">The frame to represent as a <see cref="INavigationService" />.</param>
        /// <param name="viewModelLocator">The view-model locator.</param>
        /// <param name="viewModelBinder">The view-model binder.</param>
        /// <param name="viewModelTypeResolver">The view-model type resolver.</param>
        /// <param name="suspensionManager">The suspension manager.</param>
        public FrameAdapter(Frame frame, IViewModelLocator viewModelLocator, IViewModelBinder viewModelBinder, 
            IViewModelTypeResolver viewModelTypeResolver, ISuspensionManager suspensionManager)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));
            if (viewModelLocator == null)
                throw new ArgumentNullException(nameof(viewModelLocator));
            if (viewModelBinder == null)
                throw new ArgumentNullException(nameof(viewModelBinder));
            if (viewModelTypeResolver == null)
                throw new ArgumentNullException(nameof(viewModelTypeResolver));
            if (suspensionManager == null)
                throw new ArgumentNullException(nameof(suspensionManager));

            _frame = frame;
            _viewModelLocator = viewModelLocator;
            _viewModelBinder = viewModelBinder;
            _viewModelTypeResolver = viewModelTypeResolver;
            _suspensionManager = suspensionManager;

            _frame.Navigating += OnNavigating;
            _frame.Navigated += OnNavigated;
        }

        /// <summary>
        /// Occurs before navigation
        /// </summary>
        /// <param name="sender"> The event sender. </param>
        /// <param name="e"> The event args. </param>
        protected virtual void OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            Navigating?.Invoke(sender, e);
            if (e.Cancel) return;

            var view = _frame.Content as FrameworkElement;
            if (view == null) return;

            var guard = view.DataContext as ICloseGuard;
            if (guard != null)
            {
                var task = guard.CanCloseAsync();
                if (!task.IsCompleted)
                    throw new NotSupportedException("Async task is not supported.");

                if (!task.Result)
                {
                    e.Cancel = true;
                    return;
                }
            }

            var navigationAware = view.DataContext as INavigationAware;
            if (navigationAware != null)
            {
                navigationAware.OnNavigatedFrom();
            }

            SaveState(view);

            var deactivator = view.DataContext as IDeactivate;
            if (deactivator != null)
            {
                deactivator.Deactivate(CanCloseOnNavigating(e));
            }
        }

        /// <summary>
        /// Occurs after navigation
        /// </summary>
        /// <param name="sender"> The event sender. </param>
        /// <param name="e"> The event args. </param>
        protected virtual void OnNavigated(object sender, NavigationEventArgs e)
        {
            var view = e.Content as FrameworkElement;
            if (view == null) return;

            EnsureViewModel(view);
            RestoreState(view, e.NavigationMode);

            var navigationAware = view.DataContext as INavigationAware;
            if (navigationAware != null)
            {
                navigationAware.OnNavigatedTo(e.Parameter);
            }

            var activator = view.DataContext as IActivate;
            if (activator != null)
            {
                activator.Activate();
            }
        }

        /// <summary>
        /// Ensures that the DataContext is set.
        /// </summary>
        /// <param name="view">The view.</param>
        protected void EnsureViewModel(FrameworkElement view)
        {
            if (view.DataContext == null)
            {
                var viewModel = _viewModelLocator.LocateForView(view);
                _viewModelBinder.Bind(viewModel, view, null);
            }
        }

        /// <summary>
        /// Saves the current state.
        /// </summary>
        /// <param name="view">The view.</param>
        protected void SaveState(FrameworkElement view)
        {
            var preserveState = view.DataContext as IPreserveState;
            if (preserveState == null) return;

            var pageKey = (string)view.GetValue(PageKeyProperty);
            var frameState = _suspensionManager.SessionStateForFrame(_frame);
            var pageState = new Dictionary<string, object>();
            preserveState.SaveState(pageState);
            frameState[pageKey] = pageState;
        }

        /// <summary>
        /// Restore previously saved state.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="navigationMode">The navigation mode.</param>
        protected void RestoreState(FrameworkElement view, NavigationMode navigationMode)
        {
            var frameState = _suspensionManager.SessionStateForFrame(_frame);
            var pageKey = "Page-" + _frame.BackStackDepth;
            view.SetValue(PageKeyProperty, pageKey);

            if (navigationMode == NavigationMode.New)
            {
                // Clear existing state for forward navigation when adding a new page to the navigation stack
                var nextPageKey = pageKey;
                int nextPageIndex = _frame.BackStackDepth;
                while (frameState.Remove(nextPageKey))
                {
                    nextPageIndex++;
                    nextPageKey = "Page-" + nextPageIndex;
                }
            }
            else
            {
                // Pass the preserved page state to the page, 
                // using the same strategy for loading suspended state and recreating pages discarded from cache
                var pageState = (Dictionary<string, object>)frameState[pageKey];
                var preserveState = view.DataContext as IPreserveState;
                if (preserveState != null && pageState != null && pageState.Count > 0)
                {
                    preserveState.RestoreState(pageState);
                }
            }
        }

        /// <summary>
        /// Called to check whether or not to close current instance on navigating.
        /// </summary>
        /// <param name="e"> The event args from OnNavigating event. </param>
        protected virtual bool CanCloseOnNavigating(NavigatingCancelEventArgs e)
        {
            return false;
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
        ///   Raised prior to navigation.
        /// </summary>
        public event NavigatingCancelEventHandler Navigating;

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
        /// Gets the data type of the content that is currently displayed.
        /// </summary>
        public Type CurrentSourcePageType
        {
            get { return _frame.CurrentSourcePageType; }
        }

        /// <summary>
        /// Navigates to the specified view type.
        /// </summary>
        /// <param name="sourcePageType"> The view type to navigate to.</param>
        /// <param name="parameter">The object parameter to pass to the target.</param>
        /// <returns> Whether or not navigation succeeded. </returns>
        public bool Navigate(Type sourcePageType, object parameter = null)
        {
            if (parameter == null)
                return _frame.Navigate(sourcePageType);
            return _frame.Navigate(sourcePageType, parameter);
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
