using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Caliburn.Light;

/// <summary>
/// Event arguments that allow handlers to defer completion of an asynchronous event invocation.
/// </summary>
/// <remarks>
/// An instance can only participate in one invocation at a time. It can be reused after the invocation and all
/// outstanding deferrals have completed.
/// </remarks>
public class DeferredEventArgs : EventArgs
{
    private readonly Lock _lockObject = new();
    private CancellationToken _cancellationToken;
    private TaskCompletionSource? _completion;
    private bool _invocationActive;
    private bool _acceptingDeferrals;
    private int _deferralCount;

    /// <summary>
    /// Gets a deferral that must be completed before the event invocation completes.
    /// </summary>
    /// <returns>The deferral.</returns>
    /// <exception cref="InvalidOperationException">The event is not currently being invoked.</exception>
    public EventDeferral GetDeferral()
    {
        lock (_lockObject)
        {
            if (!_invocationActive || !_acceptingDeferrals)
                ThrowDeferralsNotAvailable();

            _deferralCount++;
            return new EventDeferral(this, _cancellationToken);
        }
    }

    internal void BeginInvocation(CancellationToken cancellationToken)
    {
        lock (_lockObject)
        {
            if (_invocationActive)
                ThrowInvocationAlreadyActive();

            _invocationActive = true;
            _acceptingDeferrals = true;
            _cancellationToken = cancellationToken;
        }
    }

    internal Task EndInvocation()
    {
        lock (_lockObject)
        {
            if (!_invocationActive || !_acceptingDeferrals)
                ThrowNoActiveInvocation();

            _acceptingDeferrals = false;
            if (_deferralCount == 0)
            {
                ResetInvocation();
                return Task.CompletedTask;
            }

            _completion ??= new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            return _completion.Task;
        }
    }

    internal void CompleteDeferral()
    {
        lock (_lockObject)
        {
            if (!_invocationActive || _deferralCount == 0)
                ThrowDeferralNotAssociatedWithActiveInvocation();

            _deferralCount--;
            if (_deferralCount == 0 && !_acceptingDeferrals)
            {
                var completion = _completion;
                if (completion is null)
                    ThrowMissingCompletionSource();

                ResetInvocation();
                completion.TrySetResult();
            }
        }
    }

    private void ResetInvocation()
    {
        _invocationActive = false;
        _acceptingDeferrals = false;
        _cancellationToken = default;
        _completion = null;
        _deferralCount = 0;
    }

    [DoesNotReturn]
    private static void ThrowDeferralsNotAvailable()
        => throw new InvalidOperationException("Deferrals can only be requested while the event is being invoked.");

    [DoesNotReturn]
    private static void ThrowInvocationAlreadyActive()
        => throw new InvalidOperationException("The event arguments are already in use by an active or deferred invocation.");

    [DoesNotReturn]
    private static void ThrowNoActiveInvocation()
        => throw new InvalidOperationException("There is no active event invocation.");

    [DoesNotReturn]
    private static void ThrowDeferralNotAssociatedWithActiveInvocation()
        => throw new InvalidOperationException("The deferral is not associated with an active event invocation.");

    [DoesNotReturn]
    private static void ThrowMissingCompletionSource()
        => throw new InvalidOperationException("The active invocation has no completion source.");
}
