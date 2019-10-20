﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Caliburn.Light
{
    /// <summary>
    /// Used to gather the results from multiple child elements which may or may not prevent closing.
    /// </summary>
    /// <typeparam name="T">The type of child element.</typeparam>
    public sealed class DefaultCloseStrategy<T> : ICloseStrategy<T> where T : class
    {
        private readonly bool _closeConductedItemsWhenConductorCannotClose;

        /// <summary>
        /// Creates an instance of the class.
        /// </summary>
        /// <param name="closeConductedItemsWhenConductorCannotClose">Indicates that even if all conducted items are not closable, those that are should be closed.</param>
        public DefaultCloseStrategy(bool closeConductedItemsWhenConductorCannotClose = false)
        {
            _closeConductedItemsWhenConductorCannotClose = closeConductedItemsWhenConductorCannotClose;
        }

        /// <summary>
        /// Executes the strategy.
        /// </summary>
        /// <param name="toClose">Items that are requesting close.</param>
        /// <returns>A task containing the aggregated close results.
        /// The bool indicates whether close can occur. The enumerable indicates which children should close if the parent cannot.</returns>
        public async Task<CloseResult<T>> ExecuteAsync(IEnumerable<T> toClose)
        {
            var closables = new List<T>();
            var result = true;

            foreach (var item in toClose)
            {
                var guard = item as ICloseGuard;
                if (guard == null)
                {
                    closables.Add(item);
                }
                else
                {
                    var canClose = await guard.CanCloseAsync();
                    if (canClose)
                    {
                        closables.Add(item);
                    }

                    result = result && canClose;
                }
            }

            return new CloseResult<T>(result, _closeConductedItemsWhenConductorCannotClose ? closables : Enumerable.Empty<T>());
        }
    }
}
