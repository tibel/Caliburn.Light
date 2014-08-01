using System.Windows;

namespace Caliburn.Light
{
    /// <summary>
    /// Represents a collection of <see cref="T:Parameter"/> instances.
    /// </summary>
    public sealed class ParameterCollection : FreezableCollection<Parameter>
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <returns>The new instance.</returns>
        protected override Freezable CreateInstanceCore()
        {
            return new ParameterCollection();
        }
    }
}
