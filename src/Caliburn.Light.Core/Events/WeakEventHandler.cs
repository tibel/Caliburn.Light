using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;

namespace Caliburn.Light;

/// <summary>
/// Helper to register weak event handlers.
/// </summary>
public static class WeakEventHandler
{
    /// <summary>
    /// Registers a weak handler to <see cref="INotifyPropertyChanging.PropertyChanging"/>.
    /// </summary>
    /// <typeparam name="TSubscriber">The type of the event subscriber.</typeparam>
    /// <param name="source">The event source.</param>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="weakHandler">The weak handler.</param>
    /// <returns>A registration object that can be used to deregister from the event.</returns>
    public static IDisposable RegisterPropertyChangingWeak<TSubscriber>(this INotifyPropertyChanging source,
        TSubscriber subscriber, Action<TSubscriber, object?, PropertyChangingEventArgs> weakHandler)
        where TSubscriber : class
    {
        return new WeakNotifyPropertyChangingHandler<TSubscriber>(source, subscriber, weakHandler);
    }

    /// <summary>
    /// Registers a weak handler to <see cref="INotifyPropertyChanged.PropertyChanged"/>.
    /// </summary>
    /// <typeparam name="TSubscriber">The type of the event subscriber.</typeparam>
    /// <param name="source">The event source.</param>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="weakHandler">The weak handler.</param>
    /// <returns>A registration object that can be used to deregister from the event.</returns>
    public static IDisposable RegisterPropertyChangedWeak<TSubscriber>(this INotifyPropertyChanged source,
        TSubscriber subscriber, Action<TSubscriber, object?, PropertyChangedEventArgs> weakHandler)
        where TSubscriber : class
    {
        return new WeakNotifyPropertyChangedHandler<TSubscriber>(source, subscriber, weakHandler);
    }

    /// <summary>
    /// Registers a weak handler to <see cref="INotifyCollectionChanged.CollectionChanged"/>.
    /// </summary>
    /// <typeparam name="TSubscriber">The type of the event subscriber.</typeparam>
    /// <param name="source">The event source.</param>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="weakHandler">The weak handler.</param>
    /// <returns>A registration object that can be used to deregister from the event.</returns>
    public static IDisposable RegisterCollectionChangedWeak<TSubscriber>(this INotifyCollectionChanged source,
        TSubscriber subscriber, Action<TSubscriber, object?, NotifyCollectionChangedEventArgs> weakHandler)
        where TSubscriber : class
    {
        return new WeakNotifyCollectionChangedHandler<TSubscriber>(source, subscriber, weakHandler);
    }

    /// <summary>
    /// Registers a weak event handler to <see cref="ICommand.CanExecuteChanged"/>.
    /// </summary>
    /// <typeparam name="TSubscriber">The type of the event subscriber.</typeparam>
    /// <param name="source">The event source.</param>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="weakHandler">The weak handler.</param>
    /// <returns>A registration object that can be used to deregister from the event.</returns>
    public static IDisposable RegisterCanExecuteChangedWeak<TSubscriber>(this ICommand source,
        TSubscriber subscriber, Action<TSubscriber, object?, EventArgs> weakHandler)
        where TSubscriber : class
    {
        return new WeakCanExecuteChangedHandler<TSubscriber>(source, subscriber, weakHandler);
    }

    /// <summary>
    /// Registers a weak event handler to <see cref="IActivatable.Activated"/>.
    /// </summary>
    /// <typeparam name="TSubscriber">The type of the event subscriber.</typeparam>
    /// <param name="source">The event source.</param>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="weakHandler">The weak handler.</param>
    /// <returns>A registration object that can be used to deregister from the event.</returns>
    public static IDisposable RegisterActivatedWeak<TSubscriber>(this IActivatable source,
        TSubscriber subscriber, Action<TSubscriber, object?, ActivationEventArgs> weakHandler)
        where TSubscriber : class
    {
        return new WeakActivatedHandler<TSubscriber>(source, subscriber, weakHandler);
    }

    /// <summary>
    /// Registers a weak event handler to <see cref="IActivatable.Deactivating"/>.
    /// </summary>
    /// <typeparam name="TSubscriber">The type of the event subscriber.</typeparam>
    /// <param name="source">The event source.</param>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="weakHandler">The weak handler.</param>
    /// <returns>A registration object that can be used to deregister from the event.</returns>
    public static IDisposable RegisterDeactivatingWeak<TSubscriber>(this IActivatable source,
        TSubscriber subscriber, Action<TSubscriber, object?, DeactivationEventArgs> weakHandler)
        where TSubscriber : class
    {
        return new WeakDeactivatingHandler<TSubscriber>(source, subscriber, weakHandler);
    }

    /// <summary>
    /// Registers a weak event handler to <see cref="IActivatable.Deactivated"/>.
    /// </summary>
    /// <typeparam name="TSubscriber">The type of the event subscriber.</typeparam>
    /// <param name="source">The event source.</param>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="weakHandler">The weak handler.</param>
    /// <returns>A registration object that can be used to deregister from the event.</returns>
    public static IDisposable RegisterDeactivatedWeak<TSubscriber>(this IActivatable source,
        TSubscriber subscriber, Action<TSubscriber, object?, DeactivationEventArgs> weakHandler)
        where TSubscriber : class
    {
        return new WeakDeactivatedHandler<TSubscriber>(source, subscriber, weakHandler);
    }

    /// <summary>
    /// Registers a weak handler to the static <see cref="AsyncCommand.Executing"/> event.
    /// </summary>
    /// <remarks>
    /// The event is static, so this is a static method rather than an extension method. Dispose the
    /// returned registration for deterministic cleanup; otherwise the handler detaches itself the next
    /// time the event is raised after <paramref name="subscriber"/> has been collected.
    /// </remarks>
    /// <typeparam name="TSubscriber">The type of the event subscriber.</typeparam>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="weakHandler">The weak handler.</param>
    /// <returns>A registration object that can be used to deregister from the event.</returns>
    public static IDisposable RegisterAsyncCommandExecutingWeak<TSubscriber>(TSubscriber subscriber,
        Action<TSubscriber, object?, TaskEventArgs> weakHandler)
        where TSubscriber : class
    {
        return new WeakAsyncCommandExecutingHandler<TSubscriber>(subscriber, weakHandler);
    }

    /// <summary>
    /// Registers a weak handler to the static <see cref="EventAggregator.Executing"/> event.
    /// </summary>
    /// <remarks>
    /// The event is static, so this is a static method rather than an extension method. Dispose the
    /// returned registration for deterministic cleanup; otherwise the handler detaches itself the next
    /// time the event is raised after <paramref name="subscriber"/> has been collected.
    /// </remarks>
    /// <typeparam name="TSubscriber">The type of the event subscriber.</typeparam>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="weakHandler">The weak handler.</param>
    /// <returns>A registration object that can be used to deregister from the event.</returns>
    public static IDisposable RegisterEventAggregatorExecutingWeak<TSubscriber>(TSubscriber subscriber,
        Action<TSubscriber, object?, TaskEventArgs> weakHandler)
        where TSubscriber : class
    {
        return new WeakEventAggregatorExecutingHandler<TSubscriber>(subscriber, weakHandler);
    }
}
