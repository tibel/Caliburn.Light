using System;
using System.Collections.Generic;

namespace Caliburn.Light
{
    /// <summary>
    /// Used to gather the results from multiple child elements which may or may not prevent closing.
    /// </summary>
    /// <typeparam name="T">The type of child element.</typeparam>
    public sealed class DefaultCloseStrategy<T> : ICloseStrategy<T>
    {
        private readonly bool _closeConductedItemsWhenConductorCannotClose;

        /// <summary>
        /// Creates an instance of the class.
        /// </summary>
        /// <param name="closeConductedItemsWhenConductorCannotClose">Indicates that even if all conducted items are not closable, those that are should be closed. The default is FALSE.</param>
        public DefaultCloseStrategy(bool closeConductedItemsWhenConductorCannotClose = false)
        {
            _closeConductedItemsWhenConductorCannotClose = closeConductedItemsWhenConductorCannotClose;
        }

        /// <summary>
        /// Executes the strategy.
        /// </summary>
        /// <param name="toClose">Items that are requesting close.</param>
        /// <param name="callback">The action to call when all enumeration is complete and the close results are aggregated.
        /// The bool indicates whether close can occur. The enumerable indicates which children should close if the parent cannot.</param>
        public void Execute(IEnumerable<T> toClose, Action<bool, IEnumerable<T>> callback)
        {
            Evaluate(new EvaluationState(toClose.GetEnumerator(), callback));
        }

        private void Evaluate(EvaluationState state)
        {
            var guardPending = false;
            do
            {
                if (!state.Enumerator.MoveNext())
                {
                    state.Enumerator.Dispose();
                    state.Callback(state.FinalResult, _closeConductedItemsWhenConductorCannotClose ? state.Closable : new List<T>());
                    break;
                }

                var current = state.Enumerator.Current;
                var guard = current as ICloseGuard;
                if (guard != null)
                {
                    guardPending = true;
                    guard.CanClose(canClose =>
                    {
                        guardPending = false;
                        if (canClose)
                        {
                            state.Closable.Add(current);
                        }

                        state.FinalResult = state.FinalResult && canClose;

                        if (state.GuardMustCallEvaluate)
                        {
                            state.GuardMustCallEvaluate = false;
                            Evaluate(state);
                        }
                    });
                    state.GuardMustCallEvaluate = state.GuardMustCallEvaluate || guardPending;
                }
                else
                {
                    state.Closable.Add(current);
                }
            } while (!guardPending);
        }

        private sealed class EvaluationState
        {
            public EvaluationState(IEnumerator<T> enumerator, Action<bool, IEnumerable<T>> callback)
            {
                Enumerator = enumerator;
                Callback = callback;
            }

            public readonly IEnumerator<T> Enumerator;
            public readonly Action<bool, IEnumerable<T>> Callback;

            public readonly List<T> Closable = new List<T>();
            public bool FinalResult = true;
            public bool GuardMustCallEvaluate;
        }
    }
}
