using System;
using System.Collections.Generic;

namespace Caliburn.Light
{
    /// <summary>
    /// Extensions for <see cref="IServiceLocator"/> for WPF.
    /// </summary>
    public static class ServiceLocatorExtensions
    {
        /// <summary>
        /// Locates the view model, locates the associate view, binds them and shows it as the root view.
        /// </summary>
        /// <typeparam name="TViewModel">The view model type.</typeparam>
        /// <param name="serviceLocator">The service locator.</param>
        /// <param name="settings">The optional window settings.</param>
        public static void ShowWindowFor<TViewModel>(this IServiceLocator serviceLocator, IDictionary<string, object> settings = null)
        {
            ShowWindowFor(serviceLocator, typeof(TViewModel), settings);
        }

        /// <summary>
        /// Locates the view model, locates the associate view, binds them and shows it as the root view.
        /// </summary>
        /// <param name="serviceLocator">The service locator.</param>
        /// <param name="viewModelType">The view model type.</param>
        /// <param name="settings">The optional window settings.</param>
        public static void ShowWindowFor(this IServiceLocator serviceLocator, Type viewModelType, IDictionary<string, object> settings = null)
        {
            if (serviceLocator is null)
                throw new ArgumentNullException(nameof(serviceLocator));
            if (viewModelType is null)
                throw new ArgumentNullException(nameof(viewModelType));

            var windowManager = serviceLocator.GetInstance<IWindowManager>();
            if (windowManager is null)
                throw new InvalidOperationException("Could not resolve type 'IWindowManager'.");

            var viewModel = serviceLocator.GetInstance(viewModelType);
            if (viewModel is null)
                throw new InvalidOperationException(string.Format("Could not resolve type '{0}'.", viewModelType));

            windowManager.ShowWindow(viewModel, null, settings);
        }
    }
}
