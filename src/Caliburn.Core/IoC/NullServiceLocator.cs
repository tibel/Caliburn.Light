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
            throw new InvalidOperationException("IoC is not initialized.");
        }

        public IEnumerable<object> GetAllInstances(Type service)
        {
            throw new InvalidOperationException("IoC is not initialized.");
        }

        public TService GetInstance<TService>(string key = null)
        {
            throw new InvalidOperationException("IoC is not initialized.");
        }

        public IEnumerable<TService> GetAllInstances<TService>()
        {
            throw new InvalidOperationException("IoC is not initialized.");
        }
    }
}
