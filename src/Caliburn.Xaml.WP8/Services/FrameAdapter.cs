using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace Caliburn.Light
{
    /// <summary>
    /// A basic implementation of <see cref="INavigationService" /> designed to adapt the <see cref="Frame" /> control.
    /// </summary>
    public class FrameAdapter : INavigationService
    {
        private readonly Frame _frame;

        /// <summary>
        ///   Creates an instance of <see cref="FrameAdapter" />
        /// </summary>
        /// <param name="frame"> The frame to represent as a <see cref="INavigationService" /> . </param>
        public FrameAdapter(Frame frame)
        {
            _frame = frame;
            _frame.Navigating += OnNavigating;
            _frame.Navigated += OnNavigated;
        }

        /// <summary>
        ///   Occurs before navigation
        /// </summary>
        /// <param name="sender"> The event sender. </param>
        /// <param name="e"> The event args. </param>
        protected virtual void OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            var handler = Navigating;
            if (handler != null)
                handler(sender, e);

            if (e.Cancel) return;

            var view = _frame.Content as FrameworkElement;
            if (view == null) return;

            var guard = view.DataContext as ICloseGuard;
            if (guard != null && !e.Uri.IsAbsoluteUri)
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

            var deactivator = view.DataContext as IDeactivate;

            // If we are navigating to the same page there is no need to deactivate
            // e.g. When the app is activated with Fast Switch
            if (deactivator != null && _frame.CurrentSource != e.Uri)
            {
                deactivator.Deactivate(CanCloseOnNavigating(sender, e));
            }
        }

        /// <summary>
        /// Called to check whether or not to close current instance on navigating.
        /// </summary>
        /// <param name="sender"> The event sender from OnNavigating event. </param>
        /// <param name="e"> The event args from OnNavigating event. </param>
        protected virtual bool CanCloseOnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            return false;
        }

        /// <summary>
        ///   Occurs after navigation
        /// </summary>
        /// <param name="sender"> The event sender. </param>
        /// <param name="e"> The event args. </param>
        protected virtual void OnNavigated(object sender, NavigationEventArgs e)
        {
            if (e.Uri.IsAbsoluteUri || e.Content == null) return;

            ViewLocator.InitializeComponent(e.Content);
            var viewModel = ViewModelLocator.LocateForView(e.Content);
            if (viewModel == null) return;

            var page = e.Content as PhoneApplicationPage;
            if (page == null)
            {
                throw new ArgumentException("View '" + e.Content.GetType().FullName +
                                            "' should inherit from PhoneApplicationPage or one of its descendents.");
            }

            TryInjectQueryString(viewModel, page);
            ViewModelBinder.Bind(viewModel, page, null);

            var activator = viewModel as IActivate;
            if (activator != null)
            {
                activator.Activate();
            }

            GC.Collect();
        }

        /// <summary>
        ///   Attempts to inject query string parameters from the view into the view model.
        /// </summary>
        /// <param name="viewModel"> The view model. </param>
        /// <param name="page"> The page. </param>
        protected virtual void TryInjectQueryString(object viewModel, Page page)
        {
            var viewModelType = viewModel.GetType();

            foreach (var pair in page.NavigationContext.QueryString)
            {
                var property = viewModelType.GetProperty(pair.Key);
                if (property == null) continue;

                property.SetValue(
                    viewModel,
                    ParameterBinder.CoerceValue(property.PropertyType, pair.Value),
                    null
                    );
            }
        }

        /// <summary>
        ///   The <see cref="Uri" /> source.
        /// </summary>
        public Uri Source
        {
            get { return _frame.Source; }
            set { _frame.Source = value; }
        }

        /// <summary>
        ///   Indicates whether the navigator can navigate back.
        /// </summary>
        public bool CanGoBack
        {
            get { return _frame.CanGoBack; }
        }

        /// <summary>
        ///   Indicates whether the navigator can navigate forward.
        /// </summary>
        public bool CanGoForward
        {
            get { return _frame.CanGoForward; }
        }

        /// <summary>
        ///   The current <see cref="Uri" /> source.
        /// </summary>
        public Uri CurrentSource
        {
            get { return _frame.CurrentSource; }
        }

        /// <summary>
        ///   The current content.
        /// </summary>
        public object CurrentContent
        {
            get { return _frame.Content; }
        }

        /// <summary>
        ///   Stops the loading process.
        /// </summary>
        public void StopLoading()
        {
            _frame.StopLoading();
        }

        /// <summary>
        ///   Navigates back.
        /// </summary>
        public void GoBack()
        {
            _frame.GoBack();
        }

        /// <summary>
        ///   Navigates forward.
        /// </summary>
        public void GoForward()
        {
            _frame.GoForward();
        }

        /// <summary>
        ///   Navigates to the specified <see cref="Uri" /> .
        /// </summary>
        /// <param name="source"> The <see cref="Uri" /> to navigate to. </param>
        /// <returns> Whether or not navigation succeeded. </returns>
        public bool Navigate(Uri source)
        {
            return _frame.Navigate(source);
        }

        /// <summary>
        ///   Removes the most recent entry from the back stack.
        /// </summary>
        /// <returns> The entry that was removed. </returns>
        public JournalEntry RemoveBackEntry()
        {
            return ((Page) _frame.Content).NavigationService.RemoveBackEntry();
        }

        /// <summary>
        ///   Gets an IEnumerable that you use to enumerate the entries in back navigation history.
        /// </summary>
        /// <returns>List of entries in the back stack.</returns>
        public IEnumerable<JournalEntry> BackStack
        {
            get { return ((Page) _frame.Content).NavigationService.BackStack; }
        }

        /// <summary>
        ///   Raised after navigation.
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
        ///   Raised when navigation fails.
        /// </summary>
        public event NavigationFailedEventHandler NavigationFailed
        {
            add { _frame.NavigationFailed += value; }
            remove { _frame.NavigationFailed -= value; }
        }

        /// <summary>
        ///   Raised when navigation is stopped.
        /// </summary>
        public event NavigationStoppedEventHandler NavigationStopped
        {
            add { _frame.NavigationStopped += value; }
            remove { _frame.NavigationStopped -= value; }
        }

        /// <summary>
        ///   Raised when a fragment navigation occurs.
        /// </summary>
        public event FragmentNavigationEventHandler FragmentNavigation
        {
            add { _frame.FragmentNavigation += value; }
            remove { _frame.FragmentNavigation -= value; }
        }
    }
}
