using System;
using System.Collections.Generic;
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
        private static DependencyProperty PageKeyProperty =
            DependencyProperty.RegisterAttached("_PageKey", typeof(string), typeof(FrameAdapter), null);

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
            adapter.FrameState = new Dictionary<string, object>();
            frame.SetValue(FrameAdapterProperty, adapter);
        }

        /// <summary>
        /// Detaches this instance from the <paramref name="frame"/>.
        /// </summary>
        /// <param name="frame">The frame to detach from.</param>
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

            public IDictionary<string, object> FrameState { get; set; }

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
                    _parent.OnNavigatedFrom(previousPage, e, FrameState);
                }

                var page = _frame.Content as Page;
                if (page == null) return;

                _parent.OnNavigatedTo(page, e, FrameState);
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
                    throw new NotSupportedException("Asynchronous task is not supported.");

                if (!task.Result)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void OnNavigatedFrom(Page page, NavigationEventArgs e, IDictionary<string, object> frameState)
        {
            var navigationAware = page.DataContext as INavigationAware;
            if (navigationAware != null)
            {
                navigationAware.OnNavigatedFrom();
            }

            SavePageState(page, frameState);

            var deactivator = page.DataContext as IDeactivate;
            if (deactivator != null)
            {
                var close = page.NavigationCacheMode == NavigationCacheMode.Disabled;
                deactivator.Deactivate(close);
            }
        }

        private void OnNavigatedTo(Page page, NavigationEventArgs e, IDictionary<string, object> frameState)
        {
            if (page.DataContext == null)
            {
                var viewModel = _viewModelLocator.LocateForView(page);
                _viewModelBinder.Bind(viewModel, page, null, true);
            }

            RestorePageState(page, e.NavigationMode, frameState);

            var navigationAware = page.DataContext as INavigationAware;
            if (navigationAware != null)
            {
                navigationAware.OnNavigatedTo(e.Parameter);
            }

            var activator = page.DataContext as IActivate;
            if (activator != null)
            {
                activator.Activate();
            }
        }

        /// <summary>
        /// Save the current state for <paramref name="frame"/>.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns>The internal frame state dictionary.</returns>
        public IDictionary<string, object> SaveState(Frame frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            var adapter = (AdapterImpl)frame.GetValue(FrameAdapterProperty);
            if (adapter == null)
                throw new InvalidOperationException("Adapter is not attached to frame.");

            var frameState = adapter.FrameState;

            var currentPage = frame.Content as Page;
            if (currentPage != null)
            {
                SavePageState(currentPage, frameState);
            }

            frameState["Navigation"] = frame.GetNavigationState();
 
            return frameState;
        }

        /// <summary>
        ///  Restores previously saved for <paramref name="frame"/>.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="frameState">The state dictionary that will be used.</param>
        public void RestoreState(Frame frame, IDictionary<string, object> frameState)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));
            if (frameState == null)
                throw new ArgumentNullException(nameof(frameState));

            var adapter = (AdapterImpl)frame.GetValue(FrameAdapterProperty);
            if (adapter == null)
                throw new InvalidOperationException("Adapter is not attached to frame.");

            adapter.FrameState = frameState;

            if (frameState.ContainsKey("Navigation"))
            {
                frame.SetNavigationState((string)frameState["Navigation"]);
            }
        }

        private void SavePageState(Page page, IDictionary<string, object> frameState)
        {
            var preserveState = page.DataContext as IPreserveState;
            if (preserveState == null) return;

            var pageKey = (string)page.GetValue(PageKeyProperty);
            var pageState = new Dictionary<string, object>();
            preserveState.SaveState(pageState);
            frameState[pageKey] = pageState;
        }

        private void RestorePageState(Page page, NavigationMode navigationMode, IDictionary<string, object> frameState)
        {
            var frame = page.Frame;
            var pageKey = "Page-" + frame.BackStackDepth;
            page.SetValue(PageKeyProperty, pageKey);

            if (navigationMode == NavigationMode.New)
            {
                // Clear existing state for forward navigation when adding a new page to the navigation stack
                var nextPageKey = pageKey;
                int nextPageIndex = frame.BackStackDepth;
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
                var preserveState = page.DataContext as IPreserveState;
                if (preserveState != null)
                {
                    var pageState = (Dictionary<string, object>)frameState[pageKey];
                    preserveState.RestoreState(pageState);
                }
            }
        }
    }
}
