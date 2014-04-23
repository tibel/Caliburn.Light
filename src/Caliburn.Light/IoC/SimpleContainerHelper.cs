using System;
using System.Collections.Generic;
using System.Linq;

namespace Caliburn.Light
{
    /// <summary>
    /// Extension methods for the <see cref="SimpleContainer"/>.
    /// </summary>
    public static class SimpleContainerHelper
    {
        /// <summary>
        /// Registers an instance with the container.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="instance">The instance.</param>
        /// <returns>The container.</returns>
        public static SimpleContainer RegisterInstance<TService>(this SimpleContainer container, TService instance)
        {
            container.RegisterInstance(typeof(TService), null, instance);
            return container;
        }

        /// <summary>
        /// Registers the class so that it is created once, on first request, and the same instance is returned to all requestors thereafter.
        /// </summary>
        /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="key">The key.</param>
        /// <returns>The container.</returns>
        public static SimpleContainer RegisterSingleton<TImplementation>(this SimpleContainer container, string key = null)
        {
            return RegisterSingleton<TImplementation, TImplementation>(container, key);
        }

        /// <summary>
        /// Registers the class so that it is created once, on first request, and the same instance is returned to all requestors thereafter.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="key">The key.</param>
        /// <returns>The container.</returns>
        public static SimpleContainer RegisterSingleton<TService, TImplementation>(this SimpleContainer container, string key = null)
            where TImplementation : TService
        {
            container.RegisterSingleton(typeof(TService), key, typeof(TImplementation));
            return container;
        }

        /// <summary>
        /// Registers the class so that it is created once, on first request, and the same instance is returned to all requestors thereafter.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name = "container">The container.</param>
        /// <param name = "key">The key.</param>
        /// <param name = "handler">The handler.</param>
        public static SimpleContainer RegisterSingleton<TService>(this SimpleContainer container, string key, Func<SimpleContainer, TService> handler)
        {
            object singleton = null;
            container.RegisterHandler(typeof(TService), key, c => singleton ?? (singleton = handler(c)));
            return container;
        }

        /// <summary>
        /// Registers the class so that a new instance is created on each request.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="key">The key.</param>
        /// <returns>The container.</returns>
        public static SimpleContainer RegisterPerRequest<TService, TImplementation>(this SimpleContainer container, string key = null)
            where TImplementation : TService
        {
            container.RegisterPerRequest(typeof(TService), key, typeof(TImplementation));
            return container;
        }

        /// <summary>
        /// Registers the class so that a new instance is created on each request.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="key">The key.</param>
        /// <returns>The container.</returns>
        public static SimpleContainer RegisterPerRequest<TService>(this SimpleContainer container, string key = null)
        {
            return RegisterPerRequest<TService, TService>(container, key);
        }

        /// <summary>
        /// Registers the class so that a new instance is created on each request.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name = "container">The container.</param>
        /// <param name = "key">The key.</param>
        /// <param name = "handler">The handler.</param>
        public static SimpleContainer RegisterPerRequest<TService>(this SimpleContainer container, string key, Func<SimpleContainer, TService> handler)
        {
            container.RegisterHandler(typeof(TService), key, c => handler(c));
            return container;
        }

        /// <summary>
        /// Requests an instance.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="key">The key.</param>
        /// <returns>The instance.</returns>
        public static TService GetInstance<TService>(this SimpleContainer container, string key = null)
        {
            return (TService)container.GetInstance(typeof(TService), key);
        }

        /// <summary>
        /// Gets all instances of a particular type.
        /// </summary>
        /// <typeparam name="TService">The type to resolve.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The resolved instances.</returns>
        public static IEnumerable<TService> GetAllInstances<TService>(this SimpleContainer container)
        {
            return container.GetAllInstances(typeof(TService)).Cast<TService>();
        }
    }
}
