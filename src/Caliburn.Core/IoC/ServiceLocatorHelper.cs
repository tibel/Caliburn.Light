using System.Collections.Generic;
using System.Linq;

namespace Caliburn.Light
{
    /// <summary>
    /// Extension methods for the <see cref="IServiceLocator"/>.
    /// </summary>
    public static class ServiceLocatorHelper
    {
        /// <summary>
        /// Requests an instance.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="key">The key.</param>
        /// <returns>The instance.</returns>
        public static TService GetInstance<TService>(this IServiceLocator container, string key = null)
        {
            return (TService)container.GetInstance(typeof(TService), key);
        }

        /// <summary>
        /// Gets all instances of a particular type.
        /// </summary>
        /// <typeparam name="TService">The type to resolve.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The resolved instances.</returns>
        public static IEnumerable<TService> GetAllInstances<TService>(this IServiceLocator container)
        {
            return container.GetAllInstances(typeof(TService)).Cast<TService>();
        }
    }
}
