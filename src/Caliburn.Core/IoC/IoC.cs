using System;
using System.Collections.Generic;
using System.Linq;

namespace Caliburn.Light
{
    /// <summary>
    /// Used by the framework to pull instances from an IoC container and to inject dependencies into certain existing classes.
    /// </summary>
    public static class IoC
    {
        private static IServiceLocator _serviceLocator;

        /// <summary>
        /// Initializes with the specified service locator.
        /// </summary>
        /// <param name="serviceLocator">The service locator.</param>
        public static void Initialize(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }

        static IServiceLocator ServiceLocator
        {
            get { return _serviceLocator ?? NullServiceLocator.Instance; }
        }

        /// <summary>
        /// Gets an instance from the container.
        /// </summary>
        /// <param name="service">The type to resolve</param>
        /// <param name="key">The key to look up.</param>
        /// <returns>The resolved instance.</returns>
        public static object GetInstance(Type service, string key = null)
        {
            return ServiceLocator.GetInstance(service, key);
        }

        /// <summary>
        /// Gets an instance from the container.
        /// </summary>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <param name="key">The key to look up.</param>
        /// <returns>The resolved instance.</returns>
        public static T GetInstance<T>(string key = null)
        {
            return (T)GetInstance(typeof(T), key);
        }

        /// <summary>
        /// Gets all instances of a particular type.
        /// </summary>
        /// <param name="service">The type to resolve.</param>
        /// <returns>The resolved instances.</returns>
        public static IEnumerable<object> GetAllInstances(Type service)
        {
            return ServiceLocator.GetAllInstances(service);
        }

        /// <summary>
        /// Gets all instances of a particular type.
        /// </summary>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <returns>The resolved instances.</returns>
        public static IEnumerable<T> GetAllInstances<T>()
        {
            return GetAllInstances(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// Passes an existing instance to the IoC container to enable dependencies to be injected.
        /// </summary>
        public static void InjectProperties(object instance)
        {
            ServiceLocator.InjectProperties(instance);
        }
    }
}
