using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Caliburn.Light
{
    /// <summary>
    /// Integrate framework lifetime handling into <see cref="Page"/> navigation events.
    /// </summary>
    public class PageAdapter : IPageAdapter
    {
        private readonly IViewModelLocator _viewModelLocator;
        private readonly IViewModelBinder _viewModelBinder;

        /// <summary>
        /// Creates an instance of <see cref="PageAdapter" />.
        /// </summary>
        /// <param name="viewModelLocator">The view-model locator.</param>
        /// <param name="viewModelBinder">The view-model binder.</param>
        public PageAdapter(IViewModelLocator viewModelLocator, IViewModelBinder viewModelBinder)
        {
            if (viewModelLocator == null)
                throw new ArgumentNullException(nameof(viewModelLocator));
            if (viewModelBinder == null)
                throw new ArgumentNullException(nameof(viewModelBinder));

            _viewModelLocator = viewModelLocator;
            _viewModelBinder = viewModelBinder;
        }

        /// <summary>
        /// Invoked immediately before the Page is unloaded and is no longer the current source of a parent Frame.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="e">The event data.</param>
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

        /// <summary>
        /// Invoked immediately after the Page is unloaded and is no longer the current source of a parent Frame.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="e">The event data.</param>
        public void OnNavigatedFrom(Page page, NavigationEventArgs e)
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

        /// <summary>
        /// Invoked when the Page is loaded and becomes the current source of a parent Frame.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="e">The event data.</param>
        public void OnNavigatedTo(Page page, NavigationEventArgs e)
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
