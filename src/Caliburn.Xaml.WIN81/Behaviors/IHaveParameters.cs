
namespace Caliburn.Xaml
{
    /// <summary>
    /// Indicates that a <see cref="TriggerAction"/> is parametrized.
    /// </summary>
    public interface IHaveParameters
    {
        /// <summary>
        /// Represents the parameters of the action.
        /// </summary>
        AttachedCollection<Parameter> Parameters { get; }

        /// <summary>
        /// Forces an update of the IsEnabled state.
        /// </summary>
        void UpdateAvailability();
    }
}
