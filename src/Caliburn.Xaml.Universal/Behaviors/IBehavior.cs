#if WINDOWS_APP || WINDOWS_PHONE_APP
[assembly: System.Runtime.CompilerServices.TypeForwardedTo(typeof(Microsoft.Xaml.Interactivity.IBehavior))]
#else
// ReSharper disable once CheckNamespace
namespace Microsoft.Xaml.Interactivity
{
    using Windows.UI.Xaml;

    /// <summary>
    /// Interface implemented by all custom behaviors.
    /// </summary>
    public interface IBehavior
    {
        /// <summary>
        /// Gets the <see cref="DependencyObject" /> to which the <see cref="IBehavior" /> is attached.
        /// </summary>
        DependencyObject AssociatedObject { get; }

        /// <summary>
        /// Attaches to the specified object.
        /// </summary>
        /// <param name="associatedObject">The <see cref="DependencyObject" /> to which the <see cref="IBehavior" /> is attached.</param>
        void Attach(DependencyObject associatedObject);

        /// <summary>
        /// Detaches this instance from its associated object.
        /// </summary>
        void Detach();
    }
}
#endif
