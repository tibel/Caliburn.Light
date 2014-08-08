﻿using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;
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
        /// The phone application service.
        /// </summary>
        protected PhoneApplicationService PhoneService { get; private set; }

        /// <summary>
        /// The root frame used for navigation.
        /// </summary>
        protected PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Initialize the framework.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            UIContext.Initialize(new ViewAdapter());

            try
            {
                TypeResolver.Reset();
                SelectAssemblies().ForEach(TypeResolver.AddAssembly);

                if (!UIContext.IsInDesignTool)
                {
                    Application = Application.Current;
                    PrepareApplication();
                }

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
            Application.UnhandledException += OnUnhandledException;

            PhoneService = new PhoneApplicationService();
            Application.ApplicationLifetimeObjects.Add(PhoneService);

            RootFrame = CreatePhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;
            RootFrame.Navigated += CheckForResetNavigation;
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

        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
            if (!ReferenceEquals(Application.RootVisual, RootFrame))
                Application.RootVisual = RootFrame;
        }

        private void CheckForResetNavigation(object sender, NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Reset)
                RootFrame.Navigated += ClearBackStackAfterReset;
        }

        private void ClearBackStackAfterReset(object sender, NavigationEventArgs e)
        {
            RootFrame.Navigated -= ClearBackStackAfterReset;

            if (e.NavigationMode != NavigationMode.New && e.NavigationMode != NavigationMode.Refresh)
                return;

            while (RootFrame.RemoveBackEntry() != null) { }
        }

        /// <summary>
        /// Creates the root frame used by the application.
        /// </summary>
        /// <returns>The frame.</returns>
        protected virtual PhoneApplicationFrame CreatePhoneApplicationFrame()
        {
            return new PhoneApplicationFrame();
        }

        /// <summary>
        /// Override this to add custom behavior to execute after the application starts.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        protected virtual void OnStartup(object sender, StartupEventArgs e)
        {
        }

        /// <summary>
        /// Override this to add custom behavior for unhandled exceptions.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        protected virtual void OnUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
        }
    }
}
