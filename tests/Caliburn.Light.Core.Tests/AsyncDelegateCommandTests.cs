using System.ComponentModel;
using Caliburn.Light;
using TUnit.Core;

namespace Caliburn.Light.Core.Tests;

[NotInParallel("StaticExecutingEvent")]
public class AsyncDelegateCommandTests
{
    [Test]
    public async Task Execute_InvokesExecuteAsync()
    {
        var invoked = false;
        var command = new AsyncDelegateCommand(() =>
        {
            invoked = true;
            return Task.CompletedTask;
        });

        command.Execute(null);

        await Assert.That(invoked).IsTrue();
    }

    [Test]
    public async Task CanExecute_NoCanExecuteProvided_ReturnsTrue()
    {
        var command = new AsyncDelegateCommand(() => Task.CompletedTask);

        await Assert.That(command.CanExecute(null)).IsTrue();
    }

    [Test]
    public async Task CanExecute_WithCanExecuteFunction_UsesFunction()
    {
        var canExecute = false;
        var command = new AsyncDelegateCommand(() => Task.CompletedTask, () => canExecute);

        await Assert.That(command.CanExecute(null)).IsFalse();

        canExecute = true;
        await Assert.That(command.CanExecute(null)).IsTrue();
    }

    [Test]
    public async Task IsExecuting_DuringExecution_ReturnsTrue()
    {
        var tcs = new TaskCompletionSource();
        var command = new AsyncDelegateCommand(() => tcs.Task);

        // Capture IsExecuting during execution via PropertyChanged
        var isExecutingValues = new List<bool>();
        ((INotifyPropertyChanged)command).PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == "IsExecuting")
                isExecutingValues.Add(command.IsExecuting);
        };

        command.Execute(null);

        // During execution (task not completed yet), IsExecuting should be true
        await Assert.That(command.IsExecuting).IsTrue();

        tcs.SetResult();
        await WaitForIsExecutingFalse(command);

        await Assert.That(command.IsExecuting).IsFalse();
    }

    [Test]
    public async Task CanExecute_WhileExecuting_ReturnsFalse()
    {
        var tcs = new TaskCompletionSource();
        var command = new AsyncDelegateCommand(() => tcs.Task);

        command.Execute(null);

        await Assert.That(command.CanExecute(null)).IsFalse();

        tcs.SetResult();
        await WaitForIsExecutingFalse(command);

        await Assert.That(command.CanExecute(null)).IsTrue();
    }

    [Test]
    public async Task CanExecuteChanged_RaisedOnExecutionStart()
    {
        var tcs = new TaskCompletionSource();
        var command = new AsyncDelegateCommand(() => tcs.Task);
        var raisedCount = 0;
        command.CanExecuteChanged += (_, _) => raisedCount++;

        command.Execute(null);

        // Should have been raised when execution started
        await Assert.That(raisedCount).IsEqualTo(1);

        tcs.SetResult();
        await WaitForIsExecutingFalse(command);

        // Should also be raised when execution ended
        await Assert.That(raisedCount).IsEqualTo(2);
    }

    [Test]
    public async Task PropertyChanged_IsExecuting_RaisedDuringLifecycle()
    {
        var tcs = new TaskCompletionSource();
        var command = new AsyncDelegateCommand(() => tcs.Task);
        var propertyNames = new List<string>();
        ((INotifyPropertyChanged)command).PropertyChanged += (_, e) => propertyNames.Add(e.PropertyName!);

        command.Execute(null);

        await Assert.That(propertyNames).Contains("IsExecuting");
        await Assert.That(propertyNames).Contains("IsExecutable");

        tcs.SetResult();
        await WaitForIsExecutingFalse(command);
    }

    [Test]
    public async Task ExecutingStaticEvent_FiredForNonCompletedTask()
    {
        var tcs = new TaskCompletionSource();
        Task? capturedTask = null;
        EventHandler<TaskEventArgs> handler = (_, e) => capturedTask = e.Task;

        AsyncCommand.Executing += handler;
        try
        {
            var command = new AsyncDelegateCommand(() => tcs.Task);
            command.Execute(null);

            // The Executing event should have been fired with the task
            await Assert.That(capturedTask is not null).IsTrue();

            tcs.SetResult();
            await WaitForIsExecutingFalse(command);
        }
        finally
        {
            AsyncCommand.Executing -= handler;
        }
    }

    [Test]
    public async Task ExecutingStaticEvent_NotFiredForCompletedTask()
    {
        var fired = false;
        EventHandler<TaskEventArgs> handler = (_, _) => fired = true;

        AsyncCommand.Executing += handler;
        try
        {
            var command = new AsyncDelegateCommand(() => Task.CompletedTask);
            command.Execute(null);

            await Assert.That(fired).IsFalse();
        }
        finally
        {
            AsyncCommand.Executing -= handler;
        }
    }

    [Test]
    public async Task Constructor_NullExecute_ThrowsArgumentNullException()
    {
        await Assert.That(() => new AsyncDelegateCommand(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_WithObservation_NullExecute_ThrowsArgumentNullException()
    {
        var target = new ObservableStub();
        await Assert.That(() => new AsyncDelegateCommand(null!, () => true, target, "Name"))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_WithObservation_NullCanExecute_ThrowsArgumentNullException()
    {
        var target = new ObservableStub();
        await Assert.That(() => new AsyncDelegateCommand(() => Task.CompletedTask, null!, target, "Name"))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_WithObservation_NullTarget_ThrowsArgumentNullException()
    {
        await Assert.That(() => new AsyncDelegateCommand(() => Task.CompletedTask, () => true, null!, "Name"))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_WithObservation_EmptyPropertyNames_ThrowsArgumentNullException()
    {
        var target = new ObservableStub();
        await Assert.That(() => new AsyncDelegateCommand(() => Task.CompletedTask, () => true, target))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task IsExecutable_WhileExecuting_ReturnsFalse()
    {
        var tcs = new TaskCompletionSource();
        var command = new AsyncDelegateCommand(() => tcs.Task);

        command.Execute(null);

        await Assert.That(command.IsExecutable).IsFalse();

        tcs.SetResult();
        await WaitForIsExecutingFalse(command);

        await Assert.That(command.IsExecutable).IsTrue();
    }

    [Test]
    public async Task RaiseCanExecuteChanged_RaisesEvent()
    {
        var command = new AsyncDelegateCommand(() => Task.CompletedTask);
        var raised = false;
        command.CanExecuteChanged += (_, _) => raised = true;

        command.RaiseCanExecuteChanged();

        await Assert.That(raised).IsTrue();
    }

    [Test]
    public async Task PropertyObservation_MatchingProperty_RaisesCanExecuteChanged()
    {
        var target = new ObservableStub();
        var command = new AsyncDelegateCommand(() => Task.CompletedTask, () => true, target, "Name");
        var raised = false;
        command.CanExecuteChanged += (_, _) => raised = true;

        target.RaisePropertyChanged("Name");

        await Assert.That(raised).IsTrue();
    }

    [Test]
    public async Task PropertyObservation_NonMatchingProperty_DoesNotRaise()
    {
        var target = new ObservableStub();
        var command = new AsyncDelegateCommand(() => Task.CompletedTask, () => true, target, "Name");
        var raised = false;
        command.CanExecuteChanged += (_, _) => raised = true;

        target.RaisePropertyChanged("Other");

        await Assert.That(raised).IsFalse();
    }

    [Test]
    public async Task CanExecute_WithCanExecuteAndIsExecuting_BothChecked()
    {
        var tcs = new TaskCompletionSource();
        var canExec = true;
        var command = new AsyncDelegateCommand(() => tcs.Task, () => canExec);

        // canExecute true + not executing = true
        await Assert.That(command.CanExecute(null)).IsTrue();

        // canExecute false + not executing = false
        canExec = false;
        await Assert.That(command.CanExecute(null)).IsFalse();

        // canExecute true + executing = false
        canExec = true;
        command.Execute(null);
        await Assert.That(command.CanExecute(null)).IsFalse();

        tcs.SetResult();
        await WaitForIsExecutingFalse(command);
    }

    private static async Task WaitForIsExecutingFalse(AsyncCommand command, int timeoutMs = 5000)
    {
        if (!command.IsExecuting) return;
        var tcs = new TaskCompletionSource();
        ((INotifyPropertyChanged)command).PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == "IsExecuting" && !command.IsExecuting)
                tcs.TrySetResult();
        };
        if (!command.IsExecuting) { tcs.TrySetResult(); }
        await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs));
    }
}

public class AsyncDelegateCommandOfTTests
{
    private static Func<object?, int> IntCoercer => p => p is int i ? i : 0;

    [Test]
    public async Task Execute_CoercesParameterAndInvokesAction()
    {
        int? receivedValue = null;
        var command = new AsyncDelegateCommand<int>(IntCoercer, v =>
        {
            receivedValue = v;
            return Task.CompletedTask;
        });

        command.Execute(42);

        await Assert.That(receivedValue).IsEqualTo(42);
    }

    [Test]
    public async Task Execute_CoercesNullParameterToDefault()
    {
        int? receivedValue = null;
        var command = new AsyncDelegateCommand<int>(IntCoercer, v =>
        {
            receivedValue = v;
            return Task.CompletedTask;
        });

        command.Execute(null);

        await Assert.That(receivedValue).IsEqualTo(0);
    }

    [Test]
    public async Task CanExecute_NoCanExecuteProvided_ReturnsTrue()
    {
        var command = new AsyncDelegateCommand<int>(IntCoercer, _ => Task.CompletedTask);

        await Assert.That(command.CanExecute(42)).IsTrue();
    }

    [Test]
    public async Task CanExecute_WithCanExecuteFunction_CoercesAndUsesParameter()
    {
        var command = new AsyncDelegateCommand<int>(IntCoercer, _ => Task.CompletedTask, v => v > 0);

        await Assert.That(command.CanExecute(5)).IsTrue();
        await Assert.That(command.CanExecute(0)).IsFalse();
    }

    [Test]
    public async Task IsExecuting_DuringExecution_ReturnsTrue()
    {
        var tcs = new TaskCompletionSource();
        var command = new AsyncDelegateCommand<int>(IntCoercer, _ => tcs.Task);

        command.Execute(1);

        await Assert.That(command.IsExecuting).IsTrue();

        tcs.SetResult();
        await WaitForIsExecutingFalse(command);

        await Assert.That(command.IsExecuting).IsFalse();
    }

    [Test]
    public async Task CanExecute_WhileExecuting_ReturnsFalse()
    {
        var tcs = new TaskCompletionSource();
        var command = new AsyncDelegateCommand<int>(IntCoercer, _ => tcs.Task);

        command.Execute(1);

        await Assert.That(command.CanExecute(1)).IsFalse();

        tcs.SetResult();
        await WaitForIsExecutingFalse(command);
    }

    [Test]
    public async Task Constructor_NullCoerceParameter_ThrowsArgumentNullException()
    {
        await Assert.That(() => new AsyncDelegateCommand<int>(null!, _ => Task.CompletedTask))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_NullExecute_ThrowsArgumentNullException()
    {
        await Assert.That(() => new AsyncDelegateCommand<int>(IntCoercer, null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_WithObservation_NullCoerce_ThrowsArgumentNullException()
    {
        var target = new ObservableStub();
        await Assert.That(() => new AsyncDelegateCommand<int>(null!, _ => Task.CompletedTask, _ => true, target, "Name"))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_WithObservation_NullExecute_ThrowsArgumentNullException()
    {
        var target = new ObservableStub();
        await Assert.That(() => new AsyncDelegateCommand<int>(IntCoercer, null!, _ => true, target, "Name"))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_WithObservation_NullCanExecute_ThrowsArgumentNullException()
    {
        var target = new ObservableStub();
        await Assert.That(() => new AsyncDelegateCommand<int>(IntCoercer, _ => Task.CompletedTask, null!, target, "Name"))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_WithObservation_NullTarget_ThrowsArgumentNullException()
    {
        await Assert.That(() => new AsyncDelegateCommand<int>(IntCoercer, _ => Task.CompletedTask, _ => true, null!, "Name"))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_WithObservation_EmptyPropertyNames_ThrowsArgumentNullException()
    {
        var target = new ObservableStub();
        await Assert.That(() => new AsyncDelegateCommand<int>(IntCoercer, _ => Task.CompletedTask, _ => true, target))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task PropertyObservation_MatchingProperty_RaisesCanExecuteChanged()
    {
        var target = new ObservableStub();
        var command = new AsyncDelegateCommand<int>(IntCoercer, _ => Task.CompletedTask, _ => true, target, "Name");
        var raised = false;
        command.CanExecuteChanged += (_, _) => raised = true;

        target.RaisePropertyChanged("Name");

        await Assert.That(raised).IsTrue();
    }

    [Test]
    public async Task PropertyObservation_NonMatchingProperty_DoesNotRaise()
    {
        var target = new ObservableStub();
        var command = new AsyncDelegateCommand<int>(IntCoercer, _ => Task.CompletedTask, _ => true, target, "Name");
        var raised = false;
        command.CanExecuteChanged += (_, _) => raised = true;

        target.RaisePropertyChanged("Other");

        await Assert.That(raised).IsFalse();
    }

    private static async Task WaitForIsExecutingFalse(AsyncCommand command, int timeoutMs = 5000)
    {
        if (!command.IsExecuting) return;
        var tcs = new TaskCompletionSource();
        ((INotifyPropertyChanged)command).PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == "IsExecuting" && !command.IsExecuting)
                tcs.TrySetResult();
        };
        if (!command.IsExecuting) { tcs.TrySetResult(); }
        await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs));
    }
}
