﻿using System.Collections.Generic;

namespace Caliburn.Light
{
    /// <summary>
    /// The context used during the execution of a coroutine.
    /// </summary>
    public sealed class CoroutineExecutionContext
    {
        private const string SourceKey = "source";
        private const string TargetKey = "target";
        private const string EventArgsKey = "eventargs";

        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets additional data needed to invoke the coroutine.
        /// </summary>
        /// <param name="key">The data key.</param>
        /// <returns>Custom data associated with the context.</returns>
        public object this[string key]
        {
            get
            {
                object result;
                _values.TryGetValue(key, out result);
                return result;
            }
            set { _values[key] = value; }
        }

        /// <summary>
        /// The source from which the coroutine originates.
        /// </summary>
        public object Source
        {
            get { return this[SourceKey]; }
            set { this[SourceKey] = value; }
        }

        /// <summary>
        /// The instance on which the coroutine is invoked.
        /// </summary>
        public object Target
        {
            get { return this[TargetKey]; }
            set { this[TargetKey] = value; }
        }

        /// <summary>
        /// Any event arguments associated with the coroutine invocation.
        /// </summary>
        public object EventArgs
        {
            get { return this[EventArgsKey]; }
            set { this[EventArgsKey] = value; }
        }
    }
}
