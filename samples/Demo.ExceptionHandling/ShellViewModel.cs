﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Caliburn.Light;

namespace Demo.ExceptionHandling
{
    public class ShellViewModel : BindableObject
    {
        private readonly Dispatcher _dispatcher;

        public ShellViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;

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
            Debug.Assert(_dispatcher.CheckAccess());
            throw new InvalidOperationException("Error on execute.");
        }

        private Task OnUIContextRun()
        {
            return Task.Run(() =>
            {
                Debug.Assert(!_dispatcher.CheckAccess());
                Thread.Sleep(100);

                return _dispatcher.InvokeAsync(new Action(() =>
                {
                    Debug.Assert(_dispatcher.CheckAccess());
                    Thread.Sleep(100);
                    throw new InvalidOperationException("Error on a background Task.");
                }));
            });
        }

        private Task OnTaskRun()
        {
            return Task.Run(() =>
            {
                Debug.Assert(!_dispatcher.CheckAccess());
                Thread.Sleep(100);
                throw new InvalidOperationException("Error on a background Task.");
            });
        }

        private async Task OnAsync()
        {
            Debug.Assert(_dispatcher.CheckAccess());
            await Task.Delay(100);
            Debug.Assert(_dispatcher.CheckAccess());
            throw new InvalidOperationException("Error on async execute.");
        }
    }
}
