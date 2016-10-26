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
        public ShellViewModel()
        {
            ExecuteCommand = DelegateCommandBuilder.NoParameter()
                .OnExecute(() => OnExecute())
                .Build();

            UIContextRunCommand = DelegateCommandBuilder.NoParameter()
                .OnExecute(() => OnUIContextRun())
                .Build();

            TaskRunCommand = DelegateCommandBuilder.NoParameter()
                .OnExecute(() => OnTaskRun())
                .Build();

            AsyncCommand = DelegateCommandBuilder.NoParameter()
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

        private Task OnUIContextRun()
        {
            return Task.Run(() =>
            {
                Debug.Assert(!UIContext.CheckAccess());
                Thread.Sleep(100);

                return UIContext.Run(new Action(() =>
                {
                    Debug.Assert(UIContext.CheckAccess());
                    Thread.Sleep(100);
                    throw new InvalidOperationException("Error on a background Task.");
                }));
            });
        }

        private Task OnTaskRun()
        {
            return Task.Run(() =>
            {
                Debug.Assert(!UIContext.CheckAccess());
                Thread.Sleep(100);
                throw new InvalidOperationException("Error on a background Task.");
            });
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
