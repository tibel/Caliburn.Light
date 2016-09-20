using System;
using System.Collections;
using System.Collections.Generic;

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
            LogManager.GetLogger(typeof(IoC)).Info("Initialize");

            _serviceLocator = serviceLocator ?? NullServiceLocator.Instance;
        }

        /// <summary>
        /// Gets an instance from the container.
        /// </summary>
        /// <param name="service">The type to resolve</param>
        /// <param name="key">The key to look up.</param>
        /// <returns>The resolved instance.</returns>
        public static object GetInstance(Type service, string key = null)
        {
            return _serviceLocator.GetInstance(service, key);
        }

        /// <summary>
        /// Gets an instance from the container.
        /// </summary>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <param name="key">The key to look up.</param>
        /// <returns>The resolved instance.</returns>
        public static T GetInstance<T>(string key = null)
        {
            return _serviceLocator.GetInstance<T>(key);
        }

        /// <summary>
        /// Gets all instances of a particular type.
        /// </summary>
        /// <param name="service">The type to resolve.</param>
        /// <returns>The resolved instances.</returns>
        public static IEnumerable GetAllInstances(Type service)
        {
            return _serviceLocator.GetAllInstances(service);
        }

        /// <summary>
        /// Gets all instances of a particular type.
        /// </summary>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <returns>The resolved instances.</returns>
        public static IEnumerable<T> GetAllInstances<T>()
        {
            return _serviceLocator.GetAllInstances<T>();
        }

        /// <summary>
        /// Ensures that the given <paramref name="instance"/> is not null and throws an exception otherwise.
        /// </summary>
        /// <typeparam name="TService">The instance type.</typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="errorMessage">The error message to use if the <paramref name="instance"/> is null.</param>
        /// <returns>Returns the <paramref name="instance"/>.</returns>
        public static TService EnsureNotNull<TService>(this TService instance, string errorMessage)
            where TService : class
        {
            if (instance == null)
                throw new InvalidOperationException(errorMessage);

            return instance;
        }

        /// <summary>
        /// Ensures that the given <paramref name="instance"/> is not null and throws an exception otherwise.
        /// </summary>
        /// <typeparam name="TService">The instance type.</typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="errorMessageFunc">A function that returns the error message to use if the <paramref name="instance"/> is null.</param>
        /// <returns>Returns the <paramref name="instance"/>.</returns>
        public static TService EnsureNotNull<TService>(this TService instance, Func<string> errorMessageFunc)
            where TService : class
        {
            if (errorMessageFunc == null)
                throw new ArgumentNullException(nameof(errorMessageFunc));

            if (instance == null)
                throw new InvalidOperationException(errorMessageFunc());

            return instance;
        }
    }
}
