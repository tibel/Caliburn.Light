using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Light;

namespace Demo.ExceptionHandling
{
    public class ShellViewModel : BindableObject
    {
        private readonly IUIContext _uIContext;

        public ShellViewModel(IUIContext uIContext)
        {
            _uIContext = uIContext;

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

        private void OnExecute()
        {
            Debug.Assert(_uIContext.CheckAccess());
            throw new InvalidOperationException("Error on execute.");
        }

        private Task OnUIContextRun()
        {
            return Task.Run(() =>
            {
                Debug.Assert(!_uIContext.CheckAccess());
                Thread.Sleep(100);

                var operation = _uIContext.Run(new Action(() =>
                {
                    Debug.Assert(_uIContext.CheckAccess());
                    Thread.Sleep(100);
                    throw new InvalidOperationException("Error on a background Task.");
                }));

                return operation;
            });
        }

        private Task OnTaskRun()
        {
            return Task.Run(() =>
            {
                Debug.Assert(!_uIContext.CheckAccess());
                Thread.Sleep(100);
                throw new InvalidOperationException("Error on a background Task.");
            });
        }

        private async Task OnAsync()
        {
            Debug.Assert(_uIContext.CheckAccess());
            await Task.Delay(100);
            Debug.Assert(_uIContext.CheckAccess());
            throw new InvalidOperationException("Error on asynchronous execute.");
        }
    }
}
