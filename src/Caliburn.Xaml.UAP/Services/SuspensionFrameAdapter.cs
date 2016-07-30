using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Caliburn.Light
{
    /// <summary>
    /// Extends <see cref="FrameAdapter"/> with <see cref="ISuspensionManager"/> support.
    /// </summary>
    public class SuspensionFrameAdapter : FrameAdapter
    {
        private static DependencyProperty PageKeyProperty =
            DependencyProperty.RegisterAttached("_PageKey", typeof(string), typeof(SuspensionFrameAdapter), null);

        private readonly ISuspensionManager _suspensionManager;

        /// <summary>
        /// Creates an instance of <see cref="SuspensionFrameAdapter" />.
        /// </summary>
        /// <param name="viewModelLocator">The view-model locator.</param>
        /// <param name="viewModelBinder">The view-model binder.</param>
        /// <param name="suspensionManager">The suspension manager.</param>
        public SuspensionFrameAdapter(IViewModelLocator viewModelLocator, IViewModelBinder viewModelBinder, ISuspensionManager suspensionManager)
            : base(viewModelLocator, viewModelBinder)
        {
            if (suspensionManager == null)
                throw new ArgumentNullException(nameof(suspensionManager));

            _suspensionManager = suspensionManager;
        }

        /// <summary>
        /// Calls <see cref="FrameAdapter.OnNavigatedFromCore(Page, NavigationEventArgs)"/> and saves the page state.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="e">The event data.</param>
        protected override void OnNavigatedFromCore(Page page, NavigationEventArgs e)
        {
            base.OnNavigatedFromCore(page, e);
            SaveState(page);
        }

        /// <summary>
        /// Restores the page state and calls <see cref="FrameAdapter.OnNavigatedToCore(Page, NavigationEventArgs)"/>
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="e">The event data.</param>
        protected override void OnNavigatedToCore(Page page, NavigationEventArgs e)
        {
            RestoreState(page, e.NavigationMode);
            base.OnNavigatedToCore(page, e);
        }

        private void SaveState(Page page)
        {
            var preserveState = page.DataContext as IPreserveState;
            if (preserveState == null) return;

            var pageKey = (string)page.GetValue(PageKeyProperty);
            var frameState = _suspensionManager.SessionStateForFrame(page.Frame);
            var pageState = new Dictionary<string, object>();
            preserveState.SaveState(pageState);
            frameState[pageKey] = pageState;
        }

        private void RestoreState(Page page, NavigationMode navigationMode)
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
