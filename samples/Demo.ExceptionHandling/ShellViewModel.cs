using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Light;
using Weakly;

namespace Demo.ExceptionHandling
{
    public class ShellViewModel : BindableObject
    {
        public ShellViewModel()
        {
            ExecuteCommand = DelegateCommand
                .NoParameter()
                .OnExecute(() => OnExecute())
                .Build();

            UIContextRunCommand = DelegateCommand
                .NoParameter()
                .OnExecute(() => OnUIContextRun())
                .Build();

            TaskRunCommand = DelegateCommand
                .NoParameter()
                .OnExecute(() => OnTaskRun())
                .Build();

            AsyncCommand = DelegateCommand
                .NoParameter()
                .OnExecute(() => OnAsync())
                .Build();
        }

        public ICommand ExecuteCommand { get; private set; }
        public ICommand UIContextRunCommand { get; private set; }
        public ICommand TaskRunCommand { get; private set; }
        public ICommand AsyncCommand { get; private set; }

        private void OnExecute()
        {
            Debug.Assert(UIContext.CheckAccess());
            throw new InvalidOperationException("Error on execute.");
        }

        private void OnUIContextRun()
        {
            Task.Run(() =>
            {
                Debug.Assert(!UIContext.CheckAccess());
                Thread.Sleep(100);
                UIContext.Run(new Action(() =>
                {
                    Debug.Assert(UIContext.CheckAccess());
                    Thread.Sleep(100);
                    throw new InvalidOperationException("Error on a background Task.");
                })).ObserveException();
            }).ObserveException();
        }

        private void OnTaskRun()
        {
            Task.Run(() =>
            {
                Debug.Assert(!UIContext.CheckAccess());
                Thread.Sleep(100);
                throw new InvalidOperationException("Error on a background Task.");
            }).ObserveException();
        }

        private async Task OnAsync()
        {
            Debug.Assert(UIContext.CheckAccess());
            await Task.Delay(100);
            Debug.Assert(UIContext.CheckAccess());
            throw new InvalidOperationException("Error on async execute.");
        }
    }
}
