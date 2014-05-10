#if WINDOWS_APP || WINDOWS_PHONE_APP
[assembly: System.Runtime.CompilerServices.TypeForwardedTo(typeof(Microsoft.Xaml.Interactivity.IAction))]
#else
// ReSharper disable once CheckNamespace
namespace Microsoft.Xaml.Interactivity
{
    /// <summary>
    /// Interface implemented by all custom actions.
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="sender">The Object that is passed to the action by the behavior. Generally this is AssociatedObject or a target object.</param>
        /// <param name="parameter">The value of this parameter is determined by the caller.</param>
        /// <returns>The result of the action.</returns>
        object Execute(object sender, object parameter);
    }
}
#endif
