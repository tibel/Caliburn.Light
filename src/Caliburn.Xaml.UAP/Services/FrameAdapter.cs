using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Caliburn.Light
{
    /// <summary>
    /// Integrate framework lifetime handling into <see cref="Frame"/> navigation events.
    /// </summary>
    public class FrameAdapter : IFrameAdapter
    {
        private readonly Dictionary<Frame, Page> _previousPage = new Dictionary<Frame, Page>();
        private readonly IViewModelLocator _viewModelLocator;
        private readonly IViewModelBinder _viewModelBinder;

        /// <summary>
        /// Creates an instance of <see cref="FrameAdapter" />.
        /// </summary>
        /// <param name="viewModelLocator">The view-model locator.</param>
        /// <param name="viewModelBinder">The view-model binder.</param>
        public FrameAdapter(IViewModelLocator viewModelLocator, IViewModelBinder viewModelBinder)
        {
            if (viewModelLocator == null)
                throw new ArgumentNullException(nameof(viewModelLocator));
            if (viewModelBinder == null)
                throw new ArgumentNullException(nameof(viewModelBinder));

            _viewModelLocator = viewModelLocator;
            _viewModelBinder = viewModelBinder;
        }

        /// <summary>
        /// Attaches this instance to the <paramref name="frame"/>.
        /// </summary>
        /// <param name="frame">The frame to attach to.</param>
        public void AttachTo(Frame frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            frame.Navigating += OnNavigating;
            frame.Navigated += OnNavigated;
            frame.NavigationFailed += OnNavigationFailed;
        }

        /// <summary>
        /// Detachtes this instrance from the <paramref name="frame"/>.
        /// </summary>
        /// <param name="frame">The frame to detatch from.</param>
        public void DetatchFrom(Frame frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            frame.Navigating -= OnNavigating;
            frame.Navigated -= OnNavigated;
            frame.NavigationFailed -= OnNavigationFailed;
            _previousPage.Remove(frame);
        }

        private void OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            var frame = (Frame)sender;
            var page = frame.Content as Page;
            if (page == null) return;

            OnNavigatingFrom(page, e);

            if (!e.Cancel)
                _previousPage[frame] = page;
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            var frame = (Frame)sender;

            Page previousPage;
            if (_previousPage.TryGetValue(frame, out previousPage))
            {
                _previousPage.Remove(frame);
                OnNavigatedFrom(previousPage, e);
            }

            var page = frame.Content as Page;
            if (page == null) return;

            OnNavigatedTo(page, e);
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            var frame = (Frame)sender;
            _previousPage.Remove(frame);
        }

        private void OnNavigatingFrom(Page page, NavigatingCancelEventArgs e)
        {
            if (e.Cancel) return;

            var guard = page.DataContext as ICloseGuard;
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
        }

        private void OnNavigatedFrom(Page page, NavigationEventArgs e)
        {
            OnNavigatedFromCore(page, e);

            var deactivator = page.DataContext as IDeactivate;
            if (deactivator != null)
            {
                var close = page.NavigationCacheMode == NavigationCacheMode.Disabled;
                deactivator.Deactivate(close);
            }
        }

        /// <summary>
        /// Calls <see cref="INavigationAware.OnNavigatedFrom" /> on the view model
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="e">The event data.</param>
        protected virtual void OnNavigatedFromCore(Page page, NavigationEventArgs e)
        {
            var navigationAware = page.DataContext as INavigationAware;
            if (navigationAware != null)
            {
                navigationAware.OnNavigatedFrom();
            }
        }

        private void OnNavigatedTo(Page page, NavigationEventArgs e)
        {
            if (page.DataContext == null)
            {
                var viewModel = _viewModelLocator.LocateForView(page);
                _viewModelBinder.Bind(viewModel, page, null);
            }

            OnNavigatedToCore(page, e);

            var activator = page.DataContext as IActivate;
            if (activator != null)
            {
                activator.Activate();
            }
        }

        /// <summary>
        /// Calls <see cref="INavigationAware.OnNavigatedTo(object)" /> on the view model.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="e">The event data.</param>
        protected virtual void OnNavigatedToCore(Page page, NavigationEventArgs e)
        {
            var navigationAware = page.DataContext as INavigationAware;
            if (navigationAware != null)
            {
                navigationAware.OnNavigatedTo(e.Parameter);
            }
        }
    }
}
