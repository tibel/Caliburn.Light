using System.Collections.Generic;

namespace Caliburn.Light
{
    /// <summary>
    /// Denotes a class which wants to preserve state accross session lifetime.
    /// </summary>
    public interface IPreserveState
    {
        /// <summary>
        /// Restores previously saved state.
        /// </summary>
        /// <param name="state">A collection of state items to restore.</param>
        void RestoreState(IReadOnlyDictionary<string, object> state);

        /// <summary>
        /// Saves the current state.
        /// </summary>
        /// <param name="state">A collection where to put state items.</param>
        void SaveState(IDictionary<string, object> state);
    }
}
