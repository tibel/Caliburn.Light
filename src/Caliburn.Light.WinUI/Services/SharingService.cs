using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Caliburn.Light.WinUI
{
    /// <summary>
    /// Service that handles the <see cref="DataTransferManager.DataRequested"/> event.
    /// </summary>
    public class SharingService : ISharingService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SharingService" /> class.
        /// </summary>
        public SharingService()
        {
            var transferManager = DataTransferManager.GetForCurrentView();
            transferManager.DataRequested += OnDataRequested;
        }

        /// <summary>
        /// Programmatically initiates the user interface for sharing content with another app.
        /// </summary>
        public void ShowShareUI()
        {
            DataTransferManager.ShowShareUI();
        }

        /// <summary>
        /// Accepts the share request and forwards it to the view model.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="DataRequestedEventArgs" /> instance containing the event data.</param>
        protected virtual void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var view = GetCurrentView();
            if (view is null) return;

            if (view.DataContext is not ISupportSharing supportSharing) return;

            supportSharing.OnShareRequested(args.Request);
        }

        /// <summary>
        /// Determines the current view, checks for view first with frame and then view mode first with a shell view.
        /// </summary>
        /// <returns>The current view</returns>
        protected virtual FrameworkElement GetCurrentView()
        {
            var content = Window.Current.Content;

            if (content is Frame frame)
                return frame.Content as FrameworkElement;

            return content as FrameworkElement;
        }
    }
}
