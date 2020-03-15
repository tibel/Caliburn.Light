using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace Caliburn.Light.WPF
{
    /// <summary>
    /// Conducts a window with its view-model.
    /// </summary>
    public sealed class WindowConductor
    {
        private readonly Window _view;
        private readonly object _model;

        private bool _deactivatingFromView;
        private bool _deactivateFromViewModel;
        private bool _actuallyClosing;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowConductor"/> class.
        /// </summary>
        /// <param name="model">The view model.</param>
        /// <param name="view">The view.</param>
        public WindowConductor(object model, Window view)
        {
            _model = model;
            _view = view;

            if (model is IActivatable activatable)
            {
                activatable.ActivateAsync().Observe();

                view.Closed += OnViewClosed;
                activatable.Deactivated += OnModelDeactivated;
            }

            if (model is ICloseGuard guard)
                view.Closing += OnViewClosing;
        }

        /// <summary>
        /// Gets the view-model.
        /// </summary>
        public object Model => _model;

        /// <summary>
        /// Gets the view.
        /// </summary>
        public Window View => _view;

        private void OnViewClosing(object sender, CancelEventArgs e)
        {
            if (e.Cancel)
                return;

            if (_actuallyClosing)
            {
                _actuallyClosing = false;
                return;
            }

            e.Cancel = !EvaluateCanClose();
        }

        private async void OnViewClosed(object sender, EventArgs e)
        {
            _view.Closed -= OnViewClosed;
            _view.Closing -= OnViewClosing;

            if (_deactivateFromViewModel)
                return;

            _deactivatingFromView = true;
            await ((IActivatable)_model).DeactivateAsync(true);
            _deactivatingFromView = false;
        }

        private void OnModelDeactivated(object sender, DeactivationEventArgs e)
        {
            if (!e.WasClosed)
                return;

            ((IActivatable)_model).Deactivated -= OnModelDeactivated;

            if (_deactivatingFromView)
                return;

            _deactivateFromViewModel = true;
            _actuallyClosing = true;
            _view.Close();
            _actuallyClosing = false;
            _deactivateFromViewModel = false;
        }

        private bool EvaluateCanClose()
        {
            var task = ((ICloseGuard)_model).CanCloseAsync();
            if (task.IsCompleted)
                return task.Result;

            CloseViewAsync(task);
            return false;
        }

        private async void CloseViewAsync(Task<bool> task)
        {
            var canClose = await task.ConfigureAwait(true);
            if (!canClose)
                return;

            _actuallyClosing = true;
            _view.Close();
            _actuallyClosing = false;
        }
    }
}
