using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

public class DelegateCommandBuilderTests
{
    // ===== NoParameter sync =====

    [Test]
    public async Task NoParameter_OnExecute_Build_CreatesDelegateCommand()
    {
        var command = DelegateCommandBuilder.NoParameter()
            .OnExecute(() => { })
            .Build();

        await Assert.That(command).IsTypeOf<DelegateCommand>();
    }

    [Test]
    public async Task NoParameter_Sync_ExecuteInvokesAction()
    {
        var executed = false;
        var command = DelegateCommandBuilder.NoParameter()
            .OnExecute(() => executed = true)
            .Build();

        command.Execute(null);

        await Assert.That(executed).IsTrue();
    }

    [Test]
    public async Task NoParameter_Sync_CanExecuteDefaultsToTrue()
    {
        var command = DelegateCommandBuilder.NoParameter()
            .OnExecute(() => { })
            .Build();

        await Assert.That(command.CanExecute(null)).IsTrue();
    }

    [Test]
    public async Task NoParameter_Sync_OnCanExecute_ReturnsFalse()
    {
        var command = DelegateCommandBuilder.NoParameter()
            .OnExecute(() => { })
            .OnCanExecute(() => false)
            .Build();

        await Assert.That(command.CanExecute(null)).IsFalse();
    }

    [Test]
    public async Task NoParameter_Sync_OnCanExecute_ReturnsTrue()
    {
        var command = DelegateCommandBuilder.NoParameter()
            .OnExecute(() => { })
            .OnCanExecute(() => true)
            .Build();

        await Assert.That(command.CanExecute(null)).IsTrue();
    }

    [Test]
    public async Task NoParameter_Sync_Observe_BuildsCommand()
    {
        var target = new TestNotifyPropertyChanged();
        var command = DelegateCommandBuilder.NoParameter()
            .OnExecute(() => { })
            .OnCanExecute(() => true)
            .Observe(target, "TestProp")
            .Build();

        await Assert.That(command).IsTypeOf<DelegateCommand>();
    }

    // ===== NoParameter async =====

    [Test]
    public async Task NoParameter_OnExecuteAsync_Build_CreatesAsyncDelegateCommand()
    {
        var command = DelegateCommandBuilder.NoParameter()
            .OnExecute(() => Task.CompletedTask)
            .Build();

        await Assert.That(command).IsTypeOf<AsyncDelegateCommand>();
    }

    [Test]
    public async Task NoParameter_Async_CanExecuteDefaultsToTrue()
    {
        var command = DelegateCommandBuilder.NoParameter()
            .OnExecute(() => Task.CompletedTask)
            .Build();

        await Assert.That(command.CanExecute(null)).IsTrue();
    }

    [Test]
    public async Task NoParameter_Async_OnCanExecute_ReturnsFalse()
    {
        var command = DelegateCommandBuilder.NoParameter()
            .OnExecute(() => Task.CompletedTask)
            .OnCanExecute(() => false)
            .Build();

        await Assert.That(command.CanExecute(null)).IsFalse();
    }

    [Test]
    public async Task NoParameter_Async_Observe_BuildsCommand()
    {
        var target = new TestNotifyPropertyChanged();
        var command = DelegateCommandBuilder.NoParameter()
            .OnExecute(() => Task.CompletedTask)
            .OnCanExecute(() => true)
            .Observe(target, "TestProp")
            .Build();

        await Assert.That(command).IsTypeOf<AsyncDelegateCommand>();
    }

    // ===== WithParameter sync =====

    [Test]
    public async Task WithParameter_OnExecute_Build_CreatesDelegateCommandOfT()
    {
        var command = DelegateCommandBuilder.WithParameter<int>()
            .OnExecute(_ => { })
            .Build();

        await Assert.That(command).IsTypeOf<DelegateCommand<int>>();
    }

    [Test]
    public async Task WithParameter_Sync_ExecuteReceivesCoercedParameter()
    {
        int? received = null;
        var command = DelegateCommandBuilder.WithParameter<int>()
            .OnExecute(p => received = p)
            .Build();

        command.Execute(42);

        await Assert.That(received).IsEqualTo(42);
    }

    [Test]
    public async Task WithParameter_Sync_CustomCoerce_IsUsed()
    {
        int? received = null;
        var command = DelegateCommandBuilder.WithParameter<int>(p => p is int i ? i * 2 : 0)
            .OnExecute(p => received = p)
            .Build();

        command.Execute(5);

        await Assert.That(received).IsEqualTo(10);
    }

    [Test]
    public async Task WithParameter_Sync_CanExecuteDefaultsToTrue()
    {
        var command = DelegateCommandBuilder.WithParameter<int>()
            .OnExecute(_ => { })
            .Build();

        await Assert.That(command.CanExecute(0)).IsTrue();
    }

    [Test]
    public async Task WithParameter_Sync_OnCanExecute_EvaluatesParameter()
    {
        var command = DelegateCommandBuilder.WithParameter<int>()
            .OnExecute(_ => { })
            .OnCanExecute(p => p > 0)
            .Build();

        await Assert.That(command.CanExecute(1)).IsTrue();
        await Assert.That(command.CanExecute(0)).IsFalse();
    }

    [Test]
    public async Task WithParameter_Sync_Observe_BuildsCommand()
    {
        var target = new TestNotifyPropertyChanged();
        var command = DelegateCommandBuilder.WithParameter<int>()
            .OnExecute(_ => { })
            .OnCanExecute(_ => true)
            .Observe(target, "TestProp")
            .Build();

        await Assert.That(command).IsTypeOf<DelegateCommand<int>>();
    }

    // ===== WithParameter async =====

    [Test]
    public async Task WithParameter_OnExecuteAsync_Build_CreatesAsyncDelegateCommandOfT()
    {
        var command = DelegateCommandBuilder.WithParameter<string>()
            .OnExecute(_ => Task.CompletedTask)
            .Build();

        await Assert.That(command).IsTypeOf<AsyncDelegateCommand<string>>();
    }

    [Test]
    public async Task WithParameter_Async_CanExecuteDefaultsToTrue()
    {
        var command = DelegateCommandBuilder.WithParameter<string>()
            .OnExecute(_ => Task.CompletedTask)
            .Build();

        await Assert.That(command.CanExecute(null)).IsTrue();
    }

    [Test]
    public async Task WithParameter_Async_OnCanExecute_ReturnsFalse()
    {
        var command = DelegateCommandBuilder.WithParameter<string>()
            .OnExecute(_ => Task.CompletedTask)
            .OnCanExecute(_ => false)
            .Build();

        await Assert.That(command.CanExecute("x")).IsFalse();
    }

    [Test]
    public async Task WithParameter_Async_Observe_BuildsCommand()
    {
        var target = new TestNotifyPropertyChanged();
        var command = DelegateCommandBuilder.WithParameter<string>()
            .OnExecute(_ => Task.CompletedTask)
            .OnCanExecute(_ => true)
            .Observe(target, "TestProp")
            .Build();

        await Assert.That(command).IsTypeOf<AsyncDelegateCommand<string>>();
    }

    // ===== Null argument validation =====

    [Test]
    public async Task NoParameter_Sync_OnExecuteNull_Throws()
    {
        await Assert.That(() => DelegateCommandBuilder.NoParameter().OnExecute((Action)null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task NoParameter_Async_OnExecuteNull_Throws()
    {
        await Assert.That(() => DelegateCommandBuilder.NoParameter().OnExecute((Func<Task>)null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task NoParameter_Sync_OnCanExecuteNull_Throws()
    {
        var builder = DelegateCommandBuilder.NoParameter().OnExecute(() => { });

        await Assert.That(() => builder.OnCanExecute(null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task NoParameter_Async_OnCanExecuteNull_Throws()
    {
        var builder = DelegateCommandBuilder.NoParameter().OnExecute(() => Task.CompletedTask);

        await Assert.That(() => builder.OnCanExecute(null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task NoParameter_Sync_ObserveNullTarget_Throws()
    {
        var builder = DelegateCommandBuilder.NoParameter().OnExecute(() => { });

        await Assert.That(() => builder.Observe(null!, "Prop"))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task NoParameter_Sync_ObserveEmptyPropertyNames_Throws()
    {
        var target = new TestNotifyPropertyChanged();
        var builder = DelegateCommandBuilder.NoParameter().OnExecute(() => { });

        await Assert.That(() => builder.Observe(target))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task NoParameter_Async_ObserveNullTarget_Throws()
    {
        var builder = DelegateCommandBuilder.NoParameter().OnExecute(() => Task.CompletedTask);

        await Assert.That(() => builder.Observe(null!, "Prop"))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task NoParameter_Async_ObserveEmptyPropertyNames_Throws()
    {
        var target = new TestNotifyPropertyChanged();
        var builder = DelegateCommandBuilder.NoParameter().OnExecute(() => Task.CompletedTask);

        await Assert.That(() => builder.Observe(target))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task WithParameter_Sync_OnExecuteNull_Throws()
    {
        var builder = DelegateCommandBuilder.WithParameter<string>();
        await Assert.That(() => builder.OnExecute((Action<string?>)null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task WithParameter_Async_OnExecuteNull_Throws()
    {
        var builder = DelegateCommandBuilder.WithParameter<string>();
        await Assert.That(() => builder.OnExecute((Func<string?, Task>)null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task WithParameter_Sync_OnCanExecuteNull_Throws()
    {
        var builder = DelegateCommandBuilder.WithParameter<int>().OnExecute(_ => { });

        await Assert.That(() => builder.OnCanExecute(null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task WithParameter_Async_OnCanExecuteNull_Throws()
    {
        var builder = DelegateCommandBuilder.WithParameter<int>().OnExecute(_ => Task.CompletedTask);

        await Assert.That(() => builder.OnCanExecute(null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task WithParameter_Sync_ObserveNullTarget_Throws()
    {
        var builder = DelegateCommandBuilder.WithParameter<int>().OnExecute(_ => { });

        await Assert.That(() => builder.Observe(null!, "Prop"))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task WithParameter_Sync_ObserveEmptyPropertyNames_Throws()
    {
        var target = new TestNotifyPropertyChanged();
        var builder = DelegateCommandBuilder.WithParameter<int>().OnExecute(_ => { });

        await Assert.That(() => builder.Observe(target))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task WithParameter_Async_ObserveNullTarget_Throws()
    {
        var builder = DelegateCommandBuilder.WithParameter<int>().OnExecute(_ => Task.CompletedTask);

        await Assert.That(() => builder.Observe(null!, "Prop"))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task WithParameter_Async_ObserveEmptyPropertyNames_Throws()
    {
        var target = new TestNotifyPropertyChanged();
        var builder = DelegateCommandBuilder.WithParameter<int>().OnExecute(_ => Task.CompletedTask);

        await Assert.That(() => builder.Observe(target))
            .Throws<ArgumentNullException>();
    }

    // ===== Helper =====

    private sealed class TestNotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void Raise(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
