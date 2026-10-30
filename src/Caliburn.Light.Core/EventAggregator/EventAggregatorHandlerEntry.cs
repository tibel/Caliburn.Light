using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Caliburn.Light;

internal sealed class EventAggregatorHandlerEntry<TTarget, TMessage> : IEventAggregatorHandlerEntry
    where TTarget : class
{
    private readonly WeakReference<TTarget> _weakTarget;
    private readonly Func<TTarget, TMessage, Task> _handler;

    public EventAggregatorHandlerEntry(TTarget target, Func<TTarget, TMessage, Task> handler, ThreadOption threadOption)
    {
        _weakTarget = new WeakReference<TTarget>(target);
        _handler = handler;
        ThreadOption = threadOption;
    }

    public ThreadOption ThreadOption { get; }

    public bool IsDead => !_weakTarget.TryGetTarget(out _);

    public bool CanHandle(object message) => message is TMessage;

    public bool TryGetTarget([NotNullWhen(true)] out object? target)
    {
        if (_weakTarget.TryGetTarget(out var typedTarget))
        {
            target = typedTarget;
            return true;
        }

        target = null;
        return false;
    }

    public Task HandleAsync(object message, object target) => _handler((TTarget)target, (TMessage)message);
}
