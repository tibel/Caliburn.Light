using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Caliburn.Light
{
    /// <summary>
    /// Integrate framework lifetime handling into <see cref="Frame"/> navigation events.
    /// </summary>
    public class FrameAdapter : IFrameAdapter
    {
        private static DependencyProperty FrameAdapterProperty =
            DependencyProperty.RegisterAttached("_FrameAdapter", typeof(AdapterImpl), typeof(FrameAdapter), null);

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

            var adapter = (AdapterImpl)frame.GetValue(FrameAdapterProperty);
            if (adapter != null) return;

            adapter = new AdapterImpl(this, frame);
            frame.SetValue(FrameAdapterProperty, adapter);
        }

        /// <summary>
        /// Detachtes this instrance from the <paramref name="frame"/>.
        /// </summary>
        /// <param name="frame">The frame to detatch from.</param>
        public void DetatchFrom(Frame frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            var adapter = (AdapterImpl)frame.GetValue(FrameAdapterProperty);
            if (adapter == null) return;

            frame.ClearValue(FrameAdapterProperty);
            adapter.Dispose();
        }

        private class AdapterImpl : IDisposable
        {
            private readonly FrameAdapter _parent;
            private readonly Frame _frame;
            private Page _previousPage;

            public AdapterImpl(FrameAdapter parent, Frame frame)
            {
                _parent = parent;
                _frame = frame;

                frame.Navigating += OnNavigating;
                frame.Navigated += OnNavigated;
                frame.NavigationFailed += OnNavigationFailed;
            }

            private void OnNavigating(object sender, NavigatingCancelEventArgs e)
            {
                var page = _frame.Content as Page;
                if (page == null) return;

                _parent.OnNavigatingFrom(page, e);

                if (!e.Cancel)
                    _previousPage = page;
            }

            private void OnNavigated(object sender, NavigationEventArgs e)
            {
                if (_previousPage != null)
                {
                    var previousPage = _previousPage;
                    _previousPage = null;
                    _parent.OnNavigatedFrom(previousPage, e);
                }

                var page = _frame.Content as Page;
                if (page == null) return;

                _parent.OnNavigatedTo(page, e);
            }

            private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
            {
                _previousPage = null;
            }

            public void Dispose()
            {
                _frame.Navigating -= OnNavigating;
                _frame.Navigated -= OnNavigated;
                _frame.NavigationFailed -= OnNavigationFailed;
            }
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
