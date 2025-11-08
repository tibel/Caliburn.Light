using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Caliburn.Light.WPF
{
    /// <summary>
    /// Integrate framework life-cycle handling with <see cref="Page"/> navigation.
    /// </summary>
    public sealed class PageLifecycle
    {
        private Page? _previousPage;

        /// <summary>
        /// Initializes a new instance of <see cref="PageLifecycle"/>
        /// </summary>
        /// <param name="navigationService">The navigation service</param>
        /// <param name="context">The context in which the view appears.</param>
        public PageLifecycle(NavigationService navigationService, string? context)
        {
            NavigationService = navigationService;
            Context = context;

            navigationService.Navigating += OnNavigating;
            navigationService.Navigated += OnNavigated;
            navigationService.NavigationFailed += OnNavigationFailed;
        }

        /// <summary>
        /// Gets the navigation service.
        /// </summary>
        public NavigationService NavigationService { get; }

        /// <summary>
        /// Gets the context in which the view appears.
        /// </summary>
        public string? Context { get; }

        private void OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Cancel)
                return;

            var parent = (ContentControl)sender;
            if (parent.Content is not Page page)
                return;

            if (page.DataContext is ICloseGuard guard)
            {
                var task = guard.CanCloseAsync();
                if (!task.IsCompleted)
                    throw new NotSupportedException("Asynchronous task is not supported yet.");

                if (!task.Result)
                    e.Cancel = true;
            }

            if (!e.Cancel)
                _previousPage = page;
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            var parent = (ContentControl)sender;

            if (_previousPage is not null)
            {
                var previousPage = _previousPage;
                _previousPage = null;
                OnNavigatedFrom(previousPage);
            }

            if (parent.Content is Page page)
                OnNavigatedTo(page);
        }

        private static void OnNavigatedFrom(Page page)
        {
            if (page.DataContext is IActivatable activatable)
                activatable.DeactivateAsync(!page.KeepAlive).Observe();

            if (page.DataContext is IViewAware viewAware)
                viewAware.DetachView(page, View.GetContext(page));
        }

        private static void OnNavigatedTo(Page page)
        {
            if (page.DataContext is IViewAware viewAware)
                viewAware.AttachView(page, View.GetContext(page));

            if (page.DataContext is IActivatable activatable)
                activatable.ActivateAsync().Observe();
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            _previousPage = null;
        }
    }
}
