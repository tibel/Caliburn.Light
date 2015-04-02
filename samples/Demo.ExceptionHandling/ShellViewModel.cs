using System;
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
            ExecuteCommand = DelegateCommand.Create(OnExecute);
            UIContextRunCommand = DelegateCommand.Create(OnUIContextRun);
        }

        public ICommand ExecuteCommand { get; private set; }
        public ICommand UIContextRunCommand { get; private set; }

        private void OnExecute()
        {
            throw new InvalidOperationException("Error on execute.");
        }

        private void OnUIContextRun()
        {
            Task.Run(() =>
            {
                Thread.Sleep(100);
                UIContext.Run(new Action(() =>
                {
                    Thread.Sleep(100);
                    throw new InvalidOperationException("Error on a background Task.");
                })).ObserveException();
            }).ObserveException();
        }
    }
}
