using System.ComponentModel;
using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

public class DelegateCommandTests
{
    [Test]
    public async Task Execute_InvokesAction()
    {
        var invoked = false;
        var command = new DelegateCommand(() => invoked = true);

        command.Execute(null);

        await Assert.That(invoked).IsTrue();
    }

    [Test]
    public async Task CanExecute_NoCanExecuteProvided_ReturnsTrue()
    {
        var command = new DelegateCommand(() => { });

        var result = command.CanExecute(null);

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task CanExecute_WithCanExecuteFunction_UsesFunction()
    {
        var canExecute = false;
        var command = new DelegateCommand(() => { }, () => canExecute);

        await Assert.That(command.CanExecute(null)).IsFalse();

        canExecute = true;
        await Assert.That(command.CanExecute(null)).IsTrue();
    }

    [Test]
    public async Task IsExecutable_NoCanExecuteProvided_ReturnsTrue()
    {
        var command = new DelegateCommand(() => { });

        await Assert.That(command.IsExecutable).IsTrue();
    }

    [Test]
    public async Task IsExecutable_CanExecuteReturnsFalse_ReturnsFalse()
    {
        var command = new DelegateCommand(() => { }, () => false);

        await Assert.That(command.IsExecutable).IsFalse();
    }

    [Test]
    public async Task IsExecutable_CachesResultAfterCanExecute()
    {
        var callCount = 0;
        var command = new DelegateCommand(() => { }, () => { callCount++; return true; });

        // First access triggers CanExecute
        _ = command.IsExecutable;
        var countAfterFirst = callCount;

        // Second access should use cached value
        _ = command.IsExecutable;
        var countAfterSecond = callCount;

        await Assert.That(countAfterFirst).IsEqualTo(1);
        await Assert.That(countAfterSecond).IsEqualTo(1);
    }

    [Test]
    public async Task RaiseCanExecuteChanged_RaisesCanExecuteChangedEvent()
    {
        var command = new DelegateCommand(() => { });
        var raised = false;
        command.CanExecuteChanged += (_, _) => raised = true;

        command.RaiseCanExecuteChanged();

        await Assert.That(raised).IsTrue();
    }

    [Test]
    public async Task RaiseCanExecuteChanged_RaisesPropertyChangedForIsExecutable()
    {
        var command = new DelegateCommand(() => { });
        var propertyNames = new List<string>();
        ((INotifyPropertyChanged)command).PropertyChanged += (_, e) => propertyNames.Add(e.PropertyName!);

        command.RaiseCanExecuteChanged();

        await Assert.That(propertyNames).Contains("IsExecutable");
    }

    [Test]
    public async Task RaiseCanExecuteChanged_InvalidatesIsExecutableCache()
    {
        var canExecute = false;
        var command = new DelegateCommand(() => { }, () => canExecute);

        // Trigger initial cache
        await Assert.That(command.IsExecutable).IsFalse();

        canExecute = true;
        command.RaiseCanExecuteChanged();

        await Assert.That(command.IsExecutable).IsTrue();
    }

    [Test]
    public async Task Constructor_NullExecute_ThrowsArgumentNullException()
    {
        await Assert.That(() => new DelegateCommand(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_WithObservation_NullExecute_ThrowsArgumentNullException()
    {
        var target = new ObservableStub();
        await Assert.That(() => new DelegateCommand(null!, () => true, target, "Name"))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_WithObservation_NullCanExecute_ThrowsArgumentNullException()
    {
        var target = new ObservableStub();
        await Assert.That(() => new DelegateCommand(() => { }, null!, target, "Name"))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_WithObservation_NullTarget_ThrowsArgumentNullException()
    {
        await Assert.That(() => new DelegateCommand(() => { }, () => true, null!, "Name"))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_WithObservation_NullPropertyNames_ThrowsArgumentNullException()
    {
        var target = new ObservableStub();
        await Assert.That(() => new DelegateCommand(() => { }, () => true, target, null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_WithObservation_EmptyPropertyNames_ThrowsArgumentNullException()
    {
        var target = new ObservableStub();
        await Assert.That(() => new DelegateCommand(() => { }, () => true, target))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task PropertyObservation_MatchingProperty_RaisesCanExecuteChanged()
    {
        var target = new ObservableStub();
        var command = new DelegateCommand(() => { }, () => true, target, "Name");
        var raised = false;
        command.CanExecuteChanged += (_, _) => raised = true;

        target.RaisePropertyChanged("Name");

        await Assert.That(raised).IsTrue();
    }

    [Test]
    public async Task PropertyObservation_NonMatchingProperty_DoesNotRaiseCanExecuteChanged()
    {
        var target = new ObservableStub();
        var command = new DelegateCommand(() => { }, () => true, target, "Name");
        var raised = false;
        command.CanExecuteChanged += (_, _) => raised = true;

        target.RaisePropertyChanged("Other");

        await Assert.That(raised).IsFalse();
    }

    [Test]
    public async Task PropertyObservation_EmptyPropertyName_RaisesCanExecuteChanged()
    {
        var target = new ObservableStub();
        var command = new DelegateCommand(() => { }, () => true, target, "Name");
        var raised = false;
        command.CanExecuteChanged += (_, _) => raised = true;

        target.RaisePropertyChanged(string.Empty);

        await Assert.That(raised).IsTrue();
    }

    [Test]
    public async Task PropertyObservation_NullPropertyName_RaisesCanExecuteChanged()
    {
        var target = new ObservableStub();
        var command = new DelegateCommand(() => { }, () => true, target, "Name");
        var raised = false;
        command.CanExecuteChanged += (_, _) => raised = true;

        target.RaisePropertyChanged(null);

        await Assert.That(raised).IsTrue();
    }
}

public class DelegateCommandOfTTests
{
    private static Func<object?, string?> StringCoercer => p => p as string;

    [Test]
    public async Task Execute_CoercesParameterAndInvokesAction()
    {
        string? receivedValue = null;
        var command = new DelegateCommand<string>(StringCoercer, v => receivedValue = v);

        command.Execute("hello");

        await Assert.That(receivedValue).IsEqualTo("hello");
    }

    [Test]
    public async Task Execute_CoercesNullParameter()
    {
        string? receivedValue = "initial";
        var command = new DelegateCommand<string>(StringCoercer, v => receivedValue = v);

        command.Execute(null);

        await Assert.That(receivedValue).IsNull();
    }

    [Test]
    public async Task CanExecute_NoCanExecuteProvided_ReturnsTrue()
    {
        var command = new DelegateCommand<string>(StringCoercer, _ => { });

        await Assert.That(command.CanExecute("test")).IsTrue();
    }

    [Test]
    public async Task CanExecute_WithCanExecuteFunction_CoercesAndUsesParameter()
    {
        var command = new DelegateCommand<string>(StringCoercer, _ => { }, v => v == "allow");

        await Assert.That(command.CanExecute("allow")).IsTrue();
        await Assert.That(command.CanExecute("deny")).IsFalse();
    }

    [Test]
    public async Task CanExecute_WithCanExecuteFunction_NullParameter()
    {
        var command = new DelegateCommand<string>(StringCoercer, _ => { }, v => v is null);

        await Assert.That(command.CanExecute(null)).IsTrue();
    }

    [Test]
    public async Task IsExecutable_NoCanExecute_ReturnsTrue()
    {
        var command = new DelegateCommand<string>(StringCoercer, _ => { });

        await Assert.That(command.IsExecutable).IsTrue();
    }

    [Test]
    public async Task Constructor_NullCoerceParameter_ThrowsArgumentNullException()
    {
        await Assert.That(() => new DelegateCommand<string>(null!, _ => { }))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_NullExecute_ThrowsArgumentNullException()
    {
        await Assert.That(() => new DelegateCommand<string>(StringCoercer, null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_WithObservation_NullCoerce_ThrowsArgumentNullException()
    {
        var target = new ObservableStub();
        await Assert.That(() => new DelegateCommand<string>(null!, _ => { }, _ => true, target, "Name"))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_WithObservation_NullExecute_ThrowsArgumentNullException()
    {
        var target = new ObservableStub();
        await Assert.That(() => new DelegateCommand<string>(StringCoercer, null!, _ => true, target, "Name"))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_WithObservation_NullCanExecute_ThrowsArgumentNullException()
    {
        var target = new ObservableStub();
        await Assert.That(() => new DelegateCommand<string>(StringCoercer, _ => { }, null!, target, "Name"))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_WithObservation_NullTarget_ThrowsArgumentNullException()
    {
        await Assert.That(() => new DelegateCommand<string>(StringCoercer, _ => { }, _ => true, null!, "Name"))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_WithObservation_EmptyPropertyNames_ThrowsArgumentNullException()
    {
        var target = new ObservableStub();
        await Assert.That(() => new DelegateCommand<string>(StringCoercer, _ => { }, _ => true, target))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task PropertyObservation_MatchingProperty_RaisesCanExecuteChanged()
    {
        var target = new ObservableStub();
        var command = new DelegateCommand<string>(StringCoercer, _ => { }, _ => true, target, "Name");
        var raised = false;
        command.CanExecuteChanged += (_, _) => raised = true;

        target.RaisePropertyChanged("Name");

        await Assert.That(raised).IsTrue();
    }

    [Test]
    public async Task PropertyObservation_NonMatchingProperty_DoesNotRaise()
    {
        var target = new ObservableStub();
        var command = new DelegateCommand<string>(StringCoercer, _ => { }, _ => true, target, "Name");
        var raised = false;
        command.CanExecuteChanged += (_, _) => raised = true;

        target.RaisePropertyChanged("Other");

        await Assert.That(raised).IsFalse();
    }
}

/// <summary>
/// Simple INotifyPropertyChanged implementation for testing property observation.
/// </summary>
public class ObservableStub : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public void RaisePropertyChanged(string? propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
