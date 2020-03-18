using System;
using System.Windows.Controls.Primitives;

namespace Caliburn.Light.WPF
{
    /// <summary>
    /// Integrate framework life-cycle handling with <see cref="Popup"/> events.
    /// </summary>
    public sealed class PopupLifecycle
    {
        private readonly Popup _view;
        private readonly string _context;

        /// <summary>
        /// Initializes a new instance of <see cref="PopupLifecycle"/>
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="context">The context in which the view appears.</param>
        public PopupLifecycle(Popup view, string context)
        {
            _view = view;
            _context = context;

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
        public Popup View => _view;

        /// <summary>
        /// Gets the context in which the view appears.
        /// </summary>
        public string Context => _context;

        private void OnViewOpened(object sender, EventArgs e)
        {
            if (_view.DataContext is IViewAware viewAware)
                viewAware.AttachView(_view, _context);

            if (_view.DataContext is IActivatable activatable)
                activatable.ActivateAsync().Observe();
        }

        private void OnViewClosed(object sender, EventArgs e)
        {
            if (_view.DataContext is IActivatable activatable)
                activatable.DeactivateAsync(true).Observe();

            if (_view.DataContext is IViewAware viewAware)
                viewAware.DetachView(_view, _context);
        }
    }
}
