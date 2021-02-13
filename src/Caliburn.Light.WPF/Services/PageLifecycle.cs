using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Caliburn.Light.WPF
{
    /// <summary>
    /// Integrate framework life-cycle handling with <see cref="Page"/> navigation.
    /// </summary>
    internal sealed class PageLifecycle
    {
        private static readonly DependencyProperty LifecycleProperty =
            DependencyProperty.RegisterAttached("_Lifecycle", typeof(PageLifecycle), typeof(PageLifecycle), null);

        /// <summary>
        /// Attaches the framework life-cycle handling to <see cref="Frame"/>.
        /// </summary>
        /// <param name="frame">The navigation control.</param>
        /// <returns>The attached life-cycle instance.</returns>
        public static PageLifecycle AttachTo(Frame frame)
        {
            if (frame is null)
                throw new ArgumentNullException(nameof(frame));

            var lifecycle = (PageLifecycle)frame.GetValue(LifecycleProperty);
            if (lifecycle is null)
            {
                lifecycle = new PageLifecycle();

                frame.Navigating += lifecycle.OnNavigating;
                frame.Navigated += lifecycle.OnNavigated;
                frame.NavigationFailed += lifecycle.OnNavigationFailed;

                frame.SetValue(LifecycleProperty, lifecycle);
            }

            return lifecycle;
        }

        /// <summary>
        /// Attaches the framework life-cycle handling to <see cref="NavigationWindow"/>.
        /// </summary>
        /// <param name="navigationWindow">The navigation control.</param>
        /// <returns>The attached life-cycle instance.</returns>
        public static PageLifecycle AttachTo(NavigationWindow navigationWindow)
        {
            if (navigationWindow is null)
                throw new ArgumentNullException(nameof(navigationWindow));

            var lifecycle = (PageLifecycle)navigationWindow.GetValue(LifecycleProperty);
            if (lifecycle is null)
            {
                lifecycle = new PageLifecycle();

                navigationWindow.Navigating += lifecycle.OnNavigating;
                navigationWindow.Navigated += lifecycle.OnNavigated;
                navigationWindow.NavigationFailed += lifecycle.OnNavigationFailed;

                navigationWindow.SetValue(LifecycleProperty, lifecycle);
            }

            return lifecycle;
        }

        private Page _previousPage;

        private PageLifecycle()
        {
        }

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
