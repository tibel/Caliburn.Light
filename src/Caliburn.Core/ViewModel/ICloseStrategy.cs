using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Caliburn.Light
{
    /// <summary>
    /// Used to gather the results from multiple child elements which may or may not prevent closing.
    /// </summary>
    /// <typeparam name="T">The type of child element.</typeparam>
    public interface ICloseStrategy<T> where T : class
    {
        /// <summary>
        /// Executes the strategy.
        /// </summary>
        /// <param name="toClose">Items that are requesting close.</param>
        /// <returns>A task containing the aggregated close results.
        /// The bool indicates whether close can occur. The enumerable indicates which children should close if the parent cannot.</returns>
        Task<Tuple<bool, IEnumerable<T>>> ExecuteAsync(IEnumerable<T> toClose);
    }
}
