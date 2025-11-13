using Microsoft.UI.Xaml.Controls.Primitives;

namespace Caliburn.Light.WinUI
{
    /// <summary>
    /// Integrate framework life-cycle handling with <see cref="Popup"/> events.
    /// </summary>
    public sealed class PopupLifecycle
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PopupLifecycle"/>
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="context">The context in which the view appears.</param>
        public PopupLifecycle(Popup view, string? context)
        {
            View = view;
            Context = context;

            var viewModel = view.DataContext;

            if (viewModel is IViewAware || viewModel is IActivatable)
            {
                view.Opened += OnViewOpened;
                view.Closed += OnViewClosed;
            }
        }

        /// <summary>
        /// Gets the view.
        /// </summary>
        public Popup View { get; }

        /// <summary>
        /// Gets the context in which the view appears.
        /// </summary>
        public string? Context { get; }

        private void OnViewOpened(object? sender, object e)
        {
            if (View.DataContext is IViewAware viewAware)
                viewAware.AttachView(View, Context);

            if (View.DataContext is IActivatable activatable)
                activatable.ActivateAsync().Observe();
        }

        private void OnViewClosed(object? sender, object e)
        {
            if (View.DataContext is IActivatable activatable)
                activatable.DeactivateAsync(true).Observe();

            if (View.DataContext is IViewAware viewAware)
                viewAware.DetachView(View, Context);
        }
    }
}
