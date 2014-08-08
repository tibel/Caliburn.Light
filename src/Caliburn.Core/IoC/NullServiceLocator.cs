using System;
using System.Collections.Generic;
using System.Reflection;

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
            var typeInfo = service.GetTypeInfo();
            if (typeInfo.IsAbstract || typeInfo.IsInterface)
                return new object[0];

            return new[] { Activator.CreateInstance(service) };
        }

        public void InjectProperties(object instance)
        {
        }
    }
}
