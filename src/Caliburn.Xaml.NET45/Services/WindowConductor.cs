using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace Caliburn.Light
{
    internal class WindowConductor
    {
        public readonly Window View;
        public readonly object Model;

        private bool _deactivatingFromView;
        private bool _deactivateFromViewModel;
        private bool _actuallyClosing;

        public WindowConductor(object model, Window view)
        {
            Model = model;
            View = view;

            var activatable = model as IActivate;
            if (activatable != null)
                activatable.Activate();

            var deactivatable = model as IDeactivate;
            if (deactivatable != null)
            {
                view.Closed += OnViewClosed;
                deactivatable.Deactivated += OnDeactivated;
            }

            var guard = model as ICloseGuard;
            if (guard != null)
                view.Closing += OnViewClosing;
        }

        private void OnViewClosed(object sender, EventArgs e)
        {
            View.Closed -= OnViewClosed;
            View.Closing -= OnViewClosing;

            if (_deactivateFromViewModel)
                return;

            _deactivatingFromView = true;
            ((IDeactivate)Model).Deactivate(true);
            _deactivatingFromView = false;
        }

        private void OnDeactivated(object sender, DeactivationEventArgs e)
        {
            if (!e.WasClosed)
                return;

            ((IDeactivate)Model).Deactivated -= OnDeactivated;

            if (_deactivatingFromView)
                return;

            _deactivateFromViewModel = true;
            _actuallyClosing = true;
            View.Close();
            _actuallyClosing = false;
            _deactivateFromViewModel = false;
        }

        private void OnViewClosing(object sender, CancelEventArgs e)
        {
            if (e.Cancel)
                return;

            if (_actuallyClosing)
            {
                _actuallyClosing = false;
                return;
            }

            var task = ((ICloseGuard)Model).CanCloseAsync();
            if (task.IsCompleted)
            {
                var canClose = task.Result;
                e.Cancel = !canClose;
            }
            else
            {
                e.Cancel = true;
                CloseViewAsync(task);
            }
        }

        private async void CloseViewAsync(Task<bool> task)
        {
            var canClose = await task;
            if (canClose)
            {
                _actuallyClosing = true;
                View.Close();
            }
        }
    }
}
