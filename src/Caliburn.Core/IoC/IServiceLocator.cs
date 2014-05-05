using System;
using System.Collections.Generic;

namespace Caliburn.Light
{
    /// <summary>
    /// 
    /// </summary>
    public interface IServiceLocator
    {
        /// <summary>
        /// Gets an instance by type and key.
        /// </summary>
        object GetInstance(Type service, string key);

        /// <summary>
        /// Gets all instances of a particular type.
        /// </summary>
        IEnumerable<object> GetAllInstances(Type service);

        /// <summary>
        /// Passes an existing instance to the IoC container to enable dependencies to be injected.
        /// </summary>
        void InjectProperties(object instance);
    }
}
