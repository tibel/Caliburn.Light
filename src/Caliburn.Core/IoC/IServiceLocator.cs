using System;
using System.Collections;
using System.Collections.Generic;

namespace Caliburn.Light
{
    /// <summary>
    /// The generic Service Locator interface. 
    /// This interface is used to retrieve services (instances identified by type and optional name) from a container.
    /// </summary>
    public interface IServiceLocator
    {
        /// <summary>
        /// Requests an instance.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="key">The key.</param>
        /// <returns>The instance.</returns>
        object GetInstance(Type service, string key = null);

        /// <summary>
        /// Requests all instances of a given type.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns>All the instances or an empty enumerable if none are found.</returns>
        IEnumerable GetAllInstances(Type service);

        /// <summary>
        /// Requests an instance.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>The instance.</returns>
        TService GetInstance<TService>(string key = null);

        /// <summary>
        /// Gets all instances of a particular type.
        /// </summary>
        /// <typeparam name="TService">The type to resolve.</typeparam>
        /// <returns>The resolved instances.</returns>
        IEnumerable<TService> GetAllInstances<TService>();
    }
}
