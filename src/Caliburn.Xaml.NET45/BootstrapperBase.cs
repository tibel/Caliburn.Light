using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Weakly;

namespace Caliburn.Light
{
    /// <summary>
    /// Inherit from this class in order to customize the configuration of the framework.
    /// </summary>
    public abstract class BootstrapperBase
    {
        private bool _isInitialized;

        /// <summary>
        /// The application.
        /// </summary>
        protected Application Application { get; private set; }

        /// <summary>
        /// Initialize the framework.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            try
            {
                TypeResolver.Reset();
                SelectAssemblies().ForEach(TypeResolver.AddAssembly);

                Application = Application.Current;
                if (Application != null)
                    PrepareApplication();
                else
                    UIContext.Initialize(new ViewAdapter());

                Configure();
            }
            catch
            {
                _isInitialized = false;
                throw;
            }
        }

        /// <summary>
        /// Provides an opportunity to hook into the application object.
        /// </summary>
        protected virtual void PrepareApplication()
        {
            Application.Startup += OnStartup;
            Application.DispatcherUnhandledException += (s, e) => OnUnhandledException(e);
            Application.Exit += (s, e) => OnExit(e);
        }

        /// <summary>
        /// Override to configure the framework and setup your IoC container.
        /// </summary>
        protected virtual void Configure()
        {
        }

        /// <summary>
        /// Override to tell the framework where to find assemblies to inspect for views, etc.
        /// </summary>
        /// <returns>A list of assemblies to inspect.</returns>
        protected virtual IEnumerable<Assembly> SelectAssemblies()
        {
            return new[] {GetType().Assembly};
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            UIContext.Initialize(new ViewAdapter());
            OnStartup(e);
        }

        /// <summary>
        /// Override this to add custom behavior to execute when the application starts.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected virtual void OnStartup(StartupEventArgs e)
        {
        }

        /// <summary>
        /// Override this to add custom behavior to execute before the application shuts down.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected virtual void OnExit(ExitEventArgs e)
        {
        }

        /// <summary>
        /// Override this to add custom behavior for unhandled exceptions.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected virtual void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
        }

        /// <summary>
        /// Locates the view model, locates the associate view, binds them and shows it as the root view.
        /// </summary>
        /// <param name="viewModelType">The view model type.</param>
        /// <param name="settings">The optional window settings.</param>
        protected void DisplayRootViewFor(Type viewModelType, IDictionary<string, object> settings = null)
        {
            var windowManager = IoC.GetInstance<IWindowManager>();
            var viewModel = IoC.GetInstance(viewModelType);
            windowManager.ShowWindow(viewModel, null, settings);
        }

        /// <summary>
        /// Locates the view model, locates the associate view, binds them and shows it as the root view.
        /// </summary>
        /// <typeparam name="TViewModel">The view model type.</typeparam>
        /// <param name="settings">The optional window settings.</param>
        protected void DisplayRootViewFor<TViewModel>(IDictionary<string, object> settings = null)
        {
            DisplayRootViewFor(typeof (TViewModel), settings);
        }
    }
}
