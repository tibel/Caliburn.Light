using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Caliburn.Light;

internal interface IEventAggregatorHandlerEntry : IEventAggregatorHandler
{
    bool TryGetTarget([NotNullWhen(true)] out object? target);

    Task HandleAsync(object message, object target);
}
