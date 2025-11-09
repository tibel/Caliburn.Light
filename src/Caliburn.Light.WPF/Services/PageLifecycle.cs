using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Caliburn.Light.WPF
{
    /// <summary>
    /// Integrate framework life-cycle handling with <see cref="Page"/> navigation.
    /// </summary>
    public sealed class PageLifecycle
    {
        private readonly IViewModelLocator _viewModelLocator;
        private bool _actuallyNavigating;
        private Page? _previousPage;

        /// <summary>
        /// Initializes a new instance of <see cref="PageLifecycle"/>
        /// </summary>
        /// <param name="navigationService">The navigation service.</param>
        /// <param name="context">The context in which the view appears.</param>
        /// <param name="viewModelLocator">The view model locator.</param>
        public PageLifecycle(NavigationService navigationService, string? context, IViewModelLocator viewModelLocator)
        {
            _viewModelLocator = viewModelLocator;

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

            if (_actuallyNavigating)
            {
                _actuallyNavigating = false;
                _previousPage = page;
                return;
            }

            if (!EvaluateCanClose(page, e))
            {
                e.Cancel = true;
                return;
            }

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

        private void OnNavigatedFrom(Page page)
        {
            if (page.DataContext is IActivatable activatable)
                activatable.DeactivateAsync(!page.KeepAlive).Observe();

            if (page.DataContext is IViewAware viewAware)
                viewAware.DetachView(page, Context);
        }

        private void OnNavigatedTo(Page page)
        {
            if (page.DataContext is null)
                page.DataContext = _viewModelLocator.LocateForView(page);

            if (page.DataContext is IViewAware viewAware)
                viewAware.AttachView(page, Context);

            if (page.DataContext is IActivatable activatable)
                activatable.ActivateAsync().Observe();
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            _previousPage = null;
        }

        private bool EvaluateCanClose(Page page, NavigatingCancelEventArgs e)
        {
            if (page.DataContext is not ICloseGuard guard)
                return true;

            var task = guard.CanCloseAsync();
            if (task.IsCompleted)
                return task.Result;

            CloseViewAsync(task, e);
            return false;
        }

        private async void CloseViewAsync(Task<bool> task, NavigatingCancelEventArgs e)
        {
            var canClose = await task.ConfigureAwait(true);
            if (!canClose)
                return;

            _actuallyNavigating = true;

            switch (e.NavigationMode)
            {
                case NavigationMode.New:
                    if (e.Content is not null)
                        NavigationService.Navigate(e.Content, e.ExtraData);
                    else
                        NavigationService.Navigate(e.Uri, e.ExtraData);
                    break;
                case NavigationMode.Back:
                    NavigationService.GoBack();
                    break;
                case NavigationMode.Forward:
                    NavigationService.GoForward();
                    break;
                case NavigationMode.Refresh:
                    NavigationService.Refresh();
                    break;
            }

            _actuallyNavigating = false;
        }
    }
}
