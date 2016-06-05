using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Caliburn.Light
{
    public class PageAdapter : IPageAdapter
    {
        private static DependencyProperty PageKeyProperty =
            DependencyProperty.RegisterAttached("_PageKey", typeof(string), typeof(PageAdapter), null);

        private readonly IViewModelLocator _viewModelLocator;
        private readonly IViewModelBinder _viewModelBinder;
        private readonly ISuspensionManager _suspensionManager;

        /// <summary>
        /// Creates an instance of <see cref="PageAdapter" />.
        /// </summary>
        /// <param name="viewModelLocator">The view-model locator.</param>
        /// <param name="viewModelBinder">The view-model binder.</param>
        /// <param name="suspensionManager">The suspension manager.</param>
        public PageAdapter(IViewModelLocator viewModelLocator, IViewModelBinder viewModelBinder, ISuspensionManager suspensionManager)
        {
            if (viewModelLocator == null)
                throw new ArgumentNullException(nameof(viewModelLocator));
            if (viewModelBinder == null)
                throw new ArgumentNullException(nameof(viewModelBinder));
            if (suspensionManager == null)
                throw new ArgumentNullException(nameof(suspensionManager));

            _viewModelLocator = viewModelLocator;
            _viewModelBinder = viewModelBinder;
            _suspensionManager = suspensionManager;
        }

        public void OnNavigatingFrom(Page page, NavigatingCancelEventArgs e)
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

        public void OnNavigatedFrom(Page page, NavigationEventArgs e)
        {
            var navigationAware = page.DataContext as INavigationAware;
            if (navigationAware != null)
            {
                navigationAware.OnNavigatedFrom();
            }

            SaveState(page);

            var deactivator = page.DataContext as IDeactivate;
            if (deactivator != null)
            {
                var close = page.NavigationCacheMode == NavigationCacheMode.Disabled;
                deactivator.Deactivate(close);
            }
        }

        public void OnNavigatedTo(Page page, NavigationEventArgs e)
        {
            EnsureViewModel(page);
            RestoreState(page, e.NavigationMode);

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
        /// Ensures that the DataContext is set.
        /// </summary>
        /// <param name="page">The page.</param>
        protected void EnsureViewModel(Page page)
        {
            if (page.DataContext == null)
            {
                var viewModel = _viewModelLocator.LocateForView(page);
                _viewModelBinder.Bind(viewModel, page, null);
            }
        }

        /// <summary>
        /// Saves the current state.
        /// </summary>
        /// <param name="page">The page.</param>
        protected void SaveState(Page page)
        {
            var preserveState = page.DataContext as IPreserveState;
            if (preserveState == null) return;

            var pageKey = (string)page.GetValue(PageKeyProperty);
            var frameState = _suspensionManager.SessionStateForFrame(page.Frame);
            var pageState = new Dictionary<string, object>();
            preserveState.SaveState(pageState);
            frameState[pageKey] = pageState;
        }

        /// <summary>
        /// Restore previously saved state.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="navigationMode">The navigation mode.</param>
        protected void RestoreState(Page page, NavigationMode navigationMode)
        {
            var frame = page.Frame;
            var frameState = _suspensionManager.SessionStateForFrame(frame);
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
