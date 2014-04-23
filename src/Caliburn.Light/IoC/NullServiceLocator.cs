using System;
using System.Collections.Generic;

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
            return Activator.CreateInstance(service);
        }

        public IEnumerable<object> GetAllInstances(Type service)
        {
            return new[] { GetInstance(service, null) };
        }

        public void InjectProperties(object instance)
        {
        }
    }
}
