using System;
using System.Collections.Generic;
using System.Linq;

namespace Caliburn.Light
{
    internal sealed class NullServiceLocator : IServiceLocator
    {
        public static NullServiceLocator Instance = new NullServiceLocator();

        private NullServiceLocator()
        {
        }

        public object GetInstance(Type service, string key)
        {
            throw new InvalidOperationException(string.Format("Could not locate an instance for type '{0}' and key {1}.", service, key));
        }

        public IEnumerable<object> GetAllInstances(Type service)
        {
            return Enumerable.Empty<object>();
        }
    }
}
