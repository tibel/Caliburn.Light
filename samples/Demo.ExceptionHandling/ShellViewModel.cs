using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Light;

namespace Demo.ExceptionHandling
{
    public class ShellViewModel : ViewAware
    {
        private IDispatcher _dispatcher;

        public ShellViewModel()
        {
            _dispatcher = CurrentThreadDispatcher.Instance;

            ExecuteCommand = DelegateCommandBuilder.NoParameter()
                .OnExecute(OnExecute)
                .Build();

            UIContextRunCommand = DelegateCommandBuilder.NoParameter()
                .OnExecute(OnUIContextRun)
                .Build();

            TaskRunCommand = DelegateCommandBuilder.NoParameter()
                .OnExecute(OnTaskRun)
                .Build();

            AsyncCommand = DelegateCommandBuilder.NoParameter()
                .OnExecute(OnAsync)
                .Build();
        }

        public ICommand ExecuteCommand { get; }
        public ICommand UIContextRunCommand { get; }
        public ICommand TaskRunCommand { get; }
        public ICommand AsyncCommand { get; }

        protected override void OnViewAttached(object view, string context)
        {
            _dispatcher = ViewHelper.GetDispatcher(view);
            base.OnViewAttached(view, context);
        }

        private void OnExecute()
        {
            Debug.Assert(_dispatcher.CheckAccess());
            throw new InvalidOperationException("Error on execute.");
        }

        private async Task OnUIContextRun()
        {
            await Task.Delay(10).ConfigureAwait(false);
            Debug.Assert(!_dispatcher.CheckAccess());

            await _dispatcher.SwitchTo();
            Debug.Assert(_dispatcher.CheckAccess());

            throw new InvalidOperationException("Error on a UI task.");
        }

        private async Task OnTaskRun()
        {
            await Task.Delay(10).ConfigureAwait(false);
            Debug.Assert(!_dispatcher.CheckAccess());
            throw new InvalidOperationException("Error on a background task.");
        }

        private async Task OnAsync()
        {
            Debug.Assert(_dispatcher.CheckAccess());
            await Task.Delay(10).ConfigureAwait(true);
            Debug.Assert(_dispatcher.CheckAccess());
            throw new InvalidOperationException("Error on UI task.");
        }
    }
}
