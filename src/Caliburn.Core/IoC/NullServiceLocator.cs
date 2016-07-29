using System;
using System.Collections;
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
            return null;
        }

        public IEnumerable GetAllInstances(Type service)
        {
            return Enumerable.Empty<object>();
        }

        public TService GetInstance<TService>(string key)
        {
            return default(TService);
        }

        public IEnumerable<TService> GetAllInstances<TService>()
        {
            return Enumerable.Empty<TService>();
        }
    }
}
